using Ethanol.ContextBuilder.Builders;
using Ethanol.Streaming;
using Microsoft.StreamProcessing;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ethanol.ContextBuilder.Context
{
    /// <summary>
    /// Static class implements various useful aggregation methods.
    /// </summary>
    static class ContextAggregator
    {
        /// <summary>
        /// A default aggregator for the collection of items to be used together with <see cref="AggregateContextStreams"/>. 
        /// <para/>
        /// It reads the source collection, expands all non-null elements, and collects it in a single output of the same type. 
        /// The flow key for the resulting group is the first valid one found in the source flow groups.
        /// </summary>
        /// <typeparam name="TKey">The key type of the group.</typeparam>
        /// <typeparam name="TValue">The type of flow collection in the group.</typeparam>
        /// <param name="source">The source enumerable of flow groups.</param>
        /// <param name="removeDuplicities">true if duplicate elements should be removed.</param>
        /// <returns>The flow group composed from the multiple source flow groups.</returns>
        public static FlowGroup<TKey, TValue> Aggregate<TKey, TValue>(this IEnumerable<FlowGroup<TKey, TValue>> source, bool removeDuplicities = true)
        {
            var input = source.Where(x => x != null).ToList();
            var first = input.FirstOrDefault();
            var key = first != null ? first.Key : default;
            var elements = input.SelectMany(x => x.Flows).Distinct();
            return new FlowGroup<TKey, TValue>(key, (removeDuplicities ? elements.Distinct() : elements).ToArray());
        }

        public static IStreamable<TStreamKey, KeyValuePair<TKey, TTarget>> AggregateContextStreams<TStreamKey, TKey, TSource1, TSource2, TTarget>(
            this IStreamable<TStreamKey, KeyValuePair<TKey, TSource1>> source1,
            IStreamable<TStreamKey, KeyValuePair<TKey, TSource2>> source2,
            Func<TSource1[], TSource2[], TTarget> aggregator)
            where TSource1 : class where TSource2 : class
        {
            var union = source1.Select(m => new { Flow = m.Key, Item1 = m.Value, Item2 = default(TSource2) })
                 .Union(source2.Select(m => new { Flow = m.Key, Item1 = default(TSource1), Item2 = m.Value }));

            return union.GroupAggregate(
                key => key.Flow,
                agg1 => agg1.CollectList(x => x.Item1),
                agg2 => agg2.CollectList(x => x.Item2),
                (key, val1, val2) => new KeyValuePair<TKey, TTarget>(key.Key, aggregator(val1, val2)));
        }

        /// <summary>
        /// Merges three context streams using <paramref name="aggregator"/> operator.
        /// <para/>
        /// The method uses union internally and depends on <paramref name="aggregator"/> function to properly 
        /// join the individual records from all sourced streams. The records are grouped by flow key and 
        /// forwared to <paramref name="aggregator"/> function that produces a single output for each group.
        /// </summary>
        /// <typeparam name="TSource1"></typeparam>
        /// <typeparam name="TSource2"></typeparam>
        /// <typeparam name="TSource3"></typeparam>
        /// <typeparam name="TTarget"></typeparam>
        /// <param name="source1">The first stream.</param>
        /// <param name="source2">The second stream.</param>
        /// <param name="source3">The third stream.</param>
        /// <param name="aggregator">A function used to merge records. The arguments are arrays of values. Note that these arrays may be sparse, i.e., some elements may be null.</param>
        /// <returns>A context flow stream of the <typeparamref name="TTarget"/> context type.</returns>
        public static IStreamable<TStreamKey, KeyValuePair<TKey, TTarget>> AggregateContextStreams<TStreamKey, TKey, TSource1, TSource2, TSource3, TTarget>(
            this IStreamable<TStreamKey, KeyValuePair<TKey, TSource1>> source1,
            IStreamable<TStreamKey, KeyValuePair<TKey, TSource2>> source2,
            IStreamable<TStreamKey, KeyValuePair<TKey, TSource3>> source3,
            Func<TSource1[], TSource2[], TSource3[], TTarget> aggregator)
            where TSource1 : class where TSource2 : class where TSource3 : class
        {
            var union = source1.Select(m => new { Flow = m.Key, Item1 = m.Value, Item2 = default(TSource2), Item3 = default(TSource3) })
                 .Union(source2.Select(m => new { Flow = m.Key, Item1 = default(TSource1), Item2 = m.Value, Item3 = default(TSource3) }))
                 .Union(source3.Select(m => new { Flow = m.Key, Item1 = default(TSource1), Item2 = default(TSource2), Item3 = m.Value }));

            return union.GroupAggregate(
                key => key.Flow,
                agg1 => agg1.CollectList(x => x.Item1),
                agg2 => agg2.CollectList(x => x.Item2),
                agg3 => agg3.CollectList(x => x.Item3),
                (key, val1, val2, val3) => new KeyValuePair<TKey, TTarget>(key.Key, aggregator(val1, val2, val3)));
        }
        public static IStreamable<TStreamKey, KeyValuePair<TKey,TTarget>> AggregateContextStreams<TStreamKey, TKey, TSource1, TSource2, TSource3, TSource4, TTarget>(
            this IStreamable<TStreamKey, KeyValuePair<TKey,TSource1>> source1,
                 IStreamable<TStreamKey, KeyValuePair<TKey,TSource2>> source2,
                 IStreamable<TStreamKey, KeyValuePair<TKey,TSource3>> source3,
                 IStreamable<TStreamKey, KeyValuePair<TKey,TSource4>> source4,
                 Func<TSource1[], TSource2[], TSource3[], TSource4[], TTarget> aggregator)
        where TSource1 : class where TSource2 : class where TSource3 : class where TSource4 : class
        {
            var union = source1.Select(m => new { Flow = m.Key, Item1 = m.Value, Item2 = default(TSource2), Item3 = default(TSource3), Item4 = default(TSource4) })
                 .Union(source2.Select(m => new { Flow = m.Key, Item1 = default(TSource1), Item2 = m.Value, Item3 = default(TSource3), Item4 = default(TSource4) }))
                 .Union(source3.Select(m => new { Flow = m.Key, Item1 = default(TSource1), Item2 = default(TSource2), Item3 = m.Value, Item4 = default(TSource4) }))
                 .Union(source4.Select(m => new { Flow = m.Key, Item1 = default(TSource1), Item2 = default(TSource2), Item3 = default(TSource3), Item4 = m.Value }));

            return union.GroupAggregate(
                key => key.Flow,
                agg1 => agg1.CollectList(x => x.Item1),
                agg2 => agg2.CollectList(x => x.Item2),
                agg3 => agg3.CollectList(x => x.Item3),
                agg4 => agg4.CollectList(x => x.Item4),
                (key, val1, val2, val3, val4) => new KeyValuePair<TKey, TTarget>(key.Key, aggregator(val1, val2, val3, val4)));
        }

        public static IStreamable<TStreamKey, TTarget> UnionGroupAggregate<TStreamKey, TKey, TSource1, TSource2, TSource3, TSource4, TSource5, TTarget>(
        this IStreamable<TStreamKey, TSource1> source1,
             IStreamable<TStreamKey, TSource2> source2,
             IStreamable<TStreamKey, TSource3> source3,
             IStreamable<TStreamKey, TSource4> source4,
             IStreamable<TStreamKey, TSource5> source5,
             Func<TSource1, TKey> getSelectorKey1,
             Func<TSource2, TKey> getSelectorKey2,
             Func<TSource3, TKey> getSelectorKey3,
             Func<TSource4, TKey> getSelectorKey4,
             Func<TSource5, TKey> getSelectorKey5,
             Func<TKey, TSource1[], TSource2[], TSource3[], TSource4[], TSource5[], TTarget> aggregator)
        where TSource1 : class where TSource2 : class where TSource3 : class where TSource4 : class where TSource5 : class
        {
            var union = source1.Select(m => new { Key = getSelectorKey1(m), Item1 = m, Item2 = default(TSource2), Item3 = default(TSource3), Item4 = default(TSource4), Item5 = default(TSource5) })
                 .Union(source2.Select(m => new { Key = getSelectorKey2(m), Item1 = default(TSource1), Item2 = m, Item3 = default(TSource3), Item4 = default(TSource4), Item5 = default(TSource5) }))
                 .Union(source3.Select(m => new { Key = getSelectorKey3(m), Item1 = default(TSource1), Item2 = default(TSource2), Item3 = m, Item4 = default(TSource4), Item5 = default(TSource5) }))
                 .Union(source4.Select(m => new { Key = getSelectorKey4(m), Item1 = default(TSource1), Item2 = default(TSource2), Item3 = default(TSource3), Item4 = m, Item5 = default(TSource5) }))
                 .Union(source5.Select(m => new { Key = getSelectorKey5(m), Item1 = default(TSource1), Item2 = default(TSource2), Item3 = default(TSource3), Item4 = default(TSource4), Item5 = m }));

            return union.GroupAggregate(
                key => key.Key,
                agg1 => agg1.CollectList(x => x.Item1),
                agg2 => agg2.CollectList(x => x.Item2),
                agg3 => agg3.CollectList(x => x.Item3),
                agg4 => agg4.CollectList(x => x.Item4),
                agg5 => agg5.CollectList(x => x.Item5),
                (key, val1, val2, val3, val4, val5) => aggregator(key.Key, val1, val2, val3, val4, val5));
        }

        public static IStreamable<TStreamKey, KeyValuePair<TKey,TTarget>> AggregateContextStreams<TStreamKey, TKey, TSource1, TSource2, TSource3, TSource4, TSource5, TTarget>(
            this IStreamable<TStreamKey, KeyValuePair<TKey,TSource1>> source1,
            IStreamable<TStreamKey, KeyValuePair<TKey,TSource2>> source2,
            IStreamable<TStreamKey, KeyValuePair<TKey,TSource3>> source3,
            IStreamable<TStreamKey, KeyValuePair<TKey,TSource4>> source4,
            IStreamable<TStreamKey, KeyValuePair<TKey,TSource5>> source5,
            Func<TSource1[], TSource2[], TSource3[], TSource4[], TSource5[], TTarget> aggregator)
            where TSource1 : class where TSource2 : class where TSource3 : class where TSource4 : class
        {
            var union = source1.Select(m => new { Flow = m.Key, Item1 = m.Value, Item2 = default(TSource2), Item3 = default(TSource3), Item4 = default(TSource4), Item5 = default(TSource5) })
                 .Union(source2.Select(m => new { Flow = m.Key, Item1 = default(TSource1), Item2 = m.Value, Item3 = default(TSource3), Item4 = default(TSource4), Item5 = default(TSource5) }))
                 .Union(source3.Select(m => new { Flow = m.Key, Item1 = default(TSource1), Item2 = default(TSource2), Item3 = m.Value, Item4 = default(TSource4), Item5 = default(TSource5) }))
                 .Union(source4.Select(m => new { Flow = m.Key, Item1 = default(TSource1), Item2 = default(TSource2), Item3 = default(TSource3), Item4 = m.Value, Item5 = default(TSource5) }))
                 .Union(source5.Select(m => new { Flow = m.Key, Item1 = default(TSource1), Item2 = default(TSource2), Item3 = default(TSource3), Item4 = default(TSource4), Item5 = m.Value }));

            return union.GroupAggregate(
                key => key.Flow,
                agg1 => agg1.CollectList(x => x.Item1),
                agg2 => agg2.CollectList(x => x.Item2),
                agg3 => agg3.CollectList(x => x.Item3),
                agg4 => agg4.CollectList(x => x.Item4),
                 agg5 => agg5.CollectList(x => x.Item5),
                (key, val1, val2, val3, val4, val5) => new KeyValuePair<TKey,TTarget>(key.Key, aggregator(val1, val2, val3, val4, val5)));
        }
        public static IStreamable<TStreamKey, KeyValuePair<TKey,TTarget>> AggregateContextStreams<TStreamKey, TKey, TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TTarget>(
            this IStreamable<TStreamKey, KeyValuePair<TKey,TSource1>> source1,
            IStreamable<TStreamKey, KeyValuePair<TKey,TSource2>> source2,
            IStreamable<TStreamKey, KeyValuePair<TKey,TSource3>> source3,
            IStreamable<TStreamKey, KeyValuePair<TKey,TSource4>> source4,
            IStreamable<TStreamKey, KeyValuePair<TKey,TSource5>> source5,
            IStreamable<TStreamKey, KeyValuePair<TKey,TSource6>> source6,
            Func<TSource1[], TSource2[], TSource3[], TSource4[], TSource5[], TSource6[], TTarget> aggregator)
            where TSource1 : class where TSource2 : class where TSource3 : class where TSource4 : class
        {
            var union = source1.Select(m => new { Flow = m.Key, Item1 = m.Value, Item2 = default(TSource2), Item3 = default(TSource3), Item4 = default(TSource4), Item5 = default(TSource5), Item6 = default(TSource6) })
                 .Union(source2.Select(m => new { Flow = m.Key, Item1 = default(TSource1), Item2 = m.Value, Item3 = default(TSource3), Item4 = default(TSource4), Item5 = default(TSource5), Item6 = default(TSource6) }))
                 .Union(source3.Select(m => new { Flow = m.Key, Item1 = default(TSource1), Item2 = default(TSource2), Item3 = m.Value, Item4 = default(TSource4), Item5 = default(TSource5), Item6 = default(TSource6) }))
                 .Union(source4.Select(m => new { Flow = m.Key, Item1 = default(TSource1), Item2 = default(TSource2), Item3 = default(TSource3), Item4 = m.Value, Item5 = default(TSource5), Item6 = default(TSource6) }))
                 .Union(source5.Select(m => new { Flow = m.Key, Item1 = default(TSource1), Item2 = default(TSource2), Item3 = default(TSource3), Item4 = default(TSource4), Item5 = m.Value, Item6 = default(TSource6) }))
                 .Union(source6.Select(m => new { Flow = m.Key, Item1 = default(TSource1), Item2 = default(TSource2), Item3 = default(TSource3), Item4 = default(TSource4), Item5 = default(TSource5), Item6 = m.Value }));

            return union.GroupAggregate(
                key => key.Flow,
                agg1 => agg1.CollectList(x => x.Item1),
                agg2 => agg2.CollectList(x => x.Item2),
                agg3 => agg3.CollectList(x => x.Item3),
                agg4 => agg4.CollectList(x => x.Item4),
                agg5 => agg5.CollectList(x => x.Item5),
                agg6 => agg6.CollectList(x => x.Item6),
                (key, val1, val2, val3, val4, val5, val6) => new KeyValuePair<TKey,TTarget>(key.Key, aggregator(val1, val2, val3, val4, val5, val6)));
        }
    }
}
