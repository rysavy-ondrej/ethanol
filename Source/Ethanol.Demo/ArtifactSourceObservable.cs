using Ethanol.Providers;
using Microsoft.StreamProcessing;
using System;
using System.IO;
using System.Linq;
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
    public class ArtifactSourceObservable<T> : IObservable<StreamEvent<T>> where T : IpfixArtifact
    {
        private readonly Subject<StreamEvent<T>> _observable;
        private int batchCount = 0;
        public ArtifactSourceObservable()
        {
            _observable = new Subject<StreamEvent<T>>();
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
                long lastTimestamp = 0;
                var count = 0;
                foreach (var obj in CsvArtifactProvider<T>.LoadFrom(stream))
                {
                    if (cancellationToken.IsCancellationRequested) break;
                    var recordTimeStamp = AddRecord(obj);
                    lastTimestamp = Math.Max(lastTimestamp, recordTimeStamp);
                    count++;
                }
                batchCount++;
                //Console.WriteLine($"{typeof(T)}: batch = {batchCount}, new = {count}, ts = {new DateTime(lastTimestamp+1)}");
                _observable.OnNext(StreamEvent.CreatePunctuation<T>(lastTimestamp + 1));    // This does not work, why?
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
        public long AddRecord(T obj)
        {
            // Console.Write(typeof(T).Name[8]);  // DEBUG: to see what is being pushed trhough the observable
            var timestamp = obj.StartTime;
            _observable.OnNext(StreamEvent.CreatePoint<T>(timestamp, obj));
            return timestamp;
        }
         
        /// <summary>
        /// Closes the current observable and signalized that it has been completed to all subscribers.
        /// </summary>
        public void Close()
        {
            _observable.OnCompleted();
        }

        /// <inheritdoc/>>
        public IDisposable Subscribe(IObserver<StreamEvent<T>> observer)
        {
            return _observable.Subscribe(observer);
        }
    }
}
