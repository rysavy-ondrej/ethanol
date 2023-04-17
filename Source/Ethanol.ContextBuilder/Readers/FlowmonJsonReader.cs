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
using Ethanol.ContextBuilder.Context;
using Ethanol.ContextBuilder.Plugins.Attributes;
using Ethanol.ContextBuilder.Readers.DataObjects;
using YamlDotNet.Serialization;

namespace Ethanol.ContextBuilder.Readers
{
    /// <summary>
    /// Reads export from flowmonexp5 in JSON format. This format is specific by 
    /// representing each flow as an individual JSON object. It is not NDJSON nor 
    /// properly formatted array of JSON objects.
    /// </summary>
    [Plugin(PluginCategory.Reader, "FlowmonJson", "Reads JSON file with IPFIX data produced by flowmonexp5 tool.")]
    abstract class FlowexpJsonReader : FlowReader<IpFlow>
    {
        static protected NLog.Logger __logger = NLog.LogManager.GetCurrentClassLogger();
        protected readonly JsonSerializerOptions _serializerOptions;


        public class Configuration
        {
            [YamlMember(Alias = "file", Description = "The file name with JSON data to read. If not provided then STDIN will be used.")]
            public string FileName { get; set; }
            [PluginParameter(Name: "tcp", PluginParameterFlag.Optional, Description: "The host:port information for binding the tcp server. Use 0.0.0.0 to bind on all interfaces.")]
            [YamlMember(Alias = "tcp")]
            public string TcpConnection { get; set; }
        }

        /// <summary>
        /// Creates a new reader for the given arguments.
        /// </summary>
        /// <param name="arguments">Collection of arguments used to create a reader.</param>
        /// <returns>A new <see cref="FlowexpJsonReader"/> object.</returns>
        [PluginCreate]
        public static FlowexpJsonReader Create(Configuration configuration)
        {
            var tcpReader = configuration.TcpConnection != null ? TcpReader.CreateFromConnectionString(configuration.TcpConnection) : null;
            if (tcpReader != null) return tcpReader;
            else
            {
                var reader = configuration.FileName != null ? File.OpenText(configuration.FileName) : System.Console.In;
                return new FileReader(reader);
            }
        }

        FlowexpJsonReader()
        {
            _serializerOptions = new JsonSerializerOptions();
            _serializerOptions.Converters.Add(new DateTimeJsonConverter());
        }

        bool TryDeserialize(string input, out FlowexpEntry entry)
        {
            try
            {
                entry = JsonSerializer.Deserialize<FlowexpEntry>(input, _serializerOptions);
                return true;
            }
            catch (Exception e)
            {
                __logger.Warn($"Cannot deserialize entry: {e.Message}");
                entry = default;
                return false;
            }
        }
        /// <summary>
        /// Reads input record from Flowmon's specific JSON or NDJSON.
        /// </summary>
        private string ReadJsonString(TextReader inputStream)
        {
            var buffer = new StringBuilder();
            while (true)
            {
                var line = inputStream.ReadLine();

                // End of file?
                if (line == null) break;

                buffer.AppendLine(line);

                // Do we have NDJSON?
                if (line.StartsWith("{") && line.EndsWith("}")) break;

                // end of multiline JSON object?
                if (line.Trim() == "}") break;
            }
            var record = buffer.ToString().Trim();
            if (string.IsNullOrWhiteSpace(record))
            {
                return null;
            }
            else
            {
                return record;
            }
        }
        class FileReader : FlowexpJsonReader
        {
            private readonly TextReader _reader;

            /// <summary>
            /// Initializes the reader with underlying <see cref="TextReader"/>.
            /// </summary>
            /// <param name="reader">The text reader device (input file or standard input).</param>
            public FileReader(TextReader reader)
            {
                _reader = reader;

            }

            /// <inheritdoc/>
            protected override void Open()
            {
                // nothing to do, reader is always provided as open...
            }

