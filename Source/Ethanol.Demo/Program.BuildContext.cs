using Ethanol.Streaming;
using Microsoft.StreamProcessing;
using System;
using System.Linq;
using System.Reactive.Linq;

namespace Ethanol.Demo
{
    record Flow(string Proto, string SrcIp, int SrcPt, string DstIp, int DstPt);

    record TlsInfo(Flow Flow, string Ja3Fingerprint, string ServerName, string CommonName, string DomainName, double ServerNameEntropy, double DomainNameEntropy);


    record TlsContext(TlsInfo[] ClientFlows, TlsInfo[] ServiceFlows); 

    record ContextFlow<T>(Flow Flow, T Context);



    partial class Program
    {

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
            var tlsStream = flowStreams[0].Where(x => x.tlscver != "N/A" && Int32.Parse(x.sp) > Int32.Parse(x.dp));
            var dnsStream = flowStreams[1].Where(x => x.pr == "UDP" && x.sp == "53");

            // enrich TLS with associated DNS queries
            var tlsDomainStream = tlsStream.LeftOuterJoin(dnsStream,
                flow => new { HOST = flow.sa, DA = flow.da },
                flow => new { HOST = flow.da, DA = flow.dnsrdata },
                left => new TlsInfo(new Flow("TCP", left.sa, Int32.Parse(left.sp), left.da, Int32.Parse(left.dp)), left.tlsja3, left.tlssni, left.tlsscn, string.Empty, ComputeDnsEntropy(left.tlssni).Max(), 0),
                (left, right) => new TlsInfo(new Flow("TCP", left.sa, Int32.Parse(left.sp), left.da, Int32.Parse(left.dp)), left.tlsja3, left.tlssni, left.tlsscn, right.dnsqname, ComputeDnsEntropy(left.tlssni).Max(), ComputeDnsEntropy(right.tlssni).Max()))
                .Distinct().Multicast(2);

            // collect all flows from the same source host with the same JA3:
            var ja3ClientStream = tlsDomainStream[0].GroupApply(
                flow => new { SrcIp = flow.Flow.SrcIp, Fingerprint = flow.Ja3Fingerprint },
                group => group.Aggregate(aggregate => aggregate.Collect(flow => flow)),
                (key, value) => new { Key = key.Key, Value = value.Distinct().ToArray() });

            // collect a bag of flows:
            var bagOfFlows = tlsDomainStream[1].GroupApply(
                flow => new { DstIp = flow.Flow.DstIp, DstPt = flow.Flow.DstPt },
                group => group.Aggregate(aggregate => aggregate.Collect(flow => flow)),
                (key, value) => new { Key = key.Key, Value = value.Distinct().ToArray() });
        
            var clientCtx = ja3ClientStream.SelectMany(x => x.Value.Select(f => new ContextFlow<TlsInfo[]>(f.Flow, x.Value)));
            var serviceCtx = bagOfFlows.SelectMany(x => x.Value.Select(f => new ContextFlow<TlsInfo[]>(f.Flow, x.Value)));
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
