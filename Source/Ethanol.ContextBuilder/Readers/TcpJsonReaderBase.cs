using Ethanol.ContextBuilder.Context;
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
    public abstract class TcpJsonReaderBase : IDisposable, IAsyncDisposable
    {
        private IPEndPoint _endpoint;
        private TcpListener _listener;
        private Task _mainLoopTask;
        private List<Task> _clientsTasks;
        private CancellationTokenSource _cancellationTokenSource;
        private BlockingCollection<IpFlow> _queue;
        private ILogger _logger;
        private bool _isDisposed;

        public IPEndPoint Endpoint => _endpoint;

        protected abstract bool TryDeserializeFlow(string jsonString, out IpFlow ipflow);
        protected abstract Task<string> ReadJsonStringAsync(StreamReader reader, CancellationToken cancellation);

        public TcpJsonReaderBase(IPEndPoint endPoint, ILogger logger)
        {
            _endpoint = endPoint;
            _cancellationTokenSource = new CancellationTokenSource();
            _queue = new BlockingCollection<IpFlow>();
            _clientsTasks = new List<Task>();
            _logger = logger;
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
                        var tcs = new TaskCompletionSource<object>();
                        _clientsTasks.Add(tcs.Task);
                        // Start as a background task... 
                        var _ = Task.Run(async () =>
                        {
                            await ReadInputData(client, cancellation);
                            _clientsTasks.Remove(tcs.Task);
                            tcs.SetResult(null);
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
                    var jsonString = await ReadJsonStringAsync(reader, cancellation);

                    // end of stream reached?
                    if (jsonString == null) break;

                    // ignore empty lines
                    if (jsonString == String.Empty) continue;

                    // read input tcp data and if suceffuly deserialized put the object in the buffer
                    // to be available to TryGetNextRecord method.
                    if (TryDeserializeFlow(jsonString, out var ipflow))
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

        public Task<IpFlow> ReadAsync(CancellationToken cancellation)
        {
            try
            {
                // Take does not wait in the case the input is completed.
                // In this case the return valus is null.
                var flow = _queue.Take(cancellation);
                return Task.FromResult(flow);
            }
            catch (OperationCanceledException)
            {
                _logger?.LogInformation($"Tcp server {_endpoint}: ReadAsync cancelled.");
                throw;
            }
        }

        public  async Task CloseAsync()
        {
            _queue.CompleteAdding();
            var activeTasks = _clientsTasks.Append(_mainLoopTask).ToArray();

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
        /// Asynchronously releases the resources used by the <see cref="TcpJsonReaderBase"/> object.
        /// </summary>
        /// <returns>A <see cref="ValueTask"/> representing the asynchronous operation.</returns>
        public async ValueTask DisposeAsync()
        {
            await AsyncCleanupOperation();
        }
    }

    public interface IJsonFlowSerializerReader
    {
        Task<string> ReadJsonStringAsync(TextReader inputStream, CancellationToken ct);
        bool TryDeserializeFlow(string jsonString, out IpFlow ipflow);
    }

    class TcpJsonReaderInternal : TcpJsonReaderBase
    {
        private readonly IJsonFlowSerializerReader _parent;

        public TcpJsonReaderInternal(IJsonFlowSerializerReader parent, IPEndPoint endPoint, ILogger logger) : base(endPoint, logger)
        {
            _parent = parent;
        }

        protected override Task<string> ReadJsonStringAsync(StreamReader reader, CancellationToken cancellation)
        {
            return _parent.ReadJsonStringAsync(reader, cancellation);
        }

        protected override bool TryDeserializeFlow(string jsonString, out IpFlow ipflow)
        {
            return _parent.TryDeserializeFlow(jsonString, out ipflow);
        }
    }
}
