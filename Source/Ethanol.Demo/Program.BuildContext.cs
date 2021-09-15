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

        record FlowAndContext<T>(string SrcIp, string SrcPt, string DstIp, string DstPt, T[] Context);
        async Task BuildFlowContext(IObservable<FileInfo> sourceFiles, BuildFlowContextConfiguration configuration)
        {

            Microsoft.StreamProcessing.Config.ForceRowBasedExecution = true;
            Microsoft.StreamProcessing.Config.DataBatchSize = 100;

            var cancellationToken = _cancellationTokenSource.Token;

            var artifactSourceLong = new ArtifactSourceObservable<ArtifactLong>();
            var artifactSourceDns = new ArtifactSourceObservable<ArtifactDns>();
            var artifactSourceTls = new ArtifactSourceObservable<ArtifactTls>();

            var convertor = new NetFlowCsvConvertor(_services.GetRequiredService<ILogger<NetFlowCsvConvertor>>(),
                new ArtifactDataSource<ArtifactLong>("flow", "proto tcp", artifactSourceLong),
                new ArtifactDataSource<ArtifactDns>("dns", "proto udp and port 53", artifactSourceDns),
                new ArtifactDataSource<ArtifactTls>("tls", "not tls-cver \"N/A\"", artifactSourceTls));


            var windowSize = TimeSpan.FromMinutes(15);
            var windowHop = TimeSpan.FromMinutes(5);

            var streamOfFlow = GetStreamOfFlows(artifactSourceLong, windowSize, windowHop);
            var streamOfDns = GetStreamOfFlows(artifactSourceDns, windowSize, windowHop);
            var streamOfTls = GetStreamOfFlows(artifactSourceTls.Where(f => f.Payload?.SourcePort > f.Payload?.DestinationPort), windowSize, windowHop).Multicast(2);

            var tlsHandshakeStream = streamOfTls[0].Where(f => f.SourcePort > f.DestinationPort).LeftOuterJoin(streamOfDns,
                f => new { HOST = f.SrcIp, DA = f.DstIp },
                f => new { HOST = f.DstIp, DA = f.DnsResponseData },
                l => new TlsHandshake(l.SrcIp, l.SrcPt, l.DstIp, l.DstPt, l.Ja3Fingerprint, l.TlsServerName, l.TlsSubjectCommonName, string.Empty, ComputeDnsEntropy(l.TlsServerName), 0),
                (l, r) => new TlsHandshake(l.SrcIp, l.SrcPt, l.DstIp, l.DstPt, l.Ja3Fingerprint, l.TlsServerName, l.TlsSubjectCommonName, r.DnsQuestionName, ComputeDnsEntropy(l.TlsServerName), ComputeDnsEntropy(r.DnsQuestionName))).Distinct();

            // collect all flows with the same JA3:
            var ja3groups = tlsHandshakeStream.GroupApply(f => new { SrcIp = f.SrcIp, Fingerprint = f.Ja3Fingerprint },
                g => g.Aggregate(a => a.Collect(f => f)),
                (k, v) => new { Key = k.Key, Value = v.Distinct().ToArray() });

            // conpute the context for each flow, i.e., (flow, ja3flows, 
            var flowAndContext = ja3groups.SelectMany(x => x.Value.Select(f => new FlowAndContext<TlsHandshake>(f.SrcIp, f.SrcPt, f.DstIp, f.DstPt, x.Value)));

            var torStreams = flowAndContext.Where(f => f.Context.Any((e => String.IsNullOrWhiteSpace(e.DomainName) && e.CommonName == "N/A" && e.ServerNameEntropy > 3)));
        }
    }
}
