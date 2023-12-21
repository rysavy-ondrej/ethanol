using Ethanol.ContextBuilder.Context;
using Ethanol.ContextBuilder.Readers.DataObjects;
using Ethanol.ContextBuilder.Serialization;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Ethanol.ContextBuilder.Readers
{
    /// <summary>
    /// Provides a reader to process flow data from the ipfixcol tool's JSON output.
    /// </summary>
    /// <remarks>
    /// The class supports two primary modes of operation:
    /// <list type="number">
    /// <item>Reading data from a file through the derived <see cref="FileReader"/> class.</item>
    /// <item>Continuously fetching data from an open TCP socket using the derived <see cref="TcpReader"/> class.</item>
    /// </list>
    /// </remarks>
    abstract class IpfixcolJsonReader : BaseFlowReader<IpFlow>

    {
        /// <summary>
        /// Logger instance for the class to record events and issues.
        /// </summary>
        protected ILogger _logger;
        /// <summary>
        /// Represents a reader for Ipfixcol JSON data.
        /// </summary>
        protected readonly JsonReaderDeserializer<IpfixcolEntry> _deserializer;

        /// <summary>
        /// Options used for JSON serialization processes.
        /// </summary>
        protected readonly JsonSerializerOptions _serializerOptions;

        public static IpfixcolJsonReader CreateTcpReader(IPEndPoint listenAt, ILogger logger)
        {
            return new TcpReader(listenAt, logger);
        }

        public static IpfixcolJsonReader CreateFileReader(TextReader reader, string filePath, ILogger logger)
        {
            return new FileReader(reader, filePath, logger);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IpfixcolJsonReader"/> class.
        /// </summary>
        protected IpfixcolJsonReader(ILogger logger)
        {
            _serializerOptions = new JsonSerializerOptions();
            _serializerOptions.Converters.Add(new DateTimeJsonConverter());
            _logger = logger;
            _deserializer = new JsonReaderDeserializer<IpfixcolEntry>(x => x.ToFlow(), logger);
        }

        class FileReader : IpfixcolJsonReader
        {
            private readonly string _filePath;
            private readonly TextReader _reader;

            /// <summary>
            /// Initializes the reader with underlying <see cref="TextReader"/>.
            /// </summary>
            /// <param name="reader">The text reader device (input file or standard input).</param>
            public FileReader(TextReader reader, string filePath, ILogger logger) : base(logger)
            {
                _filePath = filePath;
                _reader = reader;
            }

            /// <inheritdoc/>
            protected override Task OpenAsync()
            {
                return Task.CompletedTask;
            }

            /// <inheritdoc/>
            protected override async Task<IpFlow> ReadAsync(CancellationToken ct)
            {
                while (!ct.IsCancellationRequested)
                {
                    var line = await _deserializer.ReadJsonStringAsync(_reader, ct);
                    
                    // end of file?
                    if (line == null) return null;
                    
                    if (_deserializer.TryDeserializeFlow(line, out var flow))
                    {
                        return flow;
                    }
                    else
                    {                        
                        continue;
                    }
                }
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
                return $"{nameof(IpfixcolJsonReader)}({file})";
            }
        }
        class TcpReader : IpfixcolJsonReader
        {
            private readonly TcpJsonServer<IpfixcolEntry> _reader;
            public TcpReader(IPEndPoint endPoint, ILogger logger) : base(logger)
            {
                _reader = new TcpJsonServer<IpfixcolEntry>(endPoint, _deserializer, logger);
            }
            public override string ToString()
            {
                return $"{nameof(IpfixcolJsonReader)}(tcp={_reader.Endpoint})";
            }

            protected override Task CloseAsync()
            {
                return _reader.CloseAsync();
            }

            protected override Task OpenAsync()
            {
                return _reader.OpenAsync();
            }

            protected override Task<IpFlow> ReadAsync(CancellationToken ct)
            {
                return _reader.ReadAsync(ct);
            }
        }
    }
}
