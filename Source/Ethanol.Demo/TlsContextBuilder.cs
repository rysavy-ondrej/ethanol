using Ethanol.Streaming;
using Microsoft.StreamProcessing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

namespace Ethanol.Demo
{
    //
    // Facts in the context are related via the defined common key.
    // For instance, other TLs flows for the given flow is related via the common (SrcIp, Ja3Fingerprint) pair.
    //
    // TODO: 
    // Can we have a general COntext class that enables to access facts using the "via-relation"?
    //
    // 


    public record TlsFlowRecord(Flow Flow, string TlsJa3, string TlsServerName, string TlsServerCommonName, string DomainName, double ServerNameEntropy, double DomainNameEntropy);
    public record TlsContext(TlsFlowRecord TlsRecord, FlowGroup<TlsClientKey, TlsFlowRecord> TlsClientFlows, FlowGroup<BagOfFlowsKey, TlsFlowRecord> BagOfFlows);
    public record TlsClientKey(string SrcIp, string Ja3Fingerprint);
    public static class TlsContextBuilder
    {
        /// <summary>
        /// Creates a TLS facts related to the given flow.
        /// </summary>
        /// <param name="source">Flow stream.</param>
        /// <param name="configuration">Configuration.</param>
        /// <returns>A stream of flows with their contexts.</returns>
        public static IStreamable<Empty, ContextFlow<TlsContext>> BuildTlsContext(this ContextBuilder _, IStreamable<Empty, RawIpfixRecord> source)
        {
            try
            {
                var flowStreams = source.Multicast(2);
                var tlsStream = flowStreams[0].Where(x => x.TlsClientVersion != "N/A" && x.SrcPort > x.DstPort);
                var dnsStream = flowStreams[1].Where(x => x.Protocol == "UDP" && x.SrcPort == 53);

                // enrich TLS with associated DNS queries
                // it produces for every flow exactly one record - ok!
                var tlsDomainStream = tlsStream.LeftOuterJoin(dnsStream,
                    flow => new { SrcIp = flow.SrcIp, DstIp = flow.DstIp },
                    flow => new { SrcIp = flow.DstIp, DstIp = flow.DnsResponseData },
                    left => new TlsFlowRecord(left.GetFlow(), left.TlsJa3, left.TlsServerName, left.TlsServerCommonName, string.Empty, Statistics.ComputeDnsEntropy(left.TlsServerName).Max(), 0),
                    (left, right) => new TlsFlowRecord(left.GetFlow(), left.TlsJa3, left.TlsServerName, left.TlsServerCommonName, right.DnsQueryName, Statistics.ComputeDnsEntropy(left.TlsServerName).Max(), Statistics.ComputeDnsEntropy(right.TlsServerName).Max()))
                    // group results and select only the first one:
                    .GroupApply(
                        f => f.Flow,
                        group => group.Aggregate(aggregate => aggregate.CollectList(flow => flow)),
                        (key, value) => value.FirstOrDefault());

                //return tlsDomainStream.Select(f => new ContextFlow<TlsContext>(f.Flow, null));

                return tlsDomainStream.Multicast(stream =>
                {
                    // collect all flows from the same source host with the same JA3:
                    var tlsClientFlowsStream = stream.MatchGroupApply(
                        flow => new TlsClientKey(flow.Flow.SrcIp, flow.TlsJa3),
                        flow => ValueTuple.Create(flow.Flow, new TlsClientKey(flow.Flow.SrcIp, flow.TlsJa3)),
                        grouping => new ContextFlow<FlowGroup<TlsClientKey, TlsFlowRecord>>(
                            grouping.Key.Item1,
                            new FlowGroup<TlsClientKey, TlsFlowRecord>(grouping.Key.Item2, grouping.ToArray())
                        ));
                    //return tlsClientFlowsStream.Select(x=> new ContextFlow<TlsContext>(x.Flow, new TlsContext(x.Context.Flows.First(), x.Context, null)));

                    // collect a bag of flows:
                    var bagOfFlowsStream = stream.MatchGroupApply(
                        flow => new BagOfFlowsKey(flow.Flow.DstIp, flow.Flow.DstPt, flow.Flow.Proto),
                        flow => ValueTuple.Create(flow.Flow, new BagOfFlowsKey(flow.Flow.DstIp, flow.Flow.DstPt, flow.Flow.Proto)),
                        grouping => new ContextFlow<FlowGroup<BagOfFlowsKey, TlsFlowRecord>>(
                            grouping.Key.Item1,
                            new FlowGroup<BagOfFlowsKey, TlsFlowRecord>(grouping.Key.Item2, grouping.ToArray())
                        ));
                    //return bagOfFlowsStream.Select(x => new ContextFlow<TlsContext>(x.Flow, new TlsContext(x.Context.Flows.First(), null, x.Context)));
                    return stream.Select(f => new ContextFlow<TlsFlowRecord>(f.Flow, f)).MergeContextFlowStreams(tlsClientFlowsStream, bagOfFlowsStream, (x, y, z) => new TlsContext(x, y, z));
                });
            }
            catch(Exception e)
            {
                Console.Error.WriteLine(e);
                throw;
            }
        }
    }
}
