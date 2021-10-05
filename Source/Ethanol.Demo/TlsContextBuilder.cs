using Ethanol.Streaming;
using Microsoft.StreamProcessing;
using System.Linq;
using System.Reactive.Linq;

namespace Ethanol.Demo
{
    public record TlsInfo(Flow Flow, string TlsJa3, string TlsServerName, string TlsServerCommonName, string DomainName, double ServerNameEntropy, double DomainNameEntropy);
    public record TlsContext(TlsInfo TlsHandshake, TlsInfo[] TlsClientFlows, TlsInfo[] BagOfFlows);

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

            //
            // TODO:
            //
            // REMOVE DUPLICITIES!
            // 
            // Duplicities occur because of join and group operations!


            var flowStreams = source.Multicast(2);
            var tlsStream = flowStreams[0].Where(x => x.TlsClientVersion != "N/A" && x.SrcPort > x.DstPort);
            var dnsStream = flowStreams[1].Where(x => x.Protocol == "UDP" && x.SrcPort == 53);

            // enrich TLS with associated DNS queries
            var tlsDomainStream = tlsStream.LeftOuterJoin(dnsStream,
                flow => new { SrcIp = flow.SrcIp, DstIp = flow.DstIp },
                flow => new { SrcIp = flow.DstIp, DstIp = flow.DnsResponseData },
                left => new TlsInfo(left.GetFlow(), left.TlsJa3, left.TlsServerName, left.TlsServerCommonName, string.Empty, Statistics.ComputeDnsEntropy(left.TlsServerName).Max(), 0),
                (left, right) => new TlsInfo(left.GetFlow(), left.TlsJa3, left.TlsServerName, left.TlsServerCommonName, right.DnsQueryName, Statistics.ComputeDnsEntropy(left.TlsServerName).Max(), Statistics.ComputeDnsEntropy(right.TlsServerName).Max()))
                .Distinct().Multicast(3);

            // collect all flows from the same source host with the same JA3:
            var tlsClientFlowsStream = tlsDomainStream[0]
                .GroupApply(
                    flow => new { SrcIp = flow.Flow.SrcIp, Fingerprint = flow.TlsJa3 },
                    group => group.Aggregate(aggregate => aggregate.Collect(flow => flow)),
                    (key, value) => new { Key = key.Key, Value = value.Distinct() })
                .SelectMany(x => x.Value.Select(f => new ContextFlow<TlsInfo[]>(f.Flow, x.Value.ToArray())));

            // collect a bag of flows:
            var bagOfFlowsStream = tlsDomainStream[1]
                .GroupApply(
                    flow => new { DstIp = flow.Flow.DstIp, DstPt = flow.Flow.DstPt },
                    group => group.Aggregate(aggregate => aggregate.Collect(flow => flow)),
                    (key, value) => new { Key = key.Key, Value = value.Distinct() })
                .SelectMany(x => x.Value.Select(f => new ContextFlow<TlsInfo[]>(f.Flow, x.Value.ToArray())));
            return ContextAggregator.MergeContextFlowStreams((x,y,z) => new TlsContext(x,y,z), tlsDomainStream[2].Select(f=> new ContextFlow<TlsInfo>(f.Flow, f)), tlsClientFlowsStream, bagOfFlowsStream);
        }
    }
}
