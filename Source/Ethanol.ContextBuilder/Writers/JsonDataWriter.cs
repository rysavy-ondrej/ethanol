using Ethanol.ContextBuilder.Plugins.Attributes;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading;
using NLog;
using YamlDotNet.Serialization;

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

            TimeSpan _reconnectTime;
            int _reconnectAttempts;

            public TcpWriter(IPEndPoint endpoint)
            {
                _endpoint = endpoint;
                _reconnectTime = TimeSpan.FromSeconds(3);
                _reconnectAttempts = 3;
            }

            /// <summary>
            /// Writes an object to the remote TCP server.
            /// </summary>
            /// <param name="value">The object to write.</param>
            /// <exception cref="System.IO.IOException">Thrown if the object cannot be written.</exception>
            protected override void Write(object value)
            {
                var stringValue = JsonSerializer.Serialize(value, _jsonOptions);
                lock (this)
                {
                    if (TryWriteInternal(stringValue) == false)
                    {
                        __logger.Error($"Cannot write object.");
                    }
                }
            }

            bool TryWriteInternal(string line)
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
                        _writer.WriteLine(line);
                        _writer.Flush();
                        __logger.Trace($"Sent: {line.Substring(0, 50)}...");
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

            protected override void Open()
            {
                lock (this)
                {
                    TryConnectInternal();
                }
            }
            /// <summary>
            /// Closes the connection to the remote TCP server.
            /// </summary>
            protected override void Close()
            {
                lock (this)
                {
                    CloseConnectionInternal();
                }
            }

            public static TcpWriter CreateFromConnectionString(string connectionString)
            {
                try
                {
                    // Parse the connection string into an IPEndPoint object
                    var endpoint = IPEndPoint.Parse(connectionString);

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
