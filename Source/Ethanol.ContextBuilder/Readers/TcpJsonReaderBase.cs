using Ethanol.DataObjects;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Ethanol.ContextBuilder.Readers
{
    /// <summary>
    /// Base class for TCP JSON readers that read and deserialize JSON data from TCP clients.
    /// </summary>
    public class TcpJsonServer<TEntryType> : IDisposable, IAsyncDisposable
    {
        /// <summary>
        /// Gets or sets the IP endpoint for the TCP JSON reader.
        /// </summary>
        private IPEndPoint _endpoint;
        /// <summary>
        /// The TCP listener used for reading JSON data.
        /// </summary>
        private TcpListener? _listener;
        /// <summary>
        /// Represents the main loop task for reading JSON data over TCP.
        /// </summary>
        private Task? _mainLoopTask;
        /// <summary>
        /// The list of tasks representing the clients.
        /// </summary>
        private List<Task> _clientsTasks;
        /// <summary>
        /// The cancellation token source used for cancelling asynchronous operations.
        /// </summary>
        private CancellationTokenSource _cancellationTokenSource;
        /// <summary>
        /// A blocking collection to store IP flows. The records are read from the TCP stream and put in this queue. 
        /// The TryGetNextRecord method reads the records from this queue.
        /// </summary>
        private BlockingCollection<IpFlow> _queue;
        /// <summary>
        /// Gets or sets the logger for the TcpJsonReaderBase class.
        /// </summary>
        private ILogger? _logger;
        /// <summary>
        /// The deserializer used for JSON reading.
        /// </summary>
        private readonly JsonReaderDeserializer<TEntryType> _deserializer;
        /// <summary>
        /// Gets or sets a value indicating whether the object has been disposed.
        /// </summary>
        private bool _isDisposed;

        /// <summary>
        /// Gets the <see cref="System.Net.IPEndPoint"/> representing the TCP server endpoint.
        /// </summary>
        public IPEndPoint Endpoint => _endpoint;

        /// <summary>
        /// Represents a TCP JSON server that listens for incoming connections and processes JSON data.
        /// </summary>
        /// <param name="endPoint">The IP endpoint to bind the server to.</param>
        /// <param name="deserializer">The JSON deserializer used to deserialize JSON data.</param>
        /// <param name="logger">The logger used for logging server events.</param>
        public TcpJsonServer(IPEndPoint endPoint, JsonReaderDeserializer<TEntryType> deserializer, ILogger? logger)
        {
            _endpoint = endPoint;
            _cancellationTokenSource = new CancellationTokenSource();
            _queue = new BlockingCollection<IpFlow>();
            _clientsTasks = new List<Task>();
            _logger = logger;
            _deserializer = deserializer;
        }

        /// <summary>
        /// Executes the TCP server as an awaitable task. Each incoming client connection is handled in a separate task.
        /// </summary>
        /// <typeparam name="TResult">The type of the result produced by the task.</typeparam>
        private async Task RunAsync(TcpListener listener, CancellationToken cancellation)
        {
            _logger?.LogInformation($"TCP server listening on {_endpoint}");
            try
            {
                // Wait for incoming client connections
                while (!cancellation.IsCancellationRequested)
                {
                    var client = await listener.AcceptTcpClientAsync(cancellation);
                    _logger?.LogInformation($"TCP server {_endpoint} has accepted connection from {client?.Client.RemoteEndPoint}");
                    if (client != null)
                    {
                        var tcs = new TaskCompletionSource();
                        _clientsTasks.Add(tcs.Task);
                        // Start as a background task... 
                        var _ = Task.Run(async () =>
                        {
                            await ReadInputData(client, cancellation);
                            _clientsTasks.Remove(tcs.Task);
                            tcs.SetResult();
                            client.Dispose();
                        }, cancellation);
                    }
                }
            }
            catch (SocketException ex)
            {
                _logger?.LogError($"TCP server {_endpoint} socket exception: {ex.Message}");
            }
            catch (OperationCanceledException)
            {
                _logger?.LogInformation($"TCP server stopped listening on {_endpoint}.");
            }
            finally
            {
                // Stop listening for client connections when done
                listener.Stop();
            }
        }
        /// <summary>
        /// Asynchronously reads input data from a TCP client. This operation can be cancelled using <paramref name="cancellation"/> token.
        /// </summary>
        /// <param name="client">The TCP client to read data from.</param>
        /// <param name="cancellation">The cancellation token to stop the reading process.</param>
        /// <exception cref="OperationCanceledException">The operation was cancelled.</exception>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task ReadInputData(TcpClient client, CancellationToken cancellation)
        {
            var remoteEndpoint = client.Client.RemoteEndPoint;
            // get the stream
            var stream = client.GetStream();
            // gets the reader from stream:
            var reader = new StreamReader(stream);
            _logger?.LogInformation($"Tcp connection {_endpoint}<->{remoteEndpoint}: Start reading data.");
            try
            {
                while (!cancellation.IsCancellationRequested)
                {
                    var jsonString = await _deserializer.ReadJsonStringAsync(reader, cancellation);

                    // end of stream reached?
                    if (jsonString == null) break;

                    // ignore empty lines
                    if (jsonString == String.Empty) continue;

                    // read input tcp data and if suceffuly deserialized put the object in the buffer
                    // to be available to TryGetNextRecord method.
                    if (_deserializer.TryDeserializeFlow(jsonString, out var ipflow))
                    {
                        _queue.Add(ipflow);
                    }
                    else
                    {
                        _logger?.LogError($"Tcp reader {_endpoint} encountered invalid input data: {jsonString.Substring(0, 64)}.");
                    }
                }
            }
            catch (OperationCanceledException)
            {
                _logger?.LogInformation($"Tcp connection {_endpoint}<->{remoteEndpoint}: ReadInputData cancelled.");
            }
            finally
            {
                _logger?.LogInformation($"Tcp connection {_endpoint}<->{remoteEndpoint}: Connection close.");
                reader.Close();
                client.Close();
                cancellation.ThrowIfCancellationRequested();
            }
        }

        /// <summary>
        /// Opens the TCP server listener asynchronously.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public Task OpenAsync()
        {
            try
            {
                _listener = new TcpListener(_endpoint);
                _listener.Start();
                _mainLoopTask = RunAsync(_listener, _cancellationTokenSource.Token);
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Error opening TCP server listener {_endpoint}: {ex.Message}");
                return Task.FromException(ex);
            }
        }

        /// <summary>
        /// Reads an <see cref="IpFlow"/> asynchronously from the queue of flows. 
        /// Reading the source TCP stream is performed in a separate task and the result is put in a blocking queue.
        /// </summary>
        /// <param name="cancellation">The cancellation token to cancel the operation.</param>
        /// <returns>A task representing the asynchronous operation. The task result is an <see cref="IpFlow"/> or null if the input is completed.</returns>
        public Task<IpFlow?> ReadAsync(CancellationToken cancellation)
        {
            try
            {
                // Take does not wait in the case the input is completed.
                // In this case the return valus is null.
                var flow = _queue.Take(cancellation);
                return Task.FromResult<IpFlow?>(flow);
            }
            catch (OperationCanceledException)
            {
                _logger?.LogInformation($"Tcp server {_endpoint}: ReadAsync cancelled.");
                throw;
            }
        }

        /// <summary>
        /// Closes the TCP connection and cancels the reading process in all open incoming connections.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task CloseAsync()
        {
            _queue.CompleteAdding();
            var activeTasks = _clientsTasks.Append(_mainLoopTask ?? Task.CompletedTask).ToArray();

            // Cancel the reading process in all open incoming connections:
            _cancellationTokenSource.Cancel();

            try
            {
                // wait for all tasks to complete
                await Task.WhenAll(activeTasks);
            }
            catch (AggregateException e)
            {
                // some task could end with exceptions...just inform about it
                _logger?.LogWarning(e, $"Tcp server {_endpoint}: CloseAsync() exceptions.");
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public async Task AsyncCleanupOperation()
        {
            if (!_isDisposed)
            {
                _isDisposed = true;
                await CloseAsync();
                _cancellationTokenSource.Dispose();
                _queue.Dispose();
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            AsyncCleanupOperation().Wait();
        }

        /// <summary>
        /// Asynchronously releases the resources used by the <see cref="TcpJsonServer"/> object.
        /// </summary>
        /// <returns>A <see cref="ValueTask"/> representing the asynchronous operation.</returns>
        public async ValueTask DisposeAsync()
        {
            await AsyncCleanupOperation();
        }
    }
}
