using Ethanol.Streaming;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.StreamProcessing;
using System;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using YamlDotNet.Core.Tokens;

namespace Ethanol.Demo
{

    public interface IYamlOutput
    {
        string ToYaml(string space);
    }
    partial class Program
    {


        record BuildFlowContextConfiguration();
        record TlsHandshake(string SrcIp, int SrcPt, string DstIp, int DstPt, string Ja3Fingerprint, string ServerName, string CommonName, string DomainName, double ServerNameEntropy, double DomainNameEntropy) : IYamlOutput
        {
            public string ToYaml(string space)
            {
                return $"{space}Flow: {SrcIp}:{SrcPt}-{DstIp}:{DstPt}\n" +
                       $"{space}Ja3Fingerprint: {Ja3Fingerprint}\n" +
                       $"{space}ServerName: {ServerName}\n" +
                       $"{space}CommonName: {CommonName}\n" +
                       $"{space}DomainName: {DomainName}\n" +
                       $"{space}ServerNameEntropy: {ServerNameEntropy}\n" +
                       $"{space}DomainNameEntropy: {DomainNameEntropy}\n";
            }
        }


        record FlowAndContext<T>(string SrcIp, int SrcPt, string DstIp, int DstPt, T[] Context) where T : IYamlOutput
        {
            internal string ToYaml(string space)
            {
                var contextString = String.Join($"\n", Context.Select(c => c.ToYaml($"{space}  ")));
                return $"{space}Flow: {SrcIp}:{SrcPt}-{DstIp}:{DstPt}\n" +
                       $"{space}Context:\n" + 
                       $"{contextString}\n";
            }
        }

        IStreamable<Empty, FlowAndContext<TlsHandshake>> BuildFlowContext(IStreamable<Empty, RawIpfixRecord> flowStream, BuildFlowContextConfiguration configuration)
        {
            var flowStreams = flowStream.Multicast(2);

            var tlsStream = flowStreams[0].Where(x => x.tlscver != "N/A" && Int32.Parse(x.sp) > Int32.Parse(x.dp));
            var dnsStream = flowStreams[1].Where(x => x.pr == "UDP" && x.sp == "53");


            var tlsDomainStream = tlsStream.LeftOuterJoin(dnsStream,
                flow => new { HOST = flow.sa, DA = flow.da },
                flow => new { HOST = flow.da, DA = flow.dnsrdata },
                left => new TlsHandshake(left.sa, Int32.Parse(left.sp), left.da, Int32.Parse(left.dp), left.tlsja3, left.tlssni, left.tlsscn, string.Empty, ComputeDnsEntropy(left.tlssni), 0),
                (left, right) => new TlsHandshake(left.sa, Int32.Parse(left.sp), left.da, Int32.Parse(left.dp), left.tlsja3, left.tlssni, left.tlsscn, right.dnsqname, ComputeDnsEntropy(left.tlssni), ComputeDnsEntropy(right.tlssni))).Distinct();

            // collect all flows with the same JA3:
            var ja3ClientStream = tlsDomainStream.GroupApply(
                flow => new { SrcIp = flow.SrcIp, Fingerprint = flow.Ja3Fingerprint },
                group => group.Aggregate(aggregate => aggregate.Collect(flow => flow)),
                (key, value) => new { Key = key.Key, Value = value.Distinct().ToArray() });

            var flowAndContext = ja3ClientStream.SelectMany(x => x.Value.Select(f => new FlowAndContext<TlsHandshake>(f.SrcIp, f.SrcPt, f.DstIp, f.DstPt, x.Value)));
            return flowAndContext;
        }
    }
}
