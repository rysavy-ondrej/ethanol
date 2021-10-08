﻿using Ethanol.Streaming;
using Microsoft.StreamProcessing;
using System;
using System.Linq;
using System.Reactive.Linq;

namespace Ethanol.Demo
{
    public record EndpointsKey(string SrcIp, string DstIp);
    public record TlsFlowRecord(Flow Flow, string TlsJa3, string TlsServerName, string TlsServerCommonName, double ServerNameEntropy, double ServerCommonNameEntropy);
    public record DnsFlowRecord(Flow Flow, string QueryName, string ResponseData);
    public record HttpFlowRecord(Flow Flow, string Method, string HostName, string Url);
    public record TlsContext(TlsFlowRecord TlsRecord, FlowGroup<EndpointsKey, DnsFlowRecord> Domains, FlowGroup<TlsClientKey, TlsFlowRecord> TlsClientFlows, FlowGroup<BagOfFlowsKey, TlsFlowRecord> BagOfFlows, FlowGroup<FlowBurstKey, TlsFlowRecord> FlowBurst, FlowGroup<EndpointsKey, HttpFlowRecord> PlainHttpFlows);
    public record TlsClientKey(string SrcIp, string Ja3Fingerprint);
    public static class TlsContextBuilder
    {
        /// <summary>
        /// Creates a TLS facts related to the given flow.
        /// </summary>
        /// <param name="source">Flow stream.</param>
        /// <param name="configuration">Configuration.</param>
        /// <returns>A stream of flows with their contexts.</returns>
        public static IStreamable<Empty, ContextFlow<TlsContext>> BuildTlsContext(this ContextBuilder _, IStreamable<Empty, IpfixRecord> source)
        {
            try
            {
                return source.Multicast(flowStream =>
                {
                    var tlsStream = flowStream.Where(x => x.TlsClientVersion != "N/A" && x.SrcPort > x.DstPort).Select(f => new TlsFlowRecord(f.GetFlow(), f.TlsJa3, f.TlsServerName, f.TlsServerCommonName, Statistics.ComputeDnsEntropy(f.TlsServerName).Max(), Statistics.ComputeDnsEntropy(f.TlsServerCommonName).Max()));
                    var dnsStream = flowStream.Where(x => x.Protocol == "UDP" && x.SrcPort == 53).Select(f => new DnsFlowRecord(f.GetFlow(), f.DnsQueryName, f.DnsResponseData));
                    var httpStream = flowStream.Where(x => x.Protocol == "TCP" && !string.IsNullOrWhiteSpace(x.hurl)).Select(f => new HttpFlowRecord(f.GetFlow(), f.hmethod, f.hhost, f.hurl));

                    var flowContextStream = tlsStream.Multicast(stream =>
                    {
                        var dnsClientFlowsStream = stream
                            .Join(dnsStream,
                                flow => new EndpointsKey(flow.Flow.SrcIp, flow.Flow.DstIp),
                                flow => new EndpointsKey(flow.Flow.DstIp, flow.ResponseData),
                                (left, right) => new { Flow = left.Flow, Key = new EndpointsKey(right.Flow.DstIp, right.ResponseData), Value = right })
                            .GroupApply(
                                flow => new { flow.Flow, flow.Key },
                                group => group.Aggregate(aggregate => aggregate.CollectList(flow => flow.Value)),
                                (key, value) => new ContextFlow<FlowGroup<EndpointsKey, DnsFlowRecord>>(key.Key.Flow, new FlowGroup<EndpointsKey, DnsFlowRecord>(key.Key.Key, value.Distinct().ToArray()))
                               );

                        var httpClientFlowsStream = stream
                            .Join(httpStream,
                                flow => new EndpointsKey(flow.Flow.SrcIp, flow.Flow.DstIp),
                                flow => new EndpointsKey(flow.Flow.SrcIp, flow.Flow.DstIp),
                                (left, right) => new { Flow = left.Flow, Key = new EndpointsKey(right.Flow.SrcIp, right.Flow.DstIp), Value = right })
                            .GroupApply(
                                flow => new { flow.Flow, flow.Key },
                                group => group.Aggregate(aggregate => aggregate.CollectList(flow => flow.Value)),
                                (key, value) => new ContextFlow<FlowGroup<EndpointsKey, HttpFlowRecord>>(key.Key.Flow, new FlowGroup<EndpointsKey, HttpFlowRecord>(key.Key.Key, value.Distinct().ToArray()))
                               );

                        var tlsClientFlowsStream = stream.MatchGroupApply(
                        flow => new TlsClientKey(flow.Flow.SrcIp, flow.TlsJa3),
                        flow => ValueTuple.Create(flow.Flow, new TlsClientKey(flow.Flow.SrcIp, flow.TlsJa3)),
                        grouping => new ContextFlow<FlowGroup<TlsClientKey, TlsFlowRecord>>(
                            grouping.Key.Item1,
                            new FlowGroup<TlsClientKey, TlsFlowRecord>(grouping.Key.Item2, grouping.ToArray())
                        ));

                        var bagOfFlowsStream = stream.MatchGroupApply(
                        flow => new BagOfFlowsKey(flow.Flow.DstIp, flow.Flow.DstPt, flow.Flow.Proto),
                        flow => ValueTuple.Create(flow.Flow, new BagOfFlowsKey(flow.Flow.DstIp, flow.Flow.DstPt, flow.Flow.Proto)),
                        grouping => new ContextFlow<FlowGroup<BagOfFlowsKey, TlsFlowRecord>>(
                            grouping.Key.Item1,
                            new FlowGroup<BagOfFlowsKey, TlsFlowRecord>(grouping.Key.Item2, grouping.ToArray())
                        ));

                        var flowBurstStream = stream.MatchGroupApply(
                            flow => new FlowBurstKey(flow.Flow.SrcIp, flow.Flow.DstIp, flow.Flow.DstPt, flow.Flow.Proto),
                            flow => ValueTuple.Create(flow.Flow, new FlowBurstKey(flow.Flow.SrcIp, flow.Flow.DstIp, flow.Flow.DstPt, flow.Flow.Proto)),
                            grouping => new ContextFlow<FlowGroup<FlowBurstKey, TlsFlowRecord>>(grouping.Key.Item1,
                                new FlowGroup<FlowBurstKey, TlsFlowRecord>(grouping.Key.Item2, grouping.ToArray())
                            ));

                        return stream
                            .Select(f => new ContextFlow<TlsFlowRecord>(f.Flow, f))
                            .AggregateContextStreams(dnsClientFlowsStream, tlsClientFlowsStream, bagOfFlowsStream, flowBurstStream, httpClientFlowsStream, AggregateContext);
                    });
                    return flowContextStream;
                });
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e);
                throw;
            }
        }

        private static TlsContext AggregateContext(
            TlsFlowRecord[] arg1,
            FlowGroup<EndpointsKey, DnsFlowRecord>[] arg2,
            FlowGroup<TlsClientKey, TlsFlowRecord>[] arg3,
            FlowGroup<BagOfFlowsKey, TlsFlowRecord>[] arg4,
            FlowGroup<FlowBurstKey, TlsFlowRecord>[] arg5,
            FlowGroup<EndpointsKey, HttpFlowRecord>[] arg6
        )
        {
            return new TlsContext(arg1.FirstOrDefault(x => x != null), arg2.Aggregate(), arg3.Aggregate(), arg4.Aggregate(), arg5.Aggregate(), arg6.Aggregate());
        }
    }
}
