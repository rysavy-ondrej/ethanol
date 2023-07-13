// This is simple:
// Read input JSON, perform searching for IPs, kewords in domain names, etc, and produce the output...
//
// The Netify is provided as input XXX file and represented as in-memory database.
// The app fingeprint is also loaded from XXX file and represented as in-memory database NMemory (???)
// 

namespace Ethanol.ApplicationSonar
{
    /// <summary>
    /// Represents a disposable asynchronous enumerable that implements <see cref="IAsyncEnumerable{T}"/> and <see cref="IDisposable"/>.
    /// </summary>
    /// <typeparam name="T">The type of elements in the enumerable.</typeparam>
    class DisposableAsyncEnumerable<T> : IAsyncEnumerable<T>, IDisposable
    {
        private IAsyncEnumerable<T> _asyncEnumerable;
        private bool _disposedValue;
        private readonly IDisposable[] _disposables;

        /// <summary>
        /// Initializes a new instance of the <see cref="DisposableAsyncEnumerable{T}"/> class with the specified async enumerable and optional disposables.
        /// </summary>
        /// <param name="asyncEnumerable">The underlying async enumerable.</param>
        /// <param name="disposables">Optional disposable objects to be disposed of when this instance is disposed.</param>
        public DisposableAsyncEnumerable(IAsyncEnumerable<T> asyncEnumerable, params IDisposable[] disposables)
        {
            this._asyncEnumerable = asyncEnumerable;
            this._disposables = disposables;
        }

        /// <summary>
        /// Returns an asynchronous enumerator that can be used to iterate over the elements of the async enumerable.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token to cancel the enumeration.</param>
        /// <returns>An asynchronous enumerator over the elements of the async enumerable.</returns>
        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            return _asyncEnumerable.GetAsyncEnumerator(cancellationToken);
        }

        /// <summary>
        /// Releases the resources used by this instance.
        /// </summary>
        /// <param name="disposing">A flag indicating whether the method is called from the <see cref="Dispose"/> method or the finalizer.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    foreach (var d in _disposables)
                        d.Dispose();
                }
                _disposedValue = true;
            }
        }

        /// <summary>
        /// Releases all resources used by this instance.
        /// </summary>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}