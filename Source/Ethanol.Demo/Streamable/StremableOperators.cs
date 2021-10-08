using Ethanol.Streaming;
using Microsoft.StreamProcessing;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive.Linq;

namespace Ethanol.Demo
{
    public static class StremableOperators
    {
        /// <summary>
        /// Transforms the source stream to a new stream by expanding individual elements of each source event.
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TElement"></typeparam>
        /// <typeparam name="TTarget"></typeparam>
        /// <param name="source"></param>
        /// <param name="getElements"></param>
        /// <param name="getFlowKey"></param>
        /// <param name="selector"></param>
        /// <returns></returns>
        public static IStreamable<Empty, TTarget> Expand<TSource, TElement, TTarget, TKey>(this IStreamable<Empty, TSource> source, Func<TSource, IEnumerable<TElement>> getElements, Func<TElement, TSource, TTarget> selector, Func<TTarget,TKey> getKey)
        {
            var target = source.SelectMany(record => getElements(record).Select(item => selector(item, record)));
            // need to group and select only the first one otherwise we have duplicities, WHY?
            return target.GroupApply(
                val => getKey(val),
                group => group.Aggregate(aggregate => aggregate.CollectSet(flow => flow)),
                (key, value) => value.First());
        }
        /// <summary>
        /// Correlates the elements in the source stream by using <paramref name="matchKeySelector"/>
        /// and then groups them by <paramref name="resultKeySelector"/> to produce the result stream 
        /// by applying <paramref name="resultSelector"/>.
        /// </summary>
        /// <typeparam name="TEmpty"></typeparam>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TMatchKey"></typeparam>
        /// <typeparam name="TTarget"></typeparam>
        /// <param name="source">The source stream.</param>
        /// <param name="matchKeySelector">The match key for correlating source elements.</param>
        /// <param name="resultKeySelector">The grouping key for collecting results.</param>
        /// <param name="resultSelector">The function to compute the result from the grouping.</param>
        /// <returns>A stream that contains correlated and grouped elements.</returns>
        public static IStreamable<TEmpty, TTarget> MatchGroupApply<TEmpty, TSource, TMatchKey, TResultKey, TTarget>(
            this IStreamable<TEmpty, TSource> source,
            Expression<Func<TSource, TMatchKey>> matchKeySelector,
            Expression<Func<TSource, TResultKey>> resultKeySelector,
            Expression<Func<IGrouping<TResultKey, TSource>, TTarget>> resultSelector) where TSource : IEquatable<TSource>

        {
            var correlatedStream = source.Multicast(input =>
                input.Join(input, matchKeySelector, matchKeySelector,
                    (left, right) => KeyValuePair.Create(left, right)
                    )
            );
            var arg = Expression.Parameter(typeof(KeyValuePair<TSource, TSource>), "input");
            var expr = Expression.Lambda<Func<KeyValuePair<TSource, TSource>, TResultKey>>(Expression.Invoke(resultKeySelector, Expression.Property(arg, nameof(KeyValuePair<TSource, TSource>.Key))), arg);
            var groupStream = correlatedStream.GroupApply(
                expr,
                group => group.Aggregate(aggregate => aggregate.CollectList(item => item.Value)),
                (key, value) => new Grouping<TResultKey, TSource>(key.Key, value) as IGrouping<TResultKey, TSource>);

            return groupStream.Select(resultSelector);
        }
        internal class Grouping<TKey, TElement> : IGrouping<TKey, TElement>
        {
            private readonly TKey key;
            private readonly IEnumerable<TElement> values;

            public Grouping(TKey key, IEnumerable<TElement> values)
            {
                this.key = key ?? throw new ArgumentNullException(nameof(key));
                this.values = values ?? throw new ArgumentNullException(nameof(values));
            }

            public TKey Key
            {
                get { return key; }
            }

            public IEnumerator<TElement> GetEnumerator()
            {
                return values.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
    }   
}
