using Elastic.Clients.Elasticsearch.Fluent;
using Elastic.Clients.Elasticsearch.IndexManagement;
using Ethanol.Catalogs;
using Ethanol.ContextBuilder.Context;
using Ethanol.ContextBuilder.Plugins.Attributes;
using Ethanol.Streaming;
using Microsoft.StreamProcessing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace Ethanol.ContextBuilder.Builders
{
    public record FlowMeters(int Packets, int Octets, DateTime TimeStart, TimeSpan Duration);
    public record EndpointsKey(string SrcIp, string DstIp);
    public record TlsFlowRecord(IpfixKey Flow, FlowMeters Meters, string TlsJa3, string TlsServerName, string TlsServerCommonName, string ProcessName);
    public record DnsFlowRecord(IpfixKey Flow, FlowMeters Meters, string QueryName, string ResponseData);
    public record HttpFlowRecord(IpfixKey Flow, FlowMeters Meters, string Method, string HostName, string Url, string ProcessName);
    public record HttpsFlowRecord(IpfixKey Flow, FlowMeters Meters, string Method, string HostName, string Url, string ProcessName);
    public record TlsContext(TlsFlowRecord TlsRecord,
        FlowGroup<EndpointsKey, DnsFlowRecord> Domains,
        FlowGroup<TlsClientKey, TlsFlowRecord> TlsClientFlows,
        FlowGroup<BagOfFlowsKey, TlsFlowRecord> BagOfFlows,
        FlowGroup<FlowBurstKey, TlsFlowRecord> FlowBurst,
        FlowGroup<EndpointsKey, HttpFlowRecord> PlainHttpFlows);
    public record TlsClientKey(string SrcIp, string Ja3Fingerprint);

    [Plugin(PluginType.Builder, "FlowContext", "Builds the context for TLS flows in the source IPFIX stream.")]
    public class TlsFlowContextBuilder : ContextBuilder<IpfixObject, InternalContextFlow<TlsContext>, ContextFlow<TlsContext>>
    {

        public TlsFlowContextBuilder(TimeSpan windowSize, TimeSpan windowHop) : base(new IpfixObservableStream(windowSize, windowHop))
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
        internal static IContextBuilder<IpfixObject, object> Create(Configuration configuration)
        {
            return new TlsFlowContextBuilder(configuration.Window, configuration.Hop);
        }

        public override IStreamable<Empty, InternalContextFlow<TlsContext>> BuildContext(IStreamable<Empty, IpfixObject> source)
        {
            return BuildTlsContext(source);
        }

        private IStreamable<Empty, InternalContextFlow<TlsContext>> BuildTlsContext(IStreamable<Empty, IpfixObject> source)
        {
            try
            {
                return source.Multicast(flowStream =>
                {
                    var tlsStream = flowStream.Where(x => !string.IsNullOrEmpty(x.TlsVersion) && x.SourceTransportPort > x.DestinationPort).Select(f => new TlsFlowRecord(f.FlowKey, f.GetMeters(), f.TlsJa3, f.TlsServerName, f.TlsServerCommonName, f.ProcessName));
                    var dnsStream = flowStream.Where(x => x.Protocol == System.Net.Sockets.ProtocolType.Udp && x.SourceTransportPort == 53).Select(f => new DnsFlowRecord(f.FlowKey, f.GetMeters(), f.DnsQueryName, f.DnsResponseData));
                    var httpStream = flowStream.Where(x => x.Protocol == System.Net.Sockets.ProtocolType.Tcp && !string.IsNullOrWhiteSpace(x.HttpUrl)).Select(f => new HttpFlowRecord(f.FlowKey, f.GetMeters(), f.HttpMethod, f.HttpHost, f.HttpUrl, f.ProcessName));

                    var flowContextStream = tlsStream.Multicast(stream =>
                    {
                        var dnsClientFlowsStream = stream
                            .Join(dnsStream,
                                flow => new EndpointsKey(flow.Flow.SrcIp, flow.Flow.DstIp),
                                flow => new EndpointsKey(flow.Flow.DstIp, flow.ResponseData),
                                (left, right) => new { left.Flow, Key = new EndpointsKey(right.Flow.DstIp, right.ResponseData), Value = right })
                            .GroupApply(
                                flow => new { flow.Flow, flow.Key },
                                group => group.Aggregate(aggregate => aggregate.CollectList(flow => flow.Value)),
                                (key, value) => new InternalContextFlow<FlowGroup<EndpointsKey, DnsFlowRecord>>(key.Key.Flow, new FlowGroup<EndpointsKey, DnsFlowRecord>(key.Key.Key, value.Distinct().ToArray()))
                               );

                        var httpClientFlowsStream = stream
                            .Join(httpStream,
                                flow => new EndpointsKey(flow.Flow.SrcIp, flow.Flow.DstIp),
                                flow => new EndpointsKey(flow.Flow.SrcIp, flow.Flow.DstIp),
                                (left, right) => new { left.Flow, Key = new EndpointsKey(right.Flow.SrcIp, right.Flow.DstIp), Value = right })
                            .GroupApply(
                                flow => new { flow.Flow, flow.Key },
                                group => group.Aggregate(aggregate => aggregate.CollectList(flow => flow.Value)),
                                (key, value) => new InternalContextFlow<FlowGroup<EndpointsKey, HttpFlowRecord>>(key.Key.Flow, new FlowGroup<EndpointsKey, HttpFlowRecord>(key.Key.Key, value.Distinct().ToArray()))
                               );

                        var tlsClientFlowsStream = stream.MatchGroupApply(
                        flow => new TlsClientKey(flow.Flow.SrcIp, flow.TlsJa3),
                        flow => ValueTuple.Create(flow.Flow, new TlsClientKey(flow.Flow.SrcIp, flow.TlsJa3)),
                        grouping => new InternalContextFlow<FlowGroup<TlsClientKey, TlsFlowRecord>>(
                            grouping.Key.Item1,
                            new FlowGroup<TlsClientKey, TlsFlowRecord>(grouping.Key.Item2, grouping.OrderBy(t => System.Math.Abs(grouping.Key.Item1.SrcPt - t.Flow.SrcPt)).ToArray())
                        ));

                        var bagOfFlowsStream = stream.MatchGroupApply(
                        flow => new BagOfFlowsKey(flow.Flow.DstIp, flow.Flow.DstPt, flow.Flow.Proto),
                        flow => ValueTuple.Create(flow.Flow, new BagOfFlowsKey(flow.Flow.DstIp, flow.Flow.DstPt, flow.Flow.Proto)),
                        grouping => new InternalContextFlow<FlowGroup<BagOfFlowsKey, TlsFlowRecord>>(
                            grouping.Key.Item1,
                            new FlowGroup<BagOfFlowsKey, TlsFlowRecord>(grouping.Key.Item2, grouping.ToArray())
                        ));

                        var flowBurstStream = stream.MatchGroupApply(
                            flow => new FlowBurstKey(flow.Flow.SrcIp, flow.Flow.DstIp, flow.Flow.DstPt, flow.Flow.Proto),
                            flow => ValueTuple.Create(flow.Flow, new FlowBurstKey(flow.Flow.SrcIp, flow.Flow.DstIp, flow.Flow.DstPt, flow.Flow.Proto)),
                            grouping => new InternalContextFlow<FlowGroup<FlowBurstKey, TlsFlowRecord>>(grouping.Key.Item1,
                                new FlowGroup<FlowBurstKey, TlsFlowRecord>(grouping.Key.Item2, grouping.ToArray())
                            ));

                        return stream
                            .Select(f => new InternalContextFlow<TlsFlowRecord>(f.Flow, f))
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

        protected override ContextFlow<TlsContext> GetTarget(StreamEvent<InternalContextFlow<TlsContext>> arg)
        {
            return new ContextFlow<TlsContext>(arg.Payload.FlowKey.ToString(), arg.Payload.FlowKey, WindowSpan.FromLong(arg.StartTime, arg.EndTime), arg.Payload.Context);
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
        public static IStreamable<Empty, InternalContextFlow<TlsContext>> BuildTlsContext(this ContextBuilderCatalog _, IStreamable<Empty, IpfixObject> source, TimeSpan windowSize, TimeSpan windowHop)
        {
            var builder = new TlsFlowContextBuilder(windowSize, windowHop);
            return builder.BuildContext(source);
        }
    }
}
