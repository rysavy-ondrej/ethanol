using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Subjects;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Ethanol.ContextBuilder.Builders;

namespace Ethanol.ContextBuilder.Observable
{
    public record ObservableEvent<TPayload>
    {
        public DateTime StartTime { get; init; }
        public DateTime EndTime { get; init; }
        public TPayload Payload { get; init; }
        public ObservableEvent(TPayload payload, long startTime, long endTime)
        {
            Payload = payload;
            StartTime = new DateTime(startTime);
            EndTime = new DateTime(endTime);
        }

        public ObservableEvent(TPayload payload, DateTime startTime, DateTime endTime)
        {
            Payload = payload;
            StartTime = startTime;
            EndTime = endTime;
        }
    }

    public class WindowHopImplementation<T> : IObserver<ObservableEvent<T>>, IObservable<ObservableEvent<IObservable<T>>>
    {
        long _timeSpan;
        long _currentEpoch = 0;

        Subject<T> _currentWindow = null;
        List<IObserver<ObservableEvent<IObservable<T>>>> _observers;

        public WindowHopImplementation(TimeSpan timeSpan)
        {
            _timeSpan = timeSpan.Ticks;
            _observers = new List<IObserver<ObservableEvent<IObservable<T>>>>();
        }
        public IDisposable Subscribe(IObserver<ObservableEvent<IObservable<T>>> observer)
        {
            _observers.Add(observer);
            return Disposable.Create(() => { observer.OnCompleted(); _observers.Remove(observer); });
        }
        public void OnCompleted()
        {
            _currentWindow?.OnCompleted();
            var observers = _observers.ToArray();
            foreach (var o in observers) o.OnCompleted();
        }
        public void OnError(Exception e)
        {
        }

        void EmitWindow()
        {
            var observers = _observers.ToArray();
            foreach (var o in observers) o.OnNext(new ObservableEvent<IObservable<T>>(_currentWindow, (_currentEpoch * _timeSpan), (_currentEpoch * _timeSpan + _timeSpan)));
        }


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
            _currentWindow.OnNext(evt.Payload);
        }
        void ShiftWindow()
        {
            _currentWindow.OnCompleted();
            _currentEpoch++;
            var newWindow = new Subject<T>();
            _currentWindow = newWindow;
            EmitWindow();
        }
        private static long RoundTicks(long timeTicks, long roundTicks)
        {
            var sub = timeTicks % roundTicks;
            return (timeTicks - sub);
        }
    }
    public static class WindowOperators
    {
        public static IObservable<ObservableEvent<IObservable<T>>> WindowHop<T>(this IObservable<ObservableEvent<T>> source, TimeSpan timeSpan)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            return System.Reactive.Linq.Observable.Create<ObservableEvent<IObservable<T>>>(observer =>
            {
                var window = new WindowHopImplementation<T>(timeSpan);
                source.Subscribe(window);
                return window.Subscribe(observer);
            });
        }
    }
}
