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
    abstract class IpfixcolJsonReader : BaseFlowReader<IpFlow>, IJsonFlowSerializerReader

    {
        /// <summary>
        /// Logger instance for the class to record events and issues.
        /// </summary>
        protected ILogger _logger;

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
        IpfixcolJsonReader(ILogger logger)
        {
            _serializerOptions = new JsonSerializerOptions();
            _serializerOptions.Converters.Add(new DateTimeJsonConverter());
            _logger = logger;
        }

        /// <summary>
        /// Attempts to deserialize the given input string into a FlowexpEntry instance.
        /// </summary>
        /// <param name="input">The JSON input string to deserialize.</param>
        /// <param name="entry">The resulting <see cref="IpfixcolEntry"/> object if the deserialization is successful.</param>
        /// <returns>True if deserialization is successful, otherwise false.</returns>
        bool TryDeserialize(string input, out IpfixcolEntry entry)
        {
            try
            {
                entry = JsonSerializer.Deserialize<IpfixcolEntry>(input, _serializerOptions);    
                return true;
            }
            catch (Exception e)
            {
                _logger?.LogWarning($"Cannot deserialize entry: {e.Message}");
                entry = default;
                return false;
            }
        }
        /// <summary>
        /// Tries to deserialize the input string into an <see cref="IpFlow"/> object.
        /// <p/>
        /// This method first deserialized ipfixcol entry and then maps it to IpFlow object.
        /// </summary>
        /// <param name="input">The input string to deserialize.</param>
        /// <param name="ipFlow">When this method returns, contains the deserialized <see cref="IpFlow"/> object if the deserialization was successful; otherwise, the default value.</param>
        /// <returns><c>true</c> if the deserialization was successful; otherwise, <c>false</c>.</returns>
        public bool TryDeserializeFlow(string input, out IpFlow ipFlow)
        {
            try
            {
                var entry = JsonSerializer.Deserialize<IpfixcolEntry>(input, _serializerOptions);
                ipFlow = entry.ToFlow();
                return true;
            }
            catch (Exception e)
            {
                _logger?.LogWarning($"Cannot deserialize flow: {e.Message}");
                ipFlow = default;
                return false;
            }
        }

        /// <summary>
        /// Reads a JSON string from the input stream. This method supports reading both NDJSON (Newline Delimited JSON) 
        /// where each line is a complete JSON object, and multi-line formatted JSON until it reaches the end of an object.
        /// </summary>
        /// <param name="inputStream">The TextReader stream to read the JSON string from.</param>
        /// <returns>A string representation of the JSON object, or null if the end of the file is reached or the content is whitespace.</returns>
        public async Task<string> ReadJsonStringAsync(TextReader inputStream, CancellationToken ct)
        {
            var buffer = new StringBuilder();

            while (true)
            {
                var line = (await inputStream.ReadLineAsync(ct))?.Trim();

                // End of file?
                if (line == null) break;

                buffer.AppendLine(line);

                // Check for the end of JSON object (either NDJSON or multiline JSON)
                if ((line.StartsWith("{") && line.EndsWith("}")) || line == "}") break;
            }
            var record = buffer.ToString().Trim();
            return string.IsNullOrWhiteSpace(record) ? null : record;
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
                    var line = await ReadJsonStringAsync(_reader, ct);
                    
                    // end of file?
                    if (line == null) return null;
                    
                    if (TryDeserializeFlow(line, out var flow))
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
            TcpJsonReaderInternal _reader;
            public TcpReader(IPEndPoint endPoint, ILogger logger) : base(logger)
            {
                _reader = new TcpJsonReaderInternal(this, endPoint, logger);
            }
            /// <summary>
            /// Creates a new TcpReader instance by parsing the provided connection string into an IPEndPoint.
            /// </summary>
            /// <param name="connectionString">The connection string in the format "host:port" to be used for establishing the TCP connection.</param>
            /// <returns>A new <see cref="TcpReader"/> instance for the given connection string, or null if an error occurs during creation.</returns>
            internal static TcpReader CreateFromConnectionString(string connectionString, ILogger logger)
            {
                try
                {
                    // Parse the connection string into an IPEndPoint object
                    var endpoint = IPEndPointResolver.GetIPEndPoint(connectionString);
                    logger?.LogInformation($"Listening for incoming tcp connection, endpoint={endpoint}.");
                    // Create the writer
                    return new TcpReader(endpoint, logger);
                }
                catch (Exception ex)
                {
                    logger?.LogError($"Error creating TcpReader from '{connectionString}' string: {ex.Message}");
                    return null;
                }
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
