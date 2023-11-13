using Ethanol.ContextBuilder.Observable;
using Ethanol.ContextBuilder.Pipeline;
using System;
using System.Reactive.Subjects;
using System.Threading.Tasks;

namespace Ethanol.ContextBuilder.Filters
{
    /// <summary>
    /// Represents a filtering transformer in an observable pipeline that allows events to pass through only if they match a specified condition.
    /// </summary>
    /// <typeparam name="TValue">The type of the values contained in the observable events.</typeparam>
    public class ObservableFilter<TValue> : IObservableTransformer<ObservableEvent<TValue>, ObservableEvent<TValue>>
    {
        // Task completion source to signal completion of the data flow.
        readonly TaskCompletionSource _tcs = new();

        // The predicate function that determines if an ObservableEvent<TValue> matches the condition to pass through the filter.
        private readonly Func<ObservableEvent<TValue>, bool> _match;

        // The subject that acts as both an observer and an observable.
        private readonly Subject<ObservableEvent<TValue>> _subject;

        /// <summary>
        /// Initializes a new instance of the ObservableFilter with the match condition.
        /// </summary>
        /// <param name="match">A predicate that takes an ObservableEvent<TValue> and returns a boolean indicating whether the event passes the filter.</param>
        public ObservableFilter(Func<ObservableEvent<TValue>, bool> match)
        {
            this._match = match ?? throw new ArgumentNullException(nameof(match), "Match predicate cannot be null.");
            this._subject = new Subject<ObservableEvent<TValue>>();
        }

        /// <summary>
        /// Gets the task that completes when the filter has finished processing all events.
        /// </summary>
        public Task Completed => _tcs.Task;

        /// <summary>
        /// Called when the observable sequence has completed.
        /// </summary>
        public void OnCompleted()
        {
            _subject.OnCompleted();
            _tcs.SetResult();
        }

        /// <summary>
        /// Called when an error has occurred during processing.
        /// </summary>
        /// <param name="error">The exception that has occurred.</param>
        public void OnError(Exception error)
        {
            _subject.OnError(error);
        }

        /// <summary>
        /// Called when a new event is available to be processed.
        /// </summary>
        /// <param name="value">The current event to be processed.</param>
        public void OnNext(ObservableEvent<TValue> value)
        {
            // Only propagate the event if it matches the condition.
            if (_match(value))
            {
                _subject.OnNext(value);
            }
        }

        /// <summary>
        /// Subscribes an observer to the subject allowing filtered events to be observed.
        /// </summary>
        /// <param name="observer">The observer that wants to receive events.</param>
        /// <returns>The IDisposable subscription used to unsubscribe from the observable sequence.</returns>
        public IDisposable Subscribe(IObserver<ObservableEvent<TValue>> observer)
        {
            return _subject.Subscribe(observer);
        }
    }

}
