using Ethanol.Streaming;
using Microsoft.StreamProcessing;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive.Linq;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Ethanol.Demo
{
    record Flow(string Proto, string SrcIp, int SrcPt, string DstIp, int DstPt);

    record TlsInfo(Flow Flow, string TlsJa3, string TlsServerName, string TlsServerCommonName, string DomainName, double ServerNameEntropy, double DomainNameEntropy);


    record TlsContext(TlsInfo[] ClientFlows, TlsInfo[] ServiceFlows); 

    record ContextFlow<T>(Flow Flow, T Context);

    partial class Program
    {
        /// <summary>
        /// YAML serializer used to produce output.
        /// </summary>
        readonly ISerializer yamlSerializer = new SerializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).DisableAliases().Build();

        record BuildFlowContextConfiguration();
        /// <summary>
        /// Creates a TLS facts related to the given flow.
        /// </summary>
        /// <param name="flowStream">Flow stream.</param>
        /// <param name="configuration">Configuration.</param>
        /// <returns>A stream of flow context.</returns>
        IStreamable<Empty, ContextFlow<TlsContext>> BuildTlsContext(IStreamable<Empty, RawIpfixRecord> flowStream, BuildFlowContextConfiguration configuration)
        {
            var flowStreams = flowStream.Multicast(2);
            var tlsStream = flowStreams[0].Where(x => x.TlsClientVersion != "N/A" && x.SrcPort > x.DstPort);
            var dnsStream = flowStreams[1].Where(x => x.Protocol == "UDP" && x.SrcPort == 53);

            // enrich TLS with associated DNS queries
            var tlsDomainStream = tlsStream.LeftOuterJoin(dnsStream,
                flow => new { SrcIp = flow.SrcIp, DstIp = flow.DstIp },
                flow => new { SrcIp = flow.DstIp, DstIp = flow.DnsResponseData },
                left => new TlsInfo(new Flow(left.Protocol, left.SrcIp, left.SrcPort, left.DstIp, left.DstPort), left.TlsJa3, left.TlsServerName, left.TlsServerCommonName, string.Empty, ComputeDnsEntropy(left.TlsServerName).Max(), 0),
                (left, right) => new TlsInfo(new Flow(left.Protocol, left.SrcIp, left.SrcPort, left.DstIp, left.DstPort), left.TlsJa3, left.TlsServerName, left.TlsServerCommonName, right.DnsQueryName, ComputeDnsEntropy(left.TlsServerName).Max(), ComputeDnsEntropy(right.TlsServerName).Max()))
                .Distinct().Multicast(2);

            // collect all flows from the same source host with the same JA3:
            var ja3Clients = tlsDomainStream[0].GroupApply(
                flow => new { SrcIp = flow.Flow.SrcIp, Fingerprint = flow.TlsJa3 },
                group => group.Aggregate(aggregate => aggregate.Collect(flow => flow)),
                (key, value) => new { Key = key.Key, Value = value.Distinct().ToArray() });

            // collect a bag of flows:
            var bagOfFlows = tlsDomainStream[1].GroupApply(
                flow => new { DstIp = flow.Flow.DstIp, DstPt = flow.Flow.DstPt },
                group => group.Aggregate(aggregate => aggregate.Collect(flow => flow)),
                (key, value) => new { Key = key.Key, Value = value.Distinct().ToArray() });
        
            // expand groups to flows, e.g., flow with context
            var clientCtx = ja3Clients.SelectMany(x => x.Value.Select(f => new ContextFlow<TlsInfo[]>(f.Flow, x.Value)));
            var serviceCtx = bagOfFlows.SelectMany(x => x.Value.Select(f => new ContextFlow<TlsInfo[]>(f.Flow, x.Value)));
            
            // marge two context facts 
            var flowCtx = clientCtx.FullOuterJoin(serviceCtx,
                left => left.Flow,
                right => right.Flow,
                left => new ContextFlow<TlsContext>(left.Flow, new TlsContext(left.Context, Array.Empty<TlsInfo>())),
                right => new ContextFlow<TlsContext>(right.Flow, new TlsContext(Array.Empty<TlsInfo>(), right.Context)),
                (left, right) => new ContextFlow<TlsContext>(left.Flow, new TlsContext(left.Context, right.Context))
                );
            return flowCtx;
        }
    }
}
