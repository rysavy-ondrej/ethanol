using Ethanol.ContextBuilder.Pipeline;
using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Reactive.Subjects;
using System.Threading.Tasks;

namespace Ethanol.ContextBuilder.Observable
{
    /// <summary>
    /// Implementation class for the hopping window logic on observable events.
    /// </summary>
    /// <typeparam name="T">Type of the event payload in the observable.</typeparam>
    public class HoppingWindowAggregator<T> : IObservableTransformer<ObservableEvent<T>, ObservableEvent<IObservable<T>>>
    {
        private long _timeSpan;
        private long _currentEpoch = 0;

        // The current window subject.
        private Subject<T> _currentWindow = null;
        TaskCompletionSource _tcs = new TaskCompletionSource();

        // Collection of observers subscribed to the observable.
        private List<IObserver<ObservableEvent<IObservable<T>>>> _observers;

        public PipelineNodeType NodeType => PipelineNodeType.Transformer;

        public Task Completed => _tcs.Task;

        /// <summary>
        /// Initializes a new instance of the <see cref="HoppingWindowAggregator{T}"/> class with a specified time span.
        /// </summary>
        /// <param name="timeSpan">Duration of each window.</param>
        public HoppingWindowAggregator(TimeSpan timeSpan)
        {
            _timeSpan = timeSpan.Ticks;
            _observers = new List<IObserver<ObservableEvent<IObservable<T>>>>();
        }

        /// <summary>
        /// Subscribes an observer to the observable sequence.
        /// </summary>
        /// <param name="observer">Observer to be subscribed.</param>
        /// <returns>A disposable object representing the observer's subscription.</returns>
        public IDisposable Subscribe(IObserver<ObservableEvent<IObservable<T>>> observer)
        {
            _observers.Add(observer);
            return Disposable.Create(() => { observer.OnCompleted(); _observers.Remove(observer); });
        }

        /// <summary>
        /// Notifies all observers about the end of the sequence.
        /// </summary>
        public void OnCompleted()
        {
            _currentWindow?.OnCompleted();
            var observers = _observers.ToArray();
            foreach (var o in observers) o.OnCompleted();
            _tcs.SetResult();
        }

        /// <summary>
        /// Notifies all observers with an error.
        /// </summary>
        /// <param name="e">The error exception.</param>
        public void OnError(Exception e)
        {
            _currentWindow?.OnError(e);
        }

        // Emits the current window to all observers.
        void EmitWindow()
        {
            var observers = _observers.ToArray();
            foreach (var o in observers) o.OnNext(new ObservableEvent<IObservable<T>>(_currentWindow, (_currentEpoch * _timeSpan), (_currentEpoch * _timeSpan + _timeSpan)));
        }

        /// <summary>
        /// Process the next event.
        /// </summary>
        /// <param name="evt">The event to process.</param>
        public void OnNext(ObservableEvent<T> evt)
        {
            if (_currentWindow == null) // this is the very first event received.
            {
                _currentWindow = new Subject<T>();
                _currentEpoch = evt.StartTime.Ticks / _timeSpan;
                EmitWindow();
            }
            else
            {
                while (evt.StartTime.Ticks >= _currentEpoch * _timeSpan + _timeSpan)
                {
                    ShiftWindow();
                }
            }
            // the element is obsolete, its end time is before the current window:
            if (evt.EndTime.Ticks < GetCurrentWindowInterval().Item1) return;

            _currentWindow.OnNext(evt.Payload);
        }

        (long,long) GetCurrentWindowInterval()
        {
            return (_currentEpoch * _timeSpan, _currentEpoch * _timeSpan + _timeSpan);
        }

        // Shifts to the next window.
        void ShiftWindow()
        {
            _currentWindow.OnCompleted();
            _currentEpoch++;
            var newWindow = new Subject<T>();
            _currentWindow = newWindow;
            EmitWindow();
        }

        // Rounds the ticks value based on the rounding ticks provided.
        private static long RoundTicks(long timeTicks, long roundTicks)
        {
            var sub = timeTicks % roundTicks;
            return (timeTicks - sub);
        }
    }
}
