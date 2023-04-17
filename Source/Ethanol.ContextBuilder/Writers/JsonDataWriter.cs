using Ethanol.ContextBuilder.Plugins.Attributes;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading;
using NLog;
using YamlDotNet.Serialization;
using Ethanol.ContextBuilder.Context;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Microsoft.Extensions.Primitives;
using System.Collections.Generic;

namespace Ethanol.ContextBuilder.Writers
{
    /// <summary>
    /// Produces NDJSON output for arbitrary object type.
    /// </summary>
    [Plugin(PluginCategory.Writer, "JsonWriter", "Writes NDJSON formatted file for computed context.")]
    public abstract class JsonDataWriter : ContextWriter<object>
    {
        static protected readonly Logger __logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Creates a new <see cref="JsonDataWriter"/> object. 
        /// <para/>
        /// Arguments:
        /// <para/>
        /// file=OUTPUT_FILE specifies that the output will be written to OUTPUT_FILE instead to standard output.
        /// tcp=192.168.1.10:2941 for sending data via TCP connection.
        /// </summary>
        /// <param name="arguments">The arguments used in object creation.</param>
        /// <returns>The new  <see cref="JsonDataWriter"/> object. </returns>
        [PluginCreate]
        public static JsonDataWriter Create(Configuration configuration)
        {

            var tcpWriter = configuration.TcpConnection != null ? TcpWriter.CreateFromConnectionString(configuration.TcpConnection) : null;
            if (tcpWriter != null) return tcpWriter;
            else
            {
                var fileWriter = configuration.FileName != null ? File.CreateText(configuration.FileName) : System.Console.Out;
                return new FileWriter(fileWriter);
            }
        }

        public class Configuration
        {
            [PluginParameter(Name: "file", PluginParameterFlag.Optional, Description: "The file name with YAML data to write.")]
            [YamlMember(Alias = "file")]
            public string FileName { get; set; }
            [PluginParameter(Name: "tcp", PluginParameterFlag.Optional, Description: "The 'host:port' information for the tcp connection.")]
            [YamlMember(Alias = "tcp")]
            public string TcpConnection { get; set; }
        }

        protected JsonSerializerOptions _jsonOptions;

        JsonDataWriter()
        {
            _jsonOptions = new JsonSerializerOptions { };
            _jsonOptions.AddIPAddressConverter();
        }

        class FileWriter : JsonDataWriter
        {
            private readonly TextWriter _writer;
            
            /// <summary>
            /// Creates a JSON writer for the given <paramref name="writer"/>.
            /// </summary>
            /// <param name="writer">The text writer to produce the output.</param>
            public FileWriter(TextWriter writer)
            {
                _writer = writer;
            }
            /// <inheritdoc/>
            protected override void Close()
            {
                _writer.Close();
            }
            /// <inheritdoc/>
            protected override void Open()
            {
                // already opened.    
            }
            /// <inheritdoc/>
            protected override void Write(object value)
            {
                _writer.WriteLine(JsonSerializer.Serialize(value, _jsonOptions));
            }

            public override string ToString()
            {
                return $"{nameof(JsonDataWriter)} (Writer={_writer})";
            }
        }

        /// <summary>
        /// The tcp output writer allows to send records to a remote TCP server.
        /// <para/> 
        /// It implements a reconnect function.
        /// </summary>
        class TcpWriter : JsonDataWriter
        {
            IPEndPoint _endpoint;
            TcpClient _client;
            StreamWriter _writer;
            Task _writerTask;

            private BlockingCollection<string> _queue;
            TimeSpan _reconnectTime;
            int _reconnectAttempts;



            /// <summary>
            /// Initializes a new instance of the TcpWriter class with the specified endpoint and optional capacity.
            /// </summary>
            /// <param name="endpoint">The IPEndPoint object representing the remote endpoint to which the TCP connection will be established.</param>
            /// <param name="capacity">An optional parameter specifying the maximum capacity of the underlying BlockingCollection. The default value is 1024.</param>
            public TcpWriter(IPEndPoint endpoint, int capacity = 1024)
            {
                _endpoint = endpoint;
                _reconnectTime = TimeSpan.FromSeconds(3);
                _reconnectAttempts = 3;
                _queue = new BlockingCollection<string>(capacity);
            }

