using Ethanol.DataObjects;
using Ethanol.ContextBuilder.Readers.DataObjects;
using Ethanol.ContextBuilder.Serialization;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Ethanol.ContextBuilder.Readers
{
    /// <summary>
    /// Provides a reader to process flow data from the flowmonexp5 tool's JSON output. Note that in this specific export format,
    /// each flow is depicted as an individual JSON object, distinct from standard NDJSON or a conventional JSON object array.
    /// </summary>
    /// <remarks>
    /// The class supports two primary modes of operation:
    /// <list type="number">
    /// <item>Reading data from a file through the derived <see cref="FileReader"/> class.</item>
    /// <item>Continuously fetching data from an open TCP socket using the derived <see cref="TcpReader"/> class.</item>
    /// </list>
    /// </remarks>
    abstract class FlowmonJsonReader : BaseFlowReader<IpFlow>
    {
        /// <summary>
        /// Logger instance for the class to record events and issues.
        /// </summary>
        protected readonly ILogger? _logger;
        /// <summary>
        /// Represents a Flowmon JSON reader that uses a JsonNdJsonDeserializer to deserialize Flowmonexp5Entry objects.
        /// </summary>
        protected readonly JsonReaderDeserializer<Flowmonexp5Entry> _deserializer;

        /// <summary>
        /// Options used for JSON serialization processes.
        /// </summary>
        protected readonly JsonSerializerOptions _serializerOptions;

        public static FlowmonJsonReader CreateTcpReader(IPEndPoint listenAt, ILogger? logger)
        {
            return new TcpReader(listenAt, logger);
        }

        public static FlowmonJsonReader CreateFileReader(TextReader reader, string? filePath, ILogger? logger)
        {
            return new FileReader(reader, filePath, logger);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FlowmonJsonReader"/> class.
        /// </summary>
        protected FlowmonJsonReader(ILogger? logger)
        {
            _serializerOptions = new JsonSerializerOptions();
            _serializerOptions.Converters.Add(new DateTimeJsonConverter());
            _logger = logger;
            _deserializer = new JsonReaderDeserializer<Flowmonexp5Entry>(x => x.ToFlow(), logger);
        }

        class FileReader : FlowmonJsonReader
        {
            private readonly TextReader _reader;
            private readonly string? _filePath;

            /// <summary>
            /// Initializes the reader with underlying <see cref="TextReader"/>.
            /// </summary>
            /// <param name="reader">The text reader device (input file or standard input).</param>
            public FileReader(TextReader reader, string? filePath, ILogger? logger) : base(logger)
            {
                _reader = reader;
                _filePath = filePath;
            }

            /// <inheritdoc/>
            protected override Task OpenAsync()
            {
                // nothing to do as we have already opened input stream provided by the reader
                return Task.CompletedTask;
            }

            /// <inheritdoc/>
            protected override async Task<IpFlow?> ReadAsync(CancellationToken ct)
            {
                while (!ct.IsCancellationRequested)
                {
                    var line = await _deserializer.ReadJsonStringAsync(_reader, ct);

                    // end of file?
                    if (line == null) return null;

                    if (_deserializer.TryDeserializeFlow(line, out var ipFlow))
                    {
                        return ipFlow;
                    }
                }
                ct.ThrowIfCancellationRequested();
                return null;
            }

            /// <inheritdoc/>
            protected override Task CloseAsync()
            {
                _reader.Close();
                return Task.CompletedTask;
            }

            public override string ToString()
            {
                var file = _filePath ?? "stdin";
                return $"{nameof(FlowmonJsonReader)}({file})";
            }
        }

        class TcpReader : FlowmonJsonReader
        {
            private readonly TcpJsonServer<Flowmonexp5Entry> _reader;
            public TcpReader(IPEndPoint endPoint, ILogger? logger) : base(logger)
            {
                _reader = new TcpJsonServer<Flowmonexp5Entry>(endPoint, _deserializer, logger);
            }
            public override string ToString()
            {
                return $"{nameof(FlowmonJsonReader)}(tcp={_reader.Endpoint})";
            }

            protected override Task CloseAsync()
            {
                return _reader.CloseAsync();
            }

            protected override Task OpenAsync()
            {
                return _reader.OpenAsync();
            }

            protected override Task<IpFlow?> ReadAsync(CancellationToken ct)
            {
                return _reader.ReadAsync(ct);
            }
        }
    }
}
