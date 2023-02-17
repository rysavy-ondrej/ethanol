using Ethanol.Catalogs;
using Ethanol.ContextBuilder.Context;
using Ethanol.ContextBuilder.Plugins.Attributes;
using Ethanol.Streaming;
using Microsoft.StreamProcessing;
using System;
using System.Collections.Generic;
using System.Linq;
using YamlDotNet.Serialization;

namespace Ethanol.ContextBuilder.Builders
{
    public record FlowMeters(int Packets, int Octets, DateTime TimeStart, TimeSpan Duration);
    public record EndpointsKey(string SrcIp, string DstIp);
    public record TlsFlowRecord(FlowKey Flow, FlowMeters Meters, string TlsJa3, string TlsServerName, string TlsServerCommonName, string ProcessName);
    public record DnsFlowRecord(FlowKey Flow, FlowMeters Meters, string QueryName, string ResponseData);
    public record HttpFlowRecord(FlowKey Flow, FlowMeters Meters, string Method, string HostName, string Url, string ProcessName);
    public record HttpsFlowRecord(FlowKey Flow, FlowMeters Meters, string Method, string HostName, string Url, string ProcessName);
    public record TlsContext(TlsFlowRecord TlsRecord,
        FlowGroup<EndpointsKey, DnsFlowRecord> Domains,
        FlowGroup<TlsClientKey, TlsFlowRecord> TlsClientFlows,
        FlowGroup<BagOfFlowsKey, TlsFlowRecord> BagOfFlows,
        FlowGroup<FlowBurstKey, TlsFlowRecord> FlowBurst,
        FlowGroup<EndpointsKey, HttpFlowRecord> PlainHttpFlows);
    public record TlsClientKey(string SrcIp, string Ja3Fingerprint);


    /// <summary>
    /// Implements TLS flow context builder.
    /// </summary>
    [Plugin(PluginType.Builder, "FlowContext", "Builds the context for TLS flows in the source IPFIX stream.")]
    public class FlowContextBuilder : ContextBuilder<IpFlow, KeyValuePair<FlowKey, TlsContext>, FlowWithContext>
    {

        public FlowContextBuilder(TimeSpan windowSize, TimeSpan windowHop) : base(new IpfixObservableStream(windowSize, windowHop))
        {
        }

        public class Configuration
        {
            [YamlMember(Alias = "window", Description = "The time span of window.")]
            public TimeSpan Window { get; set; } = TimeSpan.FromSeconds(60);
            [YamlMember(Alias = "hop", Description = "The time span of window hop.")]
            public TimeSpan Hop { get; set; } = TimeSpan.FromSeconds(30);
        }

        [PluginCreate]
        internal static IContextBuilder<IpFlow, object> Create(Configuration configuration)
        {
            return new FlowContextBuilder(configuration.Window, configuration.Hop);
        }

        public override IStreamable<Empty, KeyValuePair<FlowKey, TlsContext>> BuildContext(IStreamable<Empty, IpFlow> source)
        {
            return BuildTlsContext(source);
        }

