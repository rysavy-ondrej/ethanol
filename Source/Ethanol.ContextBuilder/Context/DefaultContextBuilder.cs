using Ethanol.Catalogs;
using Ethanol.ContextBuilder;
using Ethanol.ContextBuilder.Builders;
using Ethanol.ContextBuilder.Context;
using Ethanol.Streaming;
using Microsoft.StreamProcessing;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

namespace Ethanol.ContextBuilder.Context
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
    public record FlowRelations(IpfixObject TargetFlow, FlowGroup<BagOfFlowsKey, IpfixObject> BagOfFlows, FlowGroup<FlowBurstKey, IpfixObject> FlowBurst);
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
        public static IStreamable<Empty, InternalContextFlow<FlowRelations>> BuildFlowContext(this ContextBuilderCatalog _, IStreamable<Empty, IpfixObject> flowStream)
        {
            var source = flowStream.Multicast(3);

            var bagOfFlowStream = source[0]
                .GroupApply(
                    key => new BagOfFlowsKey(key.DestinationIpAddress, key.DestinationPort, key.Protocol.ToString()),
                    group => group.Aggregate(aggregate => aggregate.CollectSet(flow => flow)),
                    (key, value) => KeyValuePair.Create(key.Key, value))
                .Expand(f => f.Value, (k, v) => new InternalContextFlow<FlowGroup<BagOfFlowsKey, IpfixObject>>(k.FlowKey, new FlowGroup<BagOfFlowsKey, IpfixObject>(v.Key, v.Value.ToArray())), k => k.FlowKey);


            var flowBurstStream = source[1]
                .GroupApply(
                    key => new FlowBurstKey(key.SourceIpAddress, key.DestinationIpAddress, key.DestinationPort, key.Protocol.ToString()),
                    group => group.Aggregate(aggregate => aggregate.CollectSet(flow => flow)),
                    (key, value) => KeyValuePair.Create(key.Key, value))
                .Expand(f => f.Value, (k, v) => new InternalContextFlow<FlowGroup<FlowBurstKey, IpfixObject>>(k.FlowKey, new FlowGroup<FlowBurstKey, IpfixObject>(v.Key, v.Value.ToArray())), k => k.FlowKey);

            var sourceFlowsStream = source[2].Select(f => new InternalContextFlow<IpfixObject>(f.FlowKey, f));

            return sourceFlowsStream.AggregateContextStreams(bagOfFlowStream, flowBurstStream, MergeFunc);
        }

        private static FlowRelations MergeFunc(IpfixObject[] arg1, FlowGroup<BagOfFlowsKey, IpfixObject>[] arg2, FlowGroup<FlowBurstKey, IpfixObject>[] arg3)
            => new FlowRelations(arg1.FirstOrDefault(),
                new FlowGroup<BagOfFlowsKey, IpfixObject>(arg2.FirstOrDefault()?.Key, arg2.SelectMany(v => v.Flows).ToArray()),
                new FlowGroup<FlowBurstKey, IpfixObject>(arg3.FirstOrDefault()?.Key, arg3.SelectMany(v => v.Flows).ToArray()));

        /// <summary>
        /// Gets various meters for the given IPFIX record.
        /// </summary>
        /// <param name="f">The IPFIX record.</param>
        /// <returns>A collection of meters for the given IPFIX record.</returns>
        public static FlowMeters GetMeters(this IpfixObject f)
        {
            return new FlowMeters(f.Packets, f.Bytes, f.TimeStart, f.TimeDuration);
        }
    }
}
