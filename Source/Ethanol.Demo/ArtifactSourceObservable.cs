using Ethanol.Providers;
using System;
using System.IO;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;

namespace Ethanol.Demo
{
    /// <summary>
    /// An observable that represents artifact source.
    /// Records are added to this observable either manually by calling <see cref="AddRecord(T)"/> method or
    /// from a source CSV file by using <see cref="LoadFrom(string, CancellationToken)"/>.
    /// </summary>
    /// <typeparam name="T">The type of artifacts.</typeparam>
    public class ArtifactSourceObservable<T> : IObservable<T> where T : IpfixArtifact
    {
        private readonly Subject<T> _observable;
        public ArtifactSourceObservable()
        {
            _observable = new Subject<T>();
        }

        /// <summary>
        /// Loads the collection of records from the given CSV file.
        /// </summary>
        /// <param name="filename">The source filename.</param>
        /// <param name="cancellationToken">The cancellation token to stop the operation.</param>
        /// <returns>A number of record loaded.</returns>
        public int LoadFrom(Stream stream, CancellationToken cancellationToken)
        {
            try
            {
                var count = 0;
                foreach (var obj in CsvArtifactProvider<T>.LoadFrom(stream))
                {
                    if (cancellationToken.IsCancellationRequested) break;
                    _observable.OnNext(obj);
                    count++;
                }
                return count;
            }
            catch(Exception e)
            {
                Console.Error.WriteLine(e);
                return 0;
            }
        }

        /// <summary>
        /// Adds a record to the current observable.
        /// </summary>
        /// <param name="obj">The new record.</param>
        public void AddRecord(T obj)
        {
            _observable.OnNext(obj);
        }
         
        /// <summary>
        /// Closes the current observable and signalized that it has been completed to all subscribers.
        /// </summary>
        public void Close()
        {
            _observable.OnCompleted();
        }

        /// <inheritdoc/>>
        public IDisposable Subscribe(IObserver<T> observer)
        {
            return _observable.Subscribe(observer);
        }
    }
}
