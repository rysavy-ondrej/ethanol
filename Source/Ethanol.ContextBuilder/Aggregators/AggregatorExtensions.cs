using Ethanol.ContextBuilder.Aggregators;
using System;
using System.Collections.Generic;

namespace Ethanol.ContextBuilder.Observable
{

    /// <summary>
    /// Provides extension methods for aggregating event streams into grooups and windows.
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

        public static IObservable<ObservableEvent<IObservable<TSource>>> HoppingWindow<TSource>(this IObservable<ObservableEvent<TSource>> source,
            IObservableTransformer<ObservableEvent<TSource>, ObservableEvent<IObservable<TSource>>> windowTransformer)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            return System.Reactive.Linq.Observable.Create<ObservableEvent<IObservable<TSource>>>(observer =>
            {
                source.Subscribe(windowTransformer);
                return windowTransformer.Subscribe(observer);
            });
        }

        /// <summary>
        /// Transforms an observable sequence into a sequence of groups based on a specified key selector function, 
        /// maps each element of each group by using a specified function, and then transforms them into a result sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <typeparam name="TKey">The type of the keys returned by the key selector function.</typeparam>
        /// <typeparam name="TValue">The type of the values in the resulting groups.</typeparam>
        /// <typeparam name="TResult">The type of the elements in the resulting sequence.</typeparam>
        /// <param name="source">The source observable sequence.</param>
        /// <param name="keySelector">A function to extract the key for each element.</param>
        /// <param name="elementSelector">A transform function to apply to each element of the source sequence.</param>
        /// <param name="resultSelector">A function to transform the groups of elements into the elements of the result sequence.</param>
        /// <returns>
        /// An observable sequence of results where each result is obtained by applying the resultSelector function 
        /// to a group of elements from the source sequence that share a common key.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if the source sequence, keySelector, elementSelector, or resultSelector is null.
        /// </exception>
        /// <remarks>
        /// This method uses deferred execution and streams its results.
        /// </remarks>
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
