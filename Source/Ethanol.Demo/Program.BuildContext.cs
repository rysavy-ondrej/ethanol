using Ethanol.Streaming;
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
    partial class Program
    {
        record BuildFlowContextConfiguration();
        record TlsHandshake(string SrcIp, string SrcPt, string DstIp, string DstPt, string Ja3Fingerprint, string ServerName, string CommonName, string DomainName, double ServerNameEntropy, double DomainNameEntropy);
        record FlowAndContext<T>(string SrcIp, string SrcPt, string DstIp, string DstPt, T[] Context)
        {
            internal string ToYaml(string space)
            {
                var contextString = String.Join($",\n{space}  ", Context.Select(c => c.ToString()));
                return $"{space}Flow: {SrcIp}:{SrcPt}-{DstIp}:{DstPt}\n" +
                       $"{space}Context: [{contextString}]\n";
            }
        }

        IStreamable<Empty, FlowAndContext<TlsHandshake>> BuildFlowContext(IStreamable<Empty, ArtifactLong> flowStream, IStreamable<Empty, ArtifactDns> dnsStream, IStreamable<Empty, ArtifactTls> tlsStream, BuildFlowContextConfiguration configuration)
        {
            var tlsDomainStream = tlsStream.LeftOuterJoin(dnsStream,
                flow => new { HOST = flow.SrcIp, DA = flow.DstIp },
                flow => new { HOST = flow.DstIp, DA = flow.DnsResponseData },
                left => new TlsHandshake(left.SrcIp, left.SrcPt, left.DstIp, left.DstPt, left.Ja3Fingerprint, left.TlsServerName, left.TlsSubjectCommonName, string.Empty, ComputeDnsEntropy(left.TlsServerName), 0),
                (left, right) => new TlsHandshake(left.SrcIp, left.SrcPt, left.DstIp, left.DstPt, left.Ja3Fingerprint, left.TlsServerName, left.TlsSubjectCommonName, right.DnsQuestionName, ComputeDnsEntropy(left.TlsServerName), ComputeDnsEntropy(right.DnsQuestionName))).Distinct();

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
