using Ethanol.Streaming;
using Microsoft.StreamProcessing;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

namespace Ethanol.Demo
{
    public record FlowBurst(string SrcIp, string DstIp, int DstPort, RawIpfixRecord[] Flows);
    public record BagOfFlows(string DstIp, int DstPort, RawIpfixRecord[] Flows);
    public record FlowRelations(RawIpfixRecord TargetFlow, BagOfFlows BagOfFlows, FlowBurst FlowBurst);
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
                    key => new { Proto = key.Protocol, DstIp = key.DstIp, DstPort = key.DstPort },
                    group => group.Aggregate(aggregate => aggregate.Collect(flow => flow)),
                    (key, value) => KeyValuePair.Create(key.Key, value.Distinct()))
                .AsContextFlow(f => f.GetFlow(), (k,v) => new BagOfFlows(k.DstIp, k.DstPort, v.ToArray()));
                

            var flowBurstStream = source[1]
                .GroupApply(
                    key => new { Proto = key.Protocol, SrcIp = key.SrcIp, DstIp = key.DstIp, DstPort = key.DstPort },
                    group => group.Aggregate(aggregate => aggregate.Collect(flow => flow)),
                    (key, value) => KeyValuePair.Create(key.Key, value.Distinct()))
                .AsContextFlow(f => f.GetFlow(), (k,v) => new FlowBurst(k.SrcIp, k.DstIp, k.DstPort, v.ToArray()));

            var sourceFlowsStream = source[2].Select(f => new ContextFlow<RawIpfixRecord>(f.GetFlow(), f));

            return ContextAggregator.MergeContextFlowStreams(merger, sourceFlowsStream, bagOfFlowStream, flowBurstStream);
        }

        private static FlowRelations merger(RawIpfixRecord arg1, BagOfFlows arg2, FlowBurst arg3) => new FlowRelations(arg1, arg2, arg3);
    }
}
