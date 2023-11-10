using Ethanol.ContextBuilder.Aggregators;
using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive;

namespace Ethanol.ContextBuilder.Observable
{

    /// <summary>
    /// Provides extension methods for aggregating event streams into time-based windows.
    /// </summary>
    public static class AggregatorExtensions
    {
        /// <summary>
        /// Transforms an observable sequence into a series of "hopping" windows, each containing events that
        /// occurred within a specified time span. Each window is represented as an observable sequence itself.
        /// This is useful for batch processing or summarizing data over fixed time intervals.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source observable sequence.</typeparam>
        /// <param name="source">The source observable sequence that will be divided into windows.</param>
        /// <param name="timeSpan">The duration of each window. Events within this time span will be grouped together.</param>
        /// <returns>
        /// An observable sequence of windows (each represented as an observable sequence of items) that are 
        /// emitted at regular intervals defined by the specified duration.
        /// </returns>
        /// <remarks>
        /// Each window is closed at the end of its lifetime, meaning that once the specified time span for
        /// a window is over, it is emitted and a new window begins, even if the previous window is empty.
        /// This operation is stateful and must keep a memory of the events during the window time span.
        /// </remarks>
        /// <exception cref="ArgumentNullException">Thrown when the source observable is null.</exception>

        public static IObservable<ObservableEvent<IObservable<TSource>>> HoppingWindow<TSource>(this IObservable<ObservableEvent<TSource>> source, TimeSpan timeSpan)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            return System.Reactive.Linq.Observable.Create<ObservableEvent<IObservable<TSource>>>(observer =>
            {
                var window = new HoppingWindowAggregator<TSource>(timeSpan);
                source.Subscribe(window);
                return window.Subscribe(observer);
            });
        }

        public static IObservable<TResult> GroupByAggregate<TSource, TKey, TValue, TResult>(this IObservable<TSource> source, 
                                    Func<TSource, TKey> keySelector,
                                    Func<TSource, TValue> elementSelector,
                                    Func<KeyValuePair<TKey, TValue[]>, TResult> resultSelector)
        {
            if (source is null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (keySelector is null)
            {
                throw new ArgumentNullException(nameof(keySelector));
            }

            if (elementSelector is null)
            {
                throw new ArgumentNullException(nameof(elementSelector));
            }

            if (resultSelector is null)
            {
                throw new ArgumentNullException(nameof(resultSelector));
            }

            return System.Reactive.Linq.Observable.Create<TResult>(observer =>
            {
                var aggregator = new GroupByAggregator<TSource, TKey, TValue, TResult>(keySelector, elementSelector, resultSelector);
                source.Subscribe(aggregator);
                return aggregator.Subscribe(observer);
            });

        }
    }
}