            /// <summary>
            /// Writes an object to the remote TCP server.
            /// </summary>
            /// <param name="value">The object to write.</param>
            /// <exception cref="System.IO.IOException">Thrown if the object cannot be written.</exception>
            protected override void Write(object value)
            {
                var stringValue = JsonSerializer.Serialize(value, _jsonOptions);
                _queue.Add(stringValue);
            }

            bool TryWriteInternal(IEnumerable<string> lines)
            {
                var attempts = _reconnectAttempts;
                while (!TryConnectInternal() && attempts-- > 0)
                {
                    Thread.Sleep(_reconnectTime);
                    __logger.Info($"Reconnect attempt: {_reconnectAttempts - attempts} of {_reconnectAttempts}.");
                }
                if (IsConnected)
                {
                    try
                    {
                        foreach (var line in lines)
                        {
                            _writer.WriteLine(line);
                            _writer.Flush();
                            __logger.Trace($"Sent: {line.Substring(0, 50)}...");
                        }
                        return true;
                    }
                    catch (SocketException e)
                    {
                        __logger.Error($"Cannot sent: {e.Message}.");
                        CloseConnectionInternal();
                        return false;
                    }
                }
                else
                {
                    __logger.Error($"Cannot sent: server is down.");
                    return false;
                }
            }

            bool IsConnected => _client?.Connected ?? false;

            void CloseConnectionInternal()
            {
                _writer?.Close();
                _writer = null;
                _client?.Close();
                _client = null;
            }

            private bool TryConnectInternal()
            {
                if (IsConnected)
                {
                    return true;
                }
                else
                {
                    __logger.Trace("Try to connect to the remote host {_endpoint}.");
                    CloseConnectionInternal();
                    try
                    {
                        _client = new TcpClient();
                        _client.Connect(_endpoint);
                        _writer = new StreamWriter(_client.GetStream());
                        __logger.Info($"Connected to remote TCP server: {_endpoint}.");
                        return true;
                    }
                    catch (SocketException e)
                    {
                        __logger.Error($"Cannot connect to {_endpoint}: {e.Message}.");
                        CloseConnectionInternal();
                        return false;
                    }
                }
            }

            // TODO:
            // how to take as many items as possible and try to write them in a block:
            // 
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
                            __logger.Error($"Cannot write objects.");
                        }
                    }
                }
            }

            protected override void Open()
            {
                if (_writerTask == null)
                {
                    __logger.Warn("Exexuting writer task.");
                    _writerTask = Task.Factory.StartNew(WriterTask);
                }
                else
                {
                    __logger.Warn("Writer task is already running.");
                }
            }
            /// <summary>
            /// Closes the connection to the remote TCP server.
            /// </summary>
            protected override void Close()
            {
                __logger.Trace("Close called.");
                _queue.CompleteAdding();
                __logger.Trace("Queue closed, waiting for writer task to finish.");
                Task.WaitAny(_writerTask);
                lock (this)
                {
                    CloseConnectionInternal();
                }
            }

            public static TcpWriter CreateFromConnectionString(string connectionString)
            {
                try
                {
                    __logger.Info($"TcpWriter Connection string = {connectionString}");
                    // Parse the connection string into an IPEndPoint object
                    var endpoint = IPEndPointResolver.GetIPEndPoint(connectionString);
                    __logger.Info($"TcpWriter Endpoint = {endpoint}");
                    // Create the writer
                    return new TcpWriter(endpoint);
                }
                catch (Exception ex)
                {
                    __logger.Error($"Cannot create TcpOutputWriter from '{connectionString}' string: {ex.Message}");
                    return null;
                }
            }
        }
    }
}
