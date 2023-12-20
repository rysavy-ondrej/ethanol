using Ethanol.DataObjects;
using Ethanol.ContextBuilder.Serialization;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Ethanol.ContextBuilder.Writers
{

    /// <summary>
    /// Represents a writer that outputs data in NDJSON (Newline Delimited JSON) format. This class provides mechanisms for writing the formatted data to either a file or a TCP connection.
    /// </summary>
    /// <remarks>
    /// This writer can be configured to direct its output to different destinations based on provided arguments. It supports writing to a file or sending the data over a TCP connection.
    /// The actual method for writing the data (file or TCP) is determined based on the provided configuration.
    /// </remarks>
    public abstract class JsonTargetHostContextWriter : ContextWriter<HostContext>
    {
        internal static JsonTargetHostContextWriter CreateFileWriter(TextWriter writer, string filePath, ILogger logger)
        {
            return new FileWriter(writer, filePath, logger);
        }

        internal static JsonTargetHostContextWriter CreateTcpWriter(IPEndPoint sendTo, ILogger logger)
        {
            return new TcpWriter(sendTo, logger);
        }

        /// <summary>
        /// Provides options for JSON serialization.
        /// </summary>
        protected JsonSerializerOptions _jsonOptions;
        private ILogger __logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonTargetHostContextWriter"/> class and sets up the JSON serialization options.
        /// </summary>
        protected JsonTargetHostContextWriter(ILogger logger)
        {
            __logger = logger;
            _jsonOptions = new JsonSerializerOptions { };
            _jsonOptions.AddIPAddressConverter();
        }

        /// <summary>
        /// Represents a writer that produces NDJSON output to a specific <see cref="TextWriter"/>.
        /// It extends the base <see cref="JsonTargetHostContextWriter"/> to specifically handle file-based writing operations.
        /// </summary>
        class FileWriter : JsonTargetHostContextWriter
        {
            private readonly TextWriter _writer;
            private readonly string _filePath;

            /// <summary>
            /// Initializes a new instance of the <see cref="FileWriter"/> class using the specified <paramref name="writer"/>.
            /// </summary>
            /// <param name="writer">The underlying text writer that will be used to write the NDJSON formatted output. This can be any writer derived from <see cref="TextWriter"/>, such as a <see cref="StreamWriter"/> for writing to files or console output.</param>
            public FileWriter(TextWriter writer, string filePath, ILogger logger) : base(logger)
            {
                _writer = writer ?? throw new ArgumentNullException(nameof(writer), "TextWriter cannot be null.");
                _filePath = filePath;
            }

            /// <summary>
            /// Closes the underlying text writer, effectively finishing the write operation.
            /// </summary>
            /// <remarks>
            /// This method ensures that any buffered data is correctly flushed to the output before closing the writer.
            /// </remarks>
            protected override void Close()
            {
                _writer.Close();
            }

            /// <summary>
            /// Opens the writer. Since the writer is typically opened outside this class, this method does not perform any operation.
            /// </summary>
            /// <remarks>
            /// In the context of the <see cref="FileWriter"/>, the writer is already opened at the time of instantiation, hence this method is left intentionally empty.
            /// </remarks>
            protected override void Open()
            {
                // already opened.    
            }

            /// <summary>
            /// Writes the provided <paramref name="value"/> in NDJSON format to the underlying writer.
            /// </summary>
            /// <param name="value">The value to be serialized and written.</param>
            /// <remarks>
            /// This method uses JSON serialization to convert the provided value into its NDJSON representation. If the value is null, no operation is performed.
            /// </remarks>
            protected override void Write(HostContext value)
            {
                if (value == null) return;
                var valueType = value.GetType();
                _writer.WriteLine(JsonSerializer.Serialize(value, valueType, _jsonOptions));
            }

            /// <summary>
            /// Returns a string representation of the <see cref="FileWriter"/> class, indicating the type of writer in use.
            /// </summary>
            /// <returns>A string representation of the current instance.</returns>
            public override string ToString()
            {
                var file = _filePath ?? "stdout";
                return $"{nameof(JsonTargetHostContextWriter)}({file})";
            }
        }


        /// <summary>
        /// The `TcpWriter` class provides functionality to write data records to a remote TCP server.
        /// It offers a built-in reconnection mechanism to handle transient connection issues.
        /// </summary>
        class TcpWriter : JsonTargetHostContextWriter
        {
            // Fields used for managing the TCP connection and writing records.
            private IPEndPoint _endpoint;
            private TcpClient _client;
            private StreamWriter _writer;
            private Task _writerTask;
            private BlockingCollection<string> _queue;
            private TimeSpan _reconnectTime;
            private int _reconnectAttempts;

            /// <summary>
            /// Initializes a new instance of the `TcpWriter` class targeting the specified endpoint.
            /// It optionally accepts a capacity for its internal buffer to control data flow.
            /// </summary>
            /// <param name="endpoint">Specifies the remote server's endpoint (host and port).</param>
            /// <param name="capacity">The maximum capacity for the internal buffering mechanism. Defaults to 1024 records.</param>

            public TcpWriter(IPEndPoint endpoint, ILogger logger) : base(logger)
            {
                _endpoint = endpoint;
                _reconnectTime = TimeSpan.FromSeconds(3);
                _reconnectAttempts = 3;
                _queue = new BlockingCollection<string>(1024);
            }

            /// <summary>
            /// Serializes and queues an object for writing to the remote TCP server.
            /// </summary>
            /// <param name="value">The object to be serialized and sent.</param>
            /// <exception cref="System.IO.IOException">This exception is thrown if there's a problem serializing or queuing the object.</exception>

            protected override void Write(HostContext value)
            {
                var stringValue = JsonSerializer.Serialize(value, _jsonOptions);
                _queue.Add(stringValue);
            }

            /// <summary>
            /// Attempts to write a collection of lines to the connected TCP server.
            /// If the connection is lost, the method will attempt to reconnect for a specified number of times.
            /// </summary>
            /// <param name="lines">The collection of strings (lines) to be written to the TCP server.</param>
            /// <returns>
            /// Returns <c>true</c> if all lines are successfully written to the server; 
            /// otherwise returns <c>false</c> if there's a connection issue or an error during writing.
            /// </returns>
            /// <remarks>
            /// This method employs a reconnection strategy where, if the initial attempt to write fails due to 
            /// connection issues, it will try to re-establish the connection for a predefined number of times 
            /// (defined by the <c>_reconnectAttempts</c> field) with a sleep interval between each attempt 
            /// (defined by the <c>_reconnectTime</c> field). This helps in ensuring data transmission even in 
            /// scenarios with intermittent network issues. If all reconnection attempts fail or a non-recoverable 
            /// error occurs, the method will return <c>false</c>.
            /// </remarks>
            bool TryWriteInternal(IEnumerable<string> lines)
            {
                var attempts = _reconnectAttempts;
                while (!TryConnectInternal() && attempts-- > 0)
                {
                    Thread.Sleep(_reconnectTime);
                    __logger?.LogInformation($"Reconnect attempt: {_reconnectAttempts - attempts} of {_reconnectAttempts}.");
                }
                if (IsConnected)
                {
                    try
                    {
                        foreach (var line in lines)
                        {
                            _writer.WriteLine(line);
                            _writer.Flush();
                            __logger?.LogTrace($"Sent: {line.Substring(0, 50)}...");
                        }
                        return true;
                    }
                    catch (SocketException e)
                    {
                        __logger?.LogError($"Cannot sent: {e.Message}.");
                        CloseConnectionInternal();
                        return false;
                    }
                }
                else
                {
                    __logger?.LogError($"Cannot sent: server is down.");
                    return false;
                }
            }

            /// <summary>
            /// Gets a value indicating whether the TcpClient is currently connected to the remote endpoint.
            /// </summary>
            /// <value>
            /// <c>true</c> if the TcpClient is connected; otherwise, <c>false</c>.
            /// </value>
            private bool IsConnected => _client?.Connected ?? false;

            /// <summary>
            /// Closes the active connection and releases associated resources.
            /// </summary>
            /// <remarks>
            /// This method ensures that the StreamWriter is closed (if instantiated) 
            /// and both the StreamWriter and TcpClient are set to <c>null</c> to indicate 
            /// they are no longer in use.
            /// </remarks>
            private void CloseConnectionInternal()
            {
                _writer?.Close();
                _writer = null;
                _client?.Close();
                _client = null;
            }

            /// <summary>
            /// Attempts to establish a connection to the specified remote endpoint.
            /// </summary>
            /// <remarks>
            /// If already connected to the remote host, the method immediately returns <c>true</c>. 
            /// If not connected, it will first close any existing resources and then try to establish 
            /// a new connection. If the connection is successful, a new StreamWriter is instantiated 
            /// for the TcpClient's stream.
            /// 
            /// Any encountered SocketExceptions during the connection attempt are logged and 
            /// resources are cleaned up accordingly.
            /// </remarks>
            /// <returns>
            /// <c>true</c> if the connection is established successfully; otherwise, <c>false</c>.
            /// </returns>
            private bool TryConnectInternal()
            {
                if (IsConnected)
                {
                    return true;
                }
                else
                {
                    __logger?.LogTrace($"Try to connect to the remote host {_endpoint}.");
                    CloseConnectionInternal();
                    try
                    {
                        _client = new TcpClient();
                        _client.Connect(_endpoint);
                        _writer = new StreamWriter(_client.GetStream());
                        __logger?.LogInformation($"Connected to remote TCP server: {_endpoint}.");
                        return true;
                    }
                    catch (SocketException e)
                    {
                        __logger?.LogError($"Cannot connect to {_endpoint}: {e.Message}.");
                        CloseConnectionInternal();
                        return false;
                    }
                }
            }

            /// <summary>
            /// Continuously processes items from the underlying queue and sends them to the remote server.
            /// </summary>
            /// <remarks>
            /// The method runs in a loop until the queue is marked as completed. In each iteration, it tries 
            /// to take as many items as possible from the queue and accumulate them in a list. It then attempts 
            /// to write these items to the remote server using the <see cref="TryWriteInternal"/> method.
            /// 
            /// The method uses a lock to ensure thread-safety while sending data, ensuring that multiple 
            /// threads do not simultaneously attempt to send data over the same connection. If writing the 
            /// items to the remote server fails, an error is logged.
            /// </remarks>
            void WriterTask()
            {
                while (!_queue.IsCompleted)
                {
                    var itemsToSend = new List<string>();
                    var value = _queue.Take();
                    itemsToSend.Add(value);
                    while (_queue.TryTake(out value)) itemsToSend.Add(value);

                    lock (this)
                    {
                        if (TryWriteInternal(itemsToSend) == false)
                        {
                            __logger?.LogError($"Cannot write objects.");
                        }
                    }
                }
            }

            /// <summary>
            /// Starts the task that continually reads records from the queue and attempts to send them to the server.
            /// </summary>
            protected override void Open()
            {
                if (_writerTask == null)
                {
                    __logger?.LogWarning("Exexuting writer task.");
                    _writerTask = Task.Factory.StartNew(WriterTask);
                }
                else
                {
                    __logger?.LogWarning("Writer task is already running.");
                }
            }
            /// <summary>
            /// Closes and cleans up the connection and resources associated with the `TcpWriter`.
            /// </summary>
            protected override void Close()
            {
                __logger?.LogTrace("Close called.");
                _queue.CompleteAdding();
                __logger?.LogTrace("Queue closed, waiting for writer task to finish.");
                Task.WaitAny(_writerTask);
                lock (this)
                {
                    CloseConnectionInternal();
                }
            }

            public override string ToString()
            {
                return $"{nameof(JsonTargetHostContextWriter)}(tcp={_endpoint}))";
            }

            /// <summary>
            /// Creates a `TcpWriter` instance based on a connection string representing the remote endpoint.
            /// </summary>
            /// <param name="connectionString">A string representing the remote server's endpoint (e.g., "192.168.1.10:1234").</param>
            /// <returns>A `TcpWriter` instance if the connection string is valid; otherwise, returns null.</returns>
            public static TcpWriter CreateFromConnectionString(string connectionString, ILogger logger)
            {
                try
                {
                    logger?.LogInformation($"TcpWriter Connection string = {connectionString}");
                    // Parse the connection string into an IPEndPoint object
                    var endpoint = IPEndPointResolver.GetIPEndPoint(connectionString);
                    logger?.LogInformation($"TcpWriter Endpoint = {endpoint}");
                    // Create the writer
                    return new TcpWriter(endpoint, logger);
                }
                catch (Exception ex)
                {
                    logger?.LogError($"Cannot create TcpOutputWriter from '{connectionString}': {ex.Message}");
                    return null;
                }
            }


        }
    }
}
