using Ethanol.ContextBuilder.Observable;
using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Subjects;
using System.Threading.Tasks;

namespace Ethanol.ContextBuilder.Aggregators
{
    /// <summary>
    /// Implementation class for the hopping window logic on observable events.
    /// </summary>
    /// <typeparam name="TRecord">Type of the event payload in the observable.</typeparam>
    public class HoppingWindowAggregator<TRecord> : ObservableBase<ObservableEvent<IObservable<TRecord>>>, IObservableTransformer<ObservableEvent<TRecord>, ObservableEvent<IObservable<TRecord>> >
    {

        /// <summary>
        /// Represents the statistics of a window in the HoppingWindowAggregator.
        /// </summary>
        public class WindowStatistics
        {
            /// <summary>
            /// Gets or sets the total number of flows.
            /// </summary>
            public int FlowsTotal { get; set; }

            /// <summary>
            /// Gets or sets the total number of out-of-order flows.
            /// </summary>
            public int OutOfOrderFlowsTotal { get; set; }

            /// <summary>
            /// Gets or sets the number of flows in the current window.
            /// </summary>
            public int FlowsInCurrentWindow { get; set; }

            /// <summary>
            /// Gets or sets the number of out-of-order flows in the current window.
            /// </summary>
            public int OutOfOrderFlowsInCurrentWindow { get; set; }

            /// <summary>
            /// Gets or sets the start time of the current window.
            /// </summary>
            public DateTime CurrentWindowStart { get; set; }

            /// <summary>
            /// Gets or sets the timestamp of the current window.
            /// </summary>
            public DateTime CurrentTimestamp { get; set; }
        }

        /// <summary>
        /// The time span used by the hopping window aggregator.
        /// </summary>
        private readonly long _timeSpan;
        /// <summary>
        /// Represents the current epoch value used in the hopping window aggregator.
        /// </summary>
        private long _currentEpoch = 0;

        /// <summary>
        /// Gets the statistics for the window.
        /// </summary>
        public WindowStatistics Statistics { get; } = new WindowStatistics();


        // The current window subject.
        private Subject<TRecord>? _currentWindow = null;
        /// <summary>
        /// Task completion source used for signaling the completion of a task.
        /// </summary>
        readonly TaskCompletionSource _tcs = new();

        // Collection of observers subscribed to the observable.
        private readonly List<IObserver<ObservableEvent<IObservable<TRecord>>>> _observers;

        /// <summary>
        /// The observer for obsolete records in the hopping window aggregator.
        /// </summary>
        private IObserver<ObservableEvent<TRecord>>? _obsoleteRecordObserver; 

        /// <summary>
        /// Gets a task that represents the completion of the operation.
        /// </summary>
        public Task Completed => _tcs.Task;

        /// <summary>
        /// Initializes a new instance of the <see cref="HoppingWindowAggregator{T}"/> class with a specified time span.
        /// </summary>
        /// <param name="timeSpan">Duration of each window.</param>
        public HoppingWindowAggregator(TimeSpan timeSpan, IObserver<ObservableEvent<TRecord>>? obsoleteRecordObserver = null)
        {
            _timeSpan = timeSpan.Ticks;
            _observers = new List<IObserver<ObservableEvent<IObservable<TRecord>>>>();
            _obsoleteRecordObserver = obsoleteRecordObserver;
        }
        /// <summary>
        /// Notifies all observers about the end of the sequence.
        /// </summary>
        public void OnCompleted()
        {
            _currentWindow?.OnCompleted();
            var observers = _observers.ToArray();
            foreach (var o in observers) o.OnCompleted();
            _obsoleteRecordObserver?.OnCompleted();
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


        /// <summary>
        /// Emits the specified window to the observers.
        /// </summary>
        /// <param name="window">The window to emit.</param>
        void EmitWindow(Subject<TRecord> window, long currentEpoch, long timeSpan)
        {
            Statistics.CurrentWindowStart = new DateTime(currentEpoch * timeSpan);

            var observers = _observers.ToArray();
            foreach (var o in observers)
            {
                o.OnNext(new ObservableEvent<IObservable<TRecord>>(window, currentEpoch * timeSpan, currentEpoch * timeSpan + timeSpan));
            }
        }

        /// <summary>
        /// Process the next event.
        /// </summary>
        /// <param name="evt">The event to process.</param>
        public void OnNext(ObservableEvent<TRecord> evt)
        {
            if (_currentWindow == null) // this is the very first event received.
            {
                _currentWindow = new Subject<TRecord>();
                _currentEpoch = evt.StartTime.Ticks / _timeSpan;
                EmitWindow(_currentWindow, _currentEpoch, _timeSpan);
            }
            else
            {
                while (evt.StartTime.Ticks >= _currentEpoch * _timeSpan + _timeSpan)
                {
                    ShiftWindow();
                }
            }
            // the element is obsolete, its end time is before the current window:
            if (evt.EndTime.Ticks < GetCurrentWindowInterval().Item1)
            {
                Statistics.OutOfOrderFlowsTotal++;
                Statistics.OutOfOrderFlowsInCurrentWindow++;

                _obsoleteRecordObserver?.OnNext(evt);                                             
                return;
            }
            else
            {
                Statistics.FlowsInCurrentWindow++;
                Statistics.FlowsTotal++;
                Statistics.CurrentTimestamp = evt.StartTime;

                if (evt.Payload != null)
                {
                    _currentWindow.OnNext(evt.Payload);
                }
            }            
        }

        /// <summary>
        /// Gets the current window interval.
        /// </summary>
        /// <returns>A tuple representing the start and end of the current window interval.</returns>
        (long, long) GetCurrentWindowInterval()
        {
            return (_currentEpoch * _timeSpan, _currentEpoch * _timeSpan + _timeSpan);
        }

        /// <summary>
        /// Shifts the window by completing the current window, incrementing the epoch, and creating a new window.
        /// </summary>
        void ShiftWindow()
        {
            _currentWindow?.OnCompleted();
            _currentEpoch++;

            Statistics.FlowsInCurrentWindow = 0;
            Statistics.OutOfOrderFlowsInCurrentWindow = 0;
            
            _currentWindow = new Subject<TRecord>();

            EmitWindow(_currentWindow, _currentEpoch, _timeSpan);
        }

        /// <summary>
        /// Represents a mechanism for releasing unmanaged resources.
        /// </summary>
        /// <remarks>
        /// This interface is typically used to release unmanaged resources such as file handles, network connections, or database connections.
        /// </remarks>
        protected override IDisposable SubscribeCore(IObserver<ObservableEvent<IObservable<TRecord>>> observer)
        {
            _observers.Add(observer);
            return Disposable.Create(() => { observer.OnCompleted(); _observers.Remove(observer); });
        }
    }
}