            /// <inheritdoc/>
            protected override bool TryGetNextRecord(CancellationToken ct, out IpFlow ipFlow)
            {
                ipFlow = null;
                var line = ReadJsonString(_reader);
                if (line == null) return false;
                if (TryDeserialize(line, out var currentEntry))
                {
                    ipFlow = currentEntry.ToFlow();
                    return true;
                }
                return false;
            }
            /// <inheritdoc/>
            protected override void Close()
            {
                _reader.Close();
            }

            public override string ToString()
            {
                return $"{nameof(FlowexpJsonReader)}(Reader={_reader})";
            }
        }
        class TcpReader : FlowexpJsonReader
        {
            IPEndPoint _endpoint;
            private TcpListener _listener;
            private Task _mainLoopTask;
            private List<Task> _clientsTasks;
            private CancellationTokenSource _cancellationTokenSource;
            private BlockingCollection<IpFlow> _queue;

            public TcpReader(IPEndPoint endPoint)
            {
                _endpoint = endPoint;
                _cancellationTokenSource = new CancellationTokenSource();
                _queue = new BlockingCollection<IpFlow>();
                _clientsTasks = new List<Task>();
            }

            public async Task RunAsync(TcpListener listener, CancellationToken cancellation)
            {
                __logger.Info($"TCP server listening on {_endpoint}");
                try
                {
                    // Wait for incoming client connections
                    while (!cancellation.IsCancellationRequested)
                    {
                        var client = await listener.AcceptTcpClientAsync(cancellation);
                        __logger.Info($"TCP server accepts connection from {client?.Client.RemoteEndPoint}");
                        if (client != null)
                        {
                            var tcs = new TaskCompletionSource<object>();
                            _clientsTasks.Add(tcs.Task);
                            // Start as a background task... 
                            var _ = Task.Factory.StartNew(() =>
                            {
                                ReadInputData(client, cancellation);
                                _clientsTasks.Remove(tcs.Task);
                                tcs.SetResult(null);
                            },  cancellation);
                        }
                    }
                }
                catch (SocketException ex)
                {
                    __logger.Error($"Socket exception: {ex.Message}");
                }
                finally
                {
                    // Stop listening for client connections when done
                    listener.Stop();
                    
                }
            }

            private void ReadInputData(TcpClient client, CancellationToken cancellation)
            {
                // get the stream
                var stream = client.GetStream();
                // gets the reader from stream:
                var reader = new StreamReader(stream);
                string jsonString;
                while(!cancellation.IsCancellationRequested &&  (jsonString = ReadJsonString(reader)) != null)
                {
                    // read input tcp data and if suceffuly deserialized put the object in the buffer
                    // to be available to TryGetNextRecord method.
                    if (this.TryDeserialize(jsonString, out var currentEntry))
                    {
                        var ipflow = currentEntry.ToFlow();
                        _queue.Add(ipflow);
                    }
                    else
                    {
                        __logger.Error($"Cannot deserialize input {jsonString.Substring(0,1024)}.");
                    }
                }
                reader.Close();
                client.Close();
               
            }

            protected override void Open()
            {
                _listener = new TcpListener(_endpoint);
                _listener.Start();  
                _mainLoopTask = RunAsync(_listener, _cancellationTokenSource.Token);
            }

            protected override bool TryGetNextRecord(CancellationToken cancellation, out IpFlow record)
            {
                // Take does not wait in the case the input is completed.
                // In this case the return valus is null.
                record = _queue.Take(cancellation);
                return record != null;
            }

            protected override void Close()
            {
                _queue.CompleteAdding();
                var activeTasks = _clientsTasks.Append(_mainLoopTask).ToArray();
                _cancellationTokenSource.Cancel();
                Task.WhenAll(activeTasks);
            }

            internal static TcpReader CreateFromConnectionString(string connectionString)
            {
                try
                {
                    // Parse the connection string into an IPEndPoint object
                    var endpoint = IPEndPointResolver.GetIPEndPoint(connectionString);

                    // Create the writer
                    return new TcpReader(endpoint);
                }
                catch (Exception ex)
                {
                    __logger.Error($"Error creating TcpReader from '{connectionString}' string: {ex.Message}");
                    return null;
                }
            }
        }
    }
}
