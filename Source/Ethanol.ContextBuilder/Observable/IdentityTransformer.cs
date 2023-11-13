using Ethanol.ContextBuilder.Pipeline;
using System;
using System.Reactive.Subjects;
using System.Threading.Tasks;

namespace Ethanol.ContextBuilder.Observable
{
    /// <summary>
    /// Represents a transformer that relays items it observes without transformation.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements in the observable sequence.</typeparam>
    public class IdentityTransformer<TSource> : IObservableTransformer<TSource, TSource>
    {
        /// <summary>
        /// Underlying subject that relays observed items to subscribers.
        /// </summary>
        private Subject<TSource> _subject = new Subject<TSource>();

        /// Represents a mechanism for signaling the completion of some asynchronous operation. 
        /// This provides a way to manually control the lifetime of a Task, signaling its completion.
        private TaskCompletionSource _tcs = new TaskCompletionSource();

        public Task Completed => _tcs.Task;

        /// <summary>
        /// Notifies subscribers that the provider has finished sending push-based notifications.
        /// </summary>
        public void OnCompleted()
        {
            _subject.OnCompleted();
            _tcs.SetResult();
        }

        /// <summary>
        /// Notifies subscribers that the provider has experienced an error condition.
        /// </summary>
        /// <param name="error">An object that provides additional information about the error.</param>
        public void OnError(Exception error)
        {
            _subject.OnError(error);
        }

        /// <summary>
        /// Provides the next item in the sequence to subscribers.
        /// </summary>
        /// <param name="value">The next item in the sequence.</param>
        public void OnNext(TSource value)
        {
            _subject.OnNext(value);
        }

        /// <summary>
        /// Subscribes an observer to the subject.
        /// </summary>
        /// <param name="observer">Observer to subscribe to the subject.</param>
        /// <returns>Reference to an IDisposable object that allows observers to stop receiving notifications before the provider has finished sending them.</returns>
        public IDisposable Subscribe(IObserver<TSource> observer)
        {
            return _subject.Subscribe(observer);
        }
    }

}
