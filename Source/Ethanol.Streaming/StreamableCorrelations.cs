using Microsoft.StreamProcessing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive.Linq;

namespace Ethanol.Streaming
{
    public static class StreamableCorrelations
    {
        /// <summary>
        /// Transforms the <paramref name="source"/> stream to a new stream by expanding all <typeparamref name="TElement"/> items of each <typeparamref name="TSource"/> object. 
        /// It also eliminate the potential duplicities of <typeparamref name="TTarget"/> object in the resulting stream.
        /// </summary>
        /// <typeparam name="TSource">The type of elements in the source stream.</typeparam>
        /// <typeparam name="TElement">The type of items generated from the source element.</typeparam>
        /// <typeparam name="TTarget">The type of resulting objects.</typeparam>
        /// <typeparam name="TKey">The key of <typeparamref name="TTarget"/> object that is used to eliminate possible duplicities.</typeparam>
        /// <param name="source">The source stream.</param>
        /// <param name="getElements">Gets the elements from the source item.</param>
        /// <param name="selector">Tha target element selector.</param>
        /// <param name="getKey">The key providing function that is used to get the key for grouping elements in the resulting stream.</param>
        /// <returns></returns>
        public static IStreamable<Empty, TTarget> Expand<TSource, TElement, TTarget, TKey>(this IStreamable<Empty, TSource> source, Func<TSource, IEnumerable<TElement>> getElements, Func<TElement, TSource, TTarget> selector, Func<TTarget, TKey> getKey)
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
        /// <summary>
        /// Enrich records in the <paramref name="source"/> stream with information from records in <paramref name="other"/> stream.
        /// It is guaranteed that the source record is enriched at most by one other record.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TPayload"></typeparam>
        /// <typeparam name="TOtherPayload"></typeparam>
        /// <typeparam name="TResultPayload"></typeparam>
        /// <param name="source"></param>
        /// <param name="other"></param>
        /// <param name="leftKeySelector"></param>
        /// <param name="rightKeySelector"></param>
        /// <param name="enrichSelector"></param>
        /// <returns></returns>
        public static IStreamable<Empty, TResultPayload> EnrichFrom<TKey, TPayload, TOtherPayload, TResultPayload>(
            this IStreamable<Empty, TPayload> source,
            IStreamable<Empty, TOtherPayload> other,
            Expression<Func<TPayload, TKey>> leftKeySelector,
            Expression<Func<TOtherPayload, TKey>> rightKeySelector,
            Expression<Func<TPayload, IGrouping<TKey, TOtherPayload>, TResultPayload>> enrichSelector)
        {
            var otherGrouped = other.GroupApply(rightKeySelector,
                group => group.Aggregate(aggregate => aggregate.CollectList(item => item)),
                (key, value) => new Grouping<TKey, TOtherPayload>(key.Key, value) as IGrouping<TKey, TOtherPayload>);

            var arg = Expression.Parameter(typeof(TPayload), "arg");
            var nullGrouping = Expression.Constant(null, typeof(IGrouping<TKey, TOtherPayload>));
            var leftOnlySelectorExpr = Expression.Lambda<Func<TPayload, TResultPayload>>(Expression.Invoke(enrichSelector, arg, nullGrouping), arg);

            return source.LeftOuterJoin(otherGrouped,
                leftKeySelector,
                g => g.Key,
                leftOnlySelectorExpr,
                enrichSelector);
        }
    }
}
