using Ethanol.ContextBuilder.Observable;
using Ethanol.ContextBuilder.Pipeline;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;

namespace Ethanol.ContextBuilder.Aggregators
{
    /// <summary>
    /// Aggregates events by grouping them based on a key and then transforming each group into a single result.
    /// </summary>
    /// <typeparam name="TSource">The type of the input events.</typeparam>
    /// <typeparam name="TKey">The type of the key to group by.</typeparam>
    /// <typeparam name="TValue">The type of the values that are being grouped.</typeparam>
    /// <typeparam name="TResult">The type of the result after the group aggregation.</typeparam>
    /// <remarks>
    /// This class implements an observable transformer that takes a stream of events,
    /// groups them by a specified key, and then transforms each group into a single, aggregated result.
    /// It's a reactive pipeline component that can be used to process streams of data in real-time.
    /// </remarks>
    public class GroupByAggregator<TSource, TKey, TValue, TResult> : ObservableBase<TResult>, IObservableTransformer<TSource, TResult>
    {
        readonly TaskCompletionSource _tcs = new TaskCompletionSource();

        private readonly Subject<TSource> _sourceSubject;
        private readonly Subject<TResult> _resultSubject;

        public GroupByAggregator(Func<TSource, TKey> keySelector,
                                 Func<TSource, TValue> elementSelector,
                                 Func<KeyValuePair<TKey, TValue[]>, TResult> resultSelector)
        {
            _sourceSubject = new Subject<TSource>();
            _resultSubject = new Subject<TResult>();
            var grouped = _sourceSubject
                .GroupBy(keySelector, elementSelector)
                .SelectMany(group => group.ToArray().Select(array => resultSelector(new KeyValuePair<TKey, TValue[]>(group.Key, array))));        
            grouped.Subscribe(_resultSubject);
        }

        public static long GetEndTime(IEnumerable<DateTime> enumerable)
        {
            long? time = null;
            foreach (var item in enumerable) { time = time == null ? item.Ticks : System.Math.Max(time.Value, item.Ticks); }
            return time ?? DateTime.MaxValue.Ticks;
        }

        public static long GetStartTime(IEnumerable<DateTime> enumerable)
        {
            long? time = null;
            foreach(var item in enumerable) { time = time == null ? item.Ticks : System.Math.Min(time.Value, item.Ticks); }
            return time ?? DateTime.MinValue.Ticks;
        }

        public Task Completed => _tcs.Task;

        public IPerformanceCounters Counters => PerformanceCounters.Default;

        public void OnCompleted()
        {
            _sourceSubject.OnCompleted();
            _tcs.SetResult();
        }

        public void OnError(Exception error)
        {
            _sourceSubject.OnError(error);
        }

        public void OnNext(TSource value)
        {
            _sourceSubject.OnNext(value);
        }

        protected override IDisposable SubscribeCore(IObserver<TResult> observer)
        {
            return _resultSubject.Subscribe(observer);
        }
    }
}
