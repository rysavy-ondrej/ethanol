using Ethanol.ContextBuilder.Context;
using Ethanol.ContextBuilder.Readers.DataObjects;
using Ethanol.ContextBuilder.Serialization;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
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
        protected ILogger _logger;

        /// <summary>
        /// Options used for JSON serialization processes.
        /// </summary>
        protected readonly JsonSerializerOptions _serializerOptions;

        public static FlowmonJsonReader CreateTcpReader(IPEndPoint listenAt, ILogger logger)
        {
            return new TcpReader(listenAt, logger);
        }

        public static FlowmonJsonReader CreateFileReader(TextReader reader, string filePath, ILogger logger)
        {
            return new FileReader(reader, filePath, logger);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FlowmonJsonReader"/> class.
        /// </summary>
        FlowmonJsonReader(ILogger logger)
        {
            _serializerOptions = new JsonSerializerOptions();
            _serializerOptions.Converters.Add(new DateTimeJsonConverter());
            _logger = logger;
        }

        /// <summary>
        /// Attempts to deserialize the given input string into a FlowexpEntry instance.
        /// </summary>
        /// <param name="input">The JSON input string to deserialize.</param>
        /// <param name="entry">The resulting <see cref="Flowmonexp5Entry"/> object if the deserialization is successful.</param>
        /// <returns>True if deserialization is successful, otherwise false.</returns>
        bool TryDeserialize(string input, out Flowmonexp5Entry entry)
        {
            try
            {
                entry = JsonSerializer.Deserialize<Flowmonexp5Entry>(input, _serializerOptions);
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
        /// Tries to deserialize the input string into an IpFlow object.
        /// </summary>
        /// <param name="input">The input string to deserialize.</param>
        /// <param name="ipFlow">When this method returns, contains the deserialized IpFlow object if the deserialization was successful, or the default IpFlow object if the deserialization failed.</param>
        /// <returns><c>true</c> if the deserialization was successful; otherwise, <c>false</c>.</returns>
        bool TryDeserializeFlow(string input, out IpFlow ipFlow)
        {
            try
            {
                var entry = JsonSerializer.Deserialize<Flowmonexp5Entry>(input, _serializerOptions);
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
        public static async Task<string> ReadJsonStringAsync(TextReader inputStream)
        {
            var buffer = new StringBuilder();

            while (true)
            {
                var line = (await inputStream.ReadLineAsync())?.Trim();

                // End of file?
                if (line == null) break;

                buffer.AppendLine(line);

                // Check for the end of JSON object (either NDJSON or multiline JSON)
                if ((line.StartsWith("{") && line.EndsWith("}")) || line == "}") break;
            }
            var record = buffer.ToString().Trim();
            return string.IsNullOrWhiteSpace(record) ? null : record;
        }

        class FileReader : FlowmonJsonReader
        {
            private readonly TextReader _reader;
            private readonly string _filePath;

            /// <summary>
            /// Initializes the reader with underlying <see cref="TextReader"/>.
            /// </summary>
            /// <param name="reader">The text reader device (input file or standard input).</param>
            public FileReader(TextReader reader, string filePath, ILogger logger) : base(logger)
            {
                _reader = reader;
                _filePath = filePath;
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
                    var line = await ReadJsonStringAsync(_reader);
                    
                    // end of file?
                    if (line == null) return null;
                    
                    if (TryDeserializeFlow(line, out var ipFlow))
                    {
                        return ipFlow;
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
        class TcpReader : FlowmonJsonReader
        {
            IPEndPoint _endpoint;
            private TcpListener _listener;
            private Task _mainLoopTask;
            private List<Task> _clientsTasks;
            private CancellationTokenSource _cancellationTokenSource;
            private BlockingCollection<IpFlow> _queue;

            public TcpReader(IPEndPoint endPoint, ILogger logger) : base(logger)
            {
                _endpoint = endPoint;
                _cancellationTokenSource = new CancellationTokenSource();
                _queue = new BlockingCollection<IpFlow>();
                _clientsTasks = new List<Task>();
            }

            public async Task RunAsync(TcpListener listener, CancellationToken cancellation)
            {
                _logger?.LogInformation($"TCP server listening on {_endpoint}");
                try
                {
                    // Wait for incoming client connections
                    while (!cancellation.IsCancellationRequested)
                    {
                        var client = await listener.AcceptTcpClientAsync(cancellation);
                        _logger?.LogInformation($"TCP server accepts connection from {client?.Client.RemoteEndPoint}");
                        if (client != null)
                        {
                            var tcs = new TaskCompletionSource<object>();
                            _clientsTasks.Add(tcs.Task);
                            // Start as a background task... 
                            var _ = Task.Factory.StartNew(async () =>
                            {
                                await ReadInputData(client, cancellation);
                                _clientsTasks.Remove(tcs.Task);
                                tcs.SetResult(null);
                                client.Dispose();
                            },  cancellation);
                        }
                    }
                }
                catch (SocketException ex)
                {
                    _logger?.LogError($"Socket exception: {ex.Message}");
                }
                finally
                {
                    // Stop listening for client connections when done
                    listener.Stop();
                    
                }
            }
            private async Task ReadInputData(TcpClient client, CancellationToken cancellation)
            {
                // get the stream
                var stream = client.GetStream();
                // gets the reader from stream:
                var reader = new StreamReader(stream);
                string jsonString;
                while(!cancellation.IsCancellationRequested &&  (jsonString = await ReadJsonStringAsync(reader)) != null)
                {
                    // read input tcp data and if suceffuly deserialized put the object in the buffer
                    // to be available to TryGetNextRecord method.
                    if (this.TryDeserializeFlow(jsonString, out var ipflow))
                    {
                        _queue.Add(ipflow);
                    }
                    else
                    {
                        _logger?.LogError($"Invalid input data: Cannot deserialize input {jsonString.Substring(0,64)}.");
                    }
                }
                reader.Close();
                client.Close();
               
            }

            protected override Task OpenAsync()
            {
                _listener = new TcpListener(_endpoint);
                _listener.Start();  
                _mainLoopTask = RunAsync(_listener, _cancellationTokenSource.Token);
                return Task.CompletedTask;
            }

            protected override Task<IpFlow> ReadAsync(CancellationToken cancellation)
            {
                // Take does not wait in the case the input is completed.
                // In this case the return valus is null.
                return Task.FromResult(_queue.Take(cancellation));
            }

            protected override Task CloseAsync()
            {
                _queue.CompleteAdding();
                var activeTasks = _clientsTasks.Append(_mainLoopTask).ToArray();
                _cancellationTokenSource.Cancel();
                return Task.WhenAll(activeTasks);
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
        }
    }
}
