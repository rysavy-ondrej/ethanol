using Ethanol.ContextBuilder.Observable;
using Ethanol.ContextBuilder.Pipeline;
using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Ethanol.ContextBuilder.Aggregators
{
    /// <summary>
    /// Implementation class for the hopping window logic on observable events.
    /// </summary>
    /// <typeparam name="TRecord">Type of the event payload in the observable.</typeparam>
    public class HoppingWindowAggregator<TRecord> : ObservableBase<ObservableEvent<IObservable<TRecord>>>, IObservableTransformer<ObservableEvent<TRecord>, ObservableEvent<IObservable<TRecord>> >
    {

        public class WindowStatistics
        {
            public int FlowsTotal { get; set; }           
            public int OutOfOrderFlowsTotal { get; set; }
            public int FlowsInCurrentWindow { get; set; }
            public int OutOfOrderFlowsInCurrentWindow { get; set; }
            public DateTime CurrentWindowStart { get; set; }
            public DateTime CurrentTimestamp { get; set; }
        }

        private readonly long _timeSpan;
        private long _currentEpoch = 0;

        public WindowStatistics Statistics { get; } = new WindowStatistics();


        // The current window subject.
        private Subject<TRecord> _currentWindow = null;
        readonly TaskCompletionSource _tcs = new TaskCompletionSource();

        // Collection of observers subscribed to the observable.
        private readonly List<IObserver<ObservableEvent<IObservable<TRecord>>>> _observers;

        private IObserver<ObservableEvent<TRecord>> _obsoleteRecordObserver; 

        public Task Completed => _tcs.Task;

        /// <summary>
        /// Initializes a new instance of the <see cref="HoppingWindowAggregator{T}"/> class with a specified time span.
        /// </summary>
        /// <param name="timeSpan">Duration of each window.</param>
        public HoppingWindowAggregator(TimeSpan timeSpan, IObserver<ObservableEvent<TRecord>> obsoleteRecordObserver = null)
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

        // Emits the current window to all observers.
        void EmitWindow()
        {
            Statistics.CurrentWindowStart = new DateTime(_currentEpoch * _timeSpan);

            var observers = _observers.ToArray();
            foreach (var o in observers) o.OnNext(new ObservableEvent<IObservable<TRecord>>(_currentWindow, _currentEpoch * _timeSpan, _currentEpoch * _timeSpan + _timeSpan));
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

                _currentWindow.OnNext(evt.Payload);
            }            
        }

        (long, long) GetCurrentWindowInterval()
        {
            return (_currentEpoch * _timeSpan, _currentEpoch * _timeSpan + _timeSpan);
        }

        // Shifts to the next window.
        void ShiftWindow()
        {
            _currentWindow.OnCompleted();
            _currentEpoch++;

            Statistics.FlowsInCurrentWindow = 0;
            Statistics.OutOfOrderFlowsInCurrentWindow = 0;
            

            var newWindow = new Subject<TRecord>();
            _currentWindow = newWindow;
            
            EmitWindow();
        }

        protected override IDisposable SubscribeCore(IObserver<ObservableEvent<IObservable<TRecord>>> observer)
        {
            _observers.Add(observer);
            return Disposable.Create(() => { observer.OnCompleted(); _observers.Remove(observer); });
        }
    }
}