        private IStreamable<Empty, KeyValuePair<FlowKey, TlsContext>> BuildTlsContext(IStreamable<Empty, IpFlow> source)
        {
            try
            {
                return source.Multicast(flowStream =>
                {
                    var tlsStream = flowStream.Where(x => x is TlsFlow && x.SourcePort > x.DestinationPort).Select(f => f as TlsFlow).Select(f => new TlsFlowRecord(f.FlowKey, f.GetMeters(), f.JA3Fingerprint, f.ServerNameIndication, f.SubjectCommonName, f.ApplicationTag));
                    var dnsStream = flowStream.Where(x => x is DnsFlow).Select(f => f as DnsFlow).Select(f => new DnsFlowRecord(f.FlowKey, f.GetMeters(), f.QuestionName, f.ResponseData));
                    var httpStream = flowStream.Where(x => x is HttpFlow).Select(f => f as HttpFlow).Select(f => new HttpFlowRecord(f.FlowKey, f.GetMeters(), f.Method, f.Hostname, f.URL, f.ApplicationTag));

                    var flowContextStream = tlsStream.Multicast(stream =>
                    {
                        var dnsClientFlowsStream = stream
                            .Join(dnsStream,
                                flow => new EndpointsKey(flow.Flow.SrcIp.ToString(), flow.Flow.DstIp.ToString()),
                                flow => new EndpointsKey(flow.Flow.DstIp.ToString(), flow.ResponseData),
                                (left, right) => new { left.Flow, Key = new EndpointsKey(right.Flow.DstIp.ToString(), right.ResponseData), Value = right })
                            .GroupApply(
                                flow => new { flow.Flow, flow.Key },
                                group => group.Aggregate(aggregate => aggregate.CollectList(flow => flow.Value)),
                                (key, value) => new KeyValuePair<FlowKey, FlowGroup<EndpointsKey, DnsFlowRecord>>(key.Key.Flow, new FlowGroup<EndpointsKey, DnsFlowRecord>(key.Key.Key, value.Distinct().ToArray()))
                               );

                        var httpClientFlowsStream = stream
                            .Join(httpStream,
                                flow => new EndpointsKey(flow.Flow.SrcIp.ToString(), flow.Flow.DstIp.ToString()),
                                flow => new EndpointsKey(flow.Flow.SrcIp.ToString(), flow.Flow.DstIp.ToString()),
                                (left, right) => new { left.Flow, Key = new EndpointsKey(right.Flow.SrcIp.ToString(), right.Flow.DstIp.ToString()), Value = right })
                            .GroupApply(
                                flow => new { flow.Flow, flow.Key },
                                group => group.Aggregate(aggregate => aggregate.CollectList(flow => flow.Value)),
                                (key, value) => new KeyValuePair<FlowKey, FlowGroup<EndpointsKey, HttpFlowRecord>>(key.Key.Flow, new FlowGroup<EndpointsKey, HttpFlowRecord>(key.Key.Key, value.Distinct().ToArray()))
                               );

                        var tlsClientFlowsStream = stream.MatchGroupApply(
                        flow => new TlsClientKey(flow.Flow.SrcIp.ToString(), flow.TlsJa3),
                        flow => ValueTuple.Create(flow.Flow, new TlsClientKey(flow.Flow.SrcIp.ToString(), flow.TlsJa3)),
                        grouping => new KeyValuePair<FlowKey, FlowGroup<TlsClientKey, TlsFlowRecord>>(
                            grouping.Key.Item1,
                            new FlowGroup<TlsClientKey, TlsFlowRecord>(grouping.Key.Item2, grouping.OrderBy(t => System.Math.Abs(grouping.Key.Item1.SrcPt - t.Flow.SrcPt)).ToArray())
                        ));

                        var bagOfFlowsStream = stream.MatchGroupApply(
                        flow => new BagOfFlowsKey(flow.Flow.DstIp.ToString(), flow.Flow.DstPt, flow.Flow.Pt.ToString()),
                        flow => ValueTuple.Create(flow.Flow, new BagOfFlowsKey(flow.Flow.DstIp.ToString(), flow.Flow.DstPt, flow.Flow.Pt.ToString())),
                        grouping => new KeyValuePair<FlowKey, FlowGroup<BagOfFlowsKey, TlsFlowRecord>>(
                            grouping.Key.Item1,
                            new FlowGroup<BagOfFlowsKey, TlsFlowRecord>(grouping.Key.Item2, grouping.ToArray())
                        ));

                        var flowBurstStream = stream.MatchGroupApply(
                            flow => new FlowBurstKey(flow.Flow.SrcIp.ToString(), flow.Flow.DstIp.ToString(), flow.Flow.DstPt, flow.Flow.Pt.ToString()),
                            flow => ValueTuple.Create(flow.Flow, new FlowBurstKey(flow.Flow.SrcIp.ToString(), flow.Flow.DstIp.ToString(), flow.Flow.DstPt, flow.Flow.Pt.ToString())),
                            grouping => new KeyValuePair<FlowKey, FlowGroup<FlowBurstKey, TlsFlowRecord>>(grouping.Key.Item1,
                                new FlowGroup<FlowBurstKey, TlsFlowRecord>(grouping.Key.Item2, grouping.ToArray())
                            ));

                        return stream
                            .Select(f => new KeyValuePair<FlowKey, TlsFlowRecord>(f.Flow, f))
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

        protected override FlowWithContext GetTarget(StreamEvent<KeyValuePair<FlowKey, TlsContext>> arg)
        {
            return new FlowWithContext(arg.Payload.Key.ToString(), arg.Payload.Key, WindowSpan.FromLong(arg.StartTime, arg.EndTime), arg.Payload.Value);
        }
    }

    /// <summary>
    /// Implements context builder for TLS flows.
    /// </summary>
    static class TlsFlowContextBuilderCatalogueExtensions
    {
        /// <summary>
        /// Creates a TLS facts related to the given flow.
        /// </summary>
        /// <param name="source">Flow stream.</param>
        /// <param name="configuration">Configuration.</param>
        /// <returns>A stream of flows with their contexts.</returns>
        public static IStreamable<Empty, KeyValuePair<FlowKey, TlsContext>> BuildTlsContext(this ContextBuilderCatalog _, IStreamable<Empty, IpFlow> source, TimeSpan windowSize, TimeSpan windowHop)
        {
            var builder = new FlowContextBuilder(windowSize, windowHop);
            return builder.BuildContext(source);
        }
    }
}
