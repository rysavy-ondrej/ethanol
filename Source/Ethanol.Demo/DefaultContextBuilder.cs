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
    public record FlowBurstKey(string SrcIp, string DstIp, int DstPort, string Protocol);
    public record BagOfFlowsKey(string DstIp, int DstPort, string Protocol);
    public record AppClientFlowsKey(string SrcIp, string DstPort, string Protocol);

    /// <summary>
    /// Group of flows with the given common <paramref name="Key"/>.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TValue">The type representing the flows records.</typeparam>
    /// <param name="Key">The key of group of flows.</param>
    /// <param name="Flows">The collection of flows.</param>
    public record FlowGroup<TKey, TValue>(TKey Key, TValue[] Flows);

    /// <summary>
    /// Records with the basic relations among the flows.
    /// </summary>
    /// <param name="TargetFlow"></param>
    /// <param name="BagOfFlows"></param>
    /// <param name="FlowBurst"></param>
    public record FlowRelations(RawIpfixRecord TargetFlow, FlowGroup<BagOfFlowsKey, RawIpfixRecord> BagOfFlows, FlowGroup<FlowBurstKey, RawIpfixRecord> FlowBurst);
    public static class DefaultContextBuilder
    { 
        /// <summary>
        /// Builds a general flow context. 
        /// </summary>
        /// <remarks>
        /// The general flow context consist of different type of flow relations according to the shared fields. From CLIENT point of view:
        /// * SrcIp, DstIp, DstPort - multiple connections to the same target service, e.g., web communication with a single server, aka FLOW-BURST
        /// * SrcIp, DstPort - connections to internet application APPCLIENT-FLOWS
        /// * DstIp, DstPort - all communications to the specific server service aka BAG-OF-FLOWS
        /// * SrcIp, Ja3 - TLSCLIENT-FLOWS
        /// </remarks>
        /// <param name="flowStream"></param>
        /// <returns></returns>
        public static IStreamable<Empty, ContextFlow<FlowRelations>> BuildFlowContext(this ContextBuilder _,IStreamable<Empty, RawIpfixRecord> flowStream)
        {
            var source = flowStream.Multicast(3);

            var bagOfFlowStream = source[0]
                .GroupApply(
                    key => new BagOfFlowsKey (key.DstIp,  key.DstPort, key.Protocol),
                    group => group.Aggregate(aggregate => aggregate.CollectSet(flow => flow)),
                    (key, value) => KeyValuePair.Create(key.Key, value))
                .Expand(f => f.Value, (k,v) => new ContextFlow<FlowGroup<BagOfFlowsKey, RawIpfixRecord>>(k.GetFlow(), new FlowGroup<BagOfFlowsKey, RawIpfixRecord>(v.Key, v.Value.ToArray())), k => k.Flow);
                

            var flowBurstStream = source[1]
                .GroupApply(
                    key => new FlowBurstKey (key.SrcIp, key.DstIp, key.DstPort, key.Protocol),
                    group => group.Aggregate(aggregate => aggregate.CollectSet(flow => flow)),
                    (key, value) => KeyValuePair.Create(key.Key, value))
                .Expand(f=>f.Value,(k,v) => new ContextFlow<FlowGroup<FlowBurstKey, RawIpfixRecord>>(k.GetFlow(), new FlowGroup<FlowBurstKey, RawIpfixRecord>(v.Key, v.Value.ToArray())), k => k.Flow);

            var sourceFlowsStream = source[2].Select(f => new ContextFlow<RawIpfixRecord>(f.GetFlow(), f));

            return ContextAggregator.MergeContextFlowStreams(sourceFlowsStream, bagOfFlowStream, flowBurstStream, MergeFunc);
        }

        private static FlowRelations MergeFunc(RawIpfixRecord arg1, FlowGroup<BagOfFlowsKey, RawIpfixRecord> arg2, FlowGroup<FlowBurstKey, RawIpfixRecord> arg3) => new FlowRelations(arg1, arg2, arg3);
    
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
        public static IStreamable<TEmpty, TTarget> MatchGroupApply<TEmpty,TSource,TMatchKey,TResultKey,TTarget>(
            this IStreamable<TEmpty,TSource> source, 
            Expression<Func<TSource, TMatchKey>> matchKeySelector,
            Expression<Func<TSource, TResultKey>> resultKeySelector,
            Expression<Func<IGrouping<TResultKey, TSource>,TTarget>> resultSelector) where TSource : IEquatable<TSource>

        {
            var correlatedStream = source.Multicast(input =>
                input.Join(input, matchKeySelector, matchKeySelector,
                    (left, right) => KeyValuePair.Create(left, right)
                    )
            );
            var arg = Expression.Parameter(typeof(KeyValuePair<TSource, TSource>),"input");
            var expr = Expression.Lambda<Func<KeyValuePair<TSource,TSource>, TResultKey>>(Expression.Invoke(resultKeySelector, Expression.Property(arg, nameof(KeyValuePair<TSource, TSource>.Key))), arg);
            var groupStream = correlatedStream.GroupApply(
                expr,
                group => group.Aggregate(aggregate => aggregate.CollectList(item => item.Value)),
                (key, value) => new Grouping<TResultKey, TSource>(key.Key, value) as IGrouping<TResultKey, TSource>);

            return groupStream.Select(resultSelector);
        }
        public class Grouping<TKey, TElement> : IGrouping<TKey, TElement>
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
