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
    /// <summary>
    /// Record representing key fields for Flow Burst.
    /// </summary>
    public record FlowBurstKey(string SrcIp, string DstIp, int DstPort, string Protocol);
    /// <summary>
    /// Record representing key fields for Bag of Flows.
    /// </summary>
    public record BagOfFlowsKey(string DstIp, int DstPort, string Protocol);

    /// <summary>
    /// Record representing key fields for single Client Bag of Flows.
    /// </summary>
    public record ClientBagOfFlowsKey(string SrcIp, string DstPort, string Protocol);

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
    public record FlowRelations(IpfixRecord TargetFlow, FlowGroup<BagOfFlowsKey, IpfixRecord> BagOfFlows, FlowGroup<FlowBurstKey, IpfixRecord> FlowBurst);
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
        public static IStreamable<Empty, ContextFlow<FlowRelations>> BuildFlowContext(this ContextBuilderCatalog _,IStreamable<Empty, IpfixRecord> flowStream)
        {
            var source = flowStream.Multicast(3);

            var bagOfFlowStream = source[0]
                .GroupApply(
                    key => new BagOfFlowsKey (key.DstIp,  key.DstPort, key.Protocol),
                    group => group.Aggregate(aggregate => aggregate.CollectSet(flow => flow)),
                    (key, value) => KeyValuePair.Create(key.Key, value))
                .Expand(f => f.Value, (k,v) => new ContextFlow<FlowGroup<BagOfFlowsKey, IpfixRecord>>(k.FlowKey, new FlowGroup<BagOfFlowsKey, IpfixRecord>(v.Key, v.Value.ToArray())), k => k.Flow);
                

            var flowBurstStream = source[1]
                .GroupApply(
                    key => new FlowBurstKey (key.SrcIp, key.DstIp, key.DstPort, key.Protocol),
                    group => group.Aggregate(aggregate => aggregate.CollectSet(flow => flow)),
                    (key, value) => KeyValuePair.Create(key.Key, value))
                .Expand(f=>f.Value,(k,v) => new ContextFlow<FlowGroup<FlowBurstKey, IpfixRecord>>(k.FlowKey, new FlowGroup<FlowBurstKey, IpfixRecord>(v.Key, v.Value.ToArray())), k => k.Flow);

            var sourceFlowsStream = source[2].Select(f => new ContextFlow<IpfixRecord>(f.FlowKey, f));

            return ContextAggregator.AggregateContextStreams(sourceFlowsStream, bagOfFlowStream, flowBurstStream, MergeFunc);
        }

        private static FlowRelations MergeFunc(IpfixRecord[] arg1, FlowGroup<BagOfFlowsKey, IpfixRecord>[] arg2, FlowGroup<FlowBurstKey, IpfixRecord>[] arg3) 
            =>  new FlowRelations(arg1.FirstOrDefault(), 
                new FlowGroup<BagOfFlowsKey, IpfixRecord>(arg2.FirstOrDefault()?.Key, arg2.SelectMany(v => v.Flows).ToArray()),
                new FlowGroup<FlowBurstKey, IpfixRecord>(arg3.FirstOrDefault()?.Key, arg3.SelectMany(v => v.Flows).ToArray()));
    
        /// <summary>
        /// Gets various meters for the given IPFIX record.
        /// </summary>
        /// <param name="f">The IPFIX record.</param>
        /// <returns>A collection of meters for the given IPFIX record.</returns>
        public static FlowMeters GetMeters(this IpfixRecord f)
        {            
            return new FlowMeters(f.InPackets, f.InBytes, TimeSpan.FromSeconds(f.TimeDuration));
        }
    }
}
