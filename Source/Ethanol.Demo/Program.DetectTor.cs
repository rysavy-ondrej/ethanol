using Ethanol.Providers;
using Ethanol.Streaming;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.StreamProcessing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;

namespace Ethanol.Demo
{
    partial class Program
    {
        record DetectTorConfiguration(double DomainNameEntropy);
        async Task DetectTor1(IObservable<FileInfo> sourceFiles, DetectTorConfiguration configuration)
        {
            //
            // This tweak is to force Trill to output the results during the processing, see more:
            //
            // https://github.com/microsoft/Trill/issues/129
            //

            Microsoft.StreamProcessing.Config.ForceRowBasedExecution = true;
            Microsoft.StreamProcessing.Config.DataBatchSize = 100;

            var cancellationToken = _cancellationTokenSource.Token;
            var convertor = InitializeStreams(new ArtifactSourceObservable<ArtifactLong>(), new ArtifactSourceObservable<ArtifactDns>(), new ArtifactSourceObservable<ArtifactTls>(), out var streamOfFlow, out var streamOfDns, out var tlsStream);
            var streamOfTls = tlsStream.Multicast(2);

            var tlsWithDomainStream = streamOfTls[0]
                .LeftOuterJoin(streamOfDns,
                    f => new { HOST = f.SrcIp, DA = f.DstIp },
                    f => new { HOST = f.DstIp, DA = f.DnsResponseData },
                    l => new { TlsFlow = l, ServerName = l.TlsServerName, CommonName = l.TlsSubjectCommonName, DomainName = string.Empty, Entropy = ComputeDnsEntropy(l.TlsServerName) },
                    (l, r) => new { TlsFlow = l, ServerName = l.TlsServerName, CommonName = l.TlsSubjectCommonName, DomainName = r.DnsQuestionName, Entropy = ComputeDnsEntropy(l.TlsServerName) });



            var identifiedExitNodesStream = tlsWithDomainStream
                .Where(e => String.IsNullOrWhiteSpace(e.DomainName) && e.CommonName == "N/A" && e.Entropy > configuration.DomainNameEntropy && e.TlsFlow.DestinationPort > 443)
                .Select(f => new { DA = f.TlsFlow.DstIp, DP = f.TlsFlow.DstPt, ServerName = f.ServerName, JA3 = f.TlsFlow.Ja3Fingerprint });

            var allTorExitNodesStream = identifiedExitNodesStream
                .Join(streamOfTls[1],
                    l => l.JA3,
                    r => r.Ja3Fingerprint,
                    (l, r) => new { DA = r.DstIp, DP = r.DstPt, Sni = r.TlsServerName, JA3 = l.JA3 }
            ).Distinct();

            var torClientsStream = streamOfFlow
                .Join(allTorExitNodesStream,
                    l => new { DA = l.DstIp, DP = l.DstPt },
                    r => new { DA = r.DA, DP = r.DP },
                    (l, r) => new { Flow = l, Tor = new Tor(l.SrcIp, l.DstIp, r.Sni) })
                .GroupAggregate(f => f.Tor.Client,
                    g => g.Count(),
                    g => g.Sum(f => int.Parse(f.Flow.Packets)),
                    g => g.Sum(f => int.Parse(f.Flow.Bytes)),
                    g => g.Collect(f => $"{f.Tor.RelayAddress}({f.Tor.RelaySni})"),
                    g => g.Collect(f => f.Flow.Key),
                    (k, g1, g2, g3, g4, g6) => new TorClient(k.Key, (int)g1, g2, g3, g4.Distinct().OrderBy(x => x).ToArray(), g6.Distinct().OrderBy(x => x).ToArray()));

            var torClientsObservable = torClientsStream.ToStreamEventObservable(ReshapingPolicy.None);

            async Task FetchRecords(CsvSourceFile obj)
            {
                _logger.LogTrace($"Loading flows from {obj.Filename} to {obj.Source.ArtifactName} observable.");
                await obj.Source.LoadFromAsync(obj.Stream, cancellationToken).ContinueWith(x => _logger.LogTrace($"Loaded {x.Result} flows from {obj.Filename}."));
            }

            try
            {
                _logger.LogTrace("Setting up the processing pipeline.");
                var outputTask = torClientsObservable.ForEachAsync(PrintRecords, cancellationToken);

                _logger.LogTrace("Start fetching data from dump files.");
                await sourceFiles
                    .Do(f => Console.WriteLine($"source: {f.Name}"))
                    .SelectMany(convertor.Generate)
                    .ForEachAsync(async f => await FetchRecords(f), cancellationToken)
                    .ContinueWith((t) => { convertor.Close(); });

                _logger.LogTrace("All files fetched, waiting for processing pipeline to complete.");
                Task.WaitAll(new[] { outputTask }, cancellationToken);
            }
            catch (TaskCanceledException)
            {
                _logger.LogInformation("Termination requested by the user.");
            }
            _logger.LogTrace("Program finished.");
        }

        /// <summary>
        /// Creates and bind the input streams.
        /// </summary>
        /// <param name="artifactSourceLong"></param>
        /// <param name="artifactSourceDns"></param>
        /// <param name="artifactSourceTls"></param>
        /// <param name="streamOfFlow"></param>
        /// <param name="streamOfDns"></param>
        /// <param name="streamOfTls"></param>
        /// <returns></returns>
        private static NetFlowCsvConvertor InitializeStreams(ArtifactSourceObservable<ArtifactLong> artifactSourceLong, ArtifactSourceObservable<ArtifactDns> artifactSourceDns, ArtifactSourceObservable<ArtifactTls> artifactSourceTls, out IStreamable<Empty, ArtifactLong> streamOfFlow, out IStreamable<Empty, ArtifactDns> streamOfDns, out IStreamable<Empty, ArtifactTls> streamOfTls)
        {
            var convertor = new NetFlowCsvConvertor(_services.GetRequiredService<ILogger<NetFlowCsvConvertor>>(),
                new ArtifactDataSource<ArtifactLong>("flow", "proto tcp", artifactSourceLong),
                new ArtifactDataSource<ArtifactDns>("dns", "proto udp and port 53", artifactSourceDns),
                new ArtifactDataSource<ArtifactTls>("tls", "not tls-cver \"N/A\"", artifactSourceTls));
            var windowSize = TimeSpan.FromMinutes(15);
            var windowHop = TimeSpan.FromMinutes(5);

            streamOfFlow = GetStreamOfFlows(artifactSourceLong, windowSize, windowHop);
            streamOfDns = GetStreamOfFlows(artifactSourceDns, windowSize, windowHop);
            streamOfTls = GetStreamOfFlows(artifactSourceTls.Where(f => f.Payload?.SourcePort > f.Payload?.DestinationPort), windowSize, windowHop);
            return convertor;
        }

        private void PrintRecords(StreamEvent<TorClient> obj)
        {
            if (obj.IsEnd)
            {
                Console.WriteLine($"- event: tor-client-detected");
                Console.WriteLine($"  kind: {obj.Kind}");
                Console.WriteLine($"  time: {new DateTime(obj.StartTime)}");
                Console.WriteLine(obj.Payload.ToYaml("  "));
            }
        }

        record Tor(string Client, string RelayAddress, string RelaySni);
        record TorClient(string Host, int Flows, int Packets, int Bytes, string[] ExitNodes, string[] FlowRefs)
        {
            public override string ToString()
            {
                return $"TorClient {{ Host = {Host}, Flows = {Flows}, Packets = {Packets}, Bytes = {Bytes}, ExitNodes = [{String.Join(',', ExitNodes)}], FlowRefs =  [{String.Join(',', FlowRefs)}] }}";
            }
            public string ToYaml(string indent)
            {
                return
                      $"{indent}host: {Host}\n" +
                      $"{indent}flows: {Flows}\n" +
                      $"{indent}packets: {Packets}\n" +
                      $"{indent}bytes: {Bytes}\n" +
                      $"{indent}exit-nodes: [{String.Join(',', ExitNodes)}]\n" +
                      $"{indent}flow-refs: [{String.Join(',', FlowRefs)}]\n";
            }
        }

        private static IStreamable<Empty, T> GetStreamOfFlows<T>(IObservable<StreamEvent<T>> source, TimeSpan windowSize, TimeSpan windowHop) where T : IpfixArtifact
        {
            var stream = source.ToStreamable(disorderPolicy: DisorderPolicy.Adjust(TimeSpan.FromMinutes(5).Ticks), FlushPolicy.FlushOnBatchBoundary).HoppingWindowLifetime(windowSize.Ticks, windowHop.Ticks);
            return stream;
        }

        record HostFlows(string Host, ulong Flows, int Packets, int Bytes);

        public double ComputeEntropy(string message)
        {
            if (message == null) return 0;
            Dictionary<char, int> K = message.GroupBy(c => c).ToDictionary(g => g.Key, g => g.Count());
            double entropyValue = 0;
            foreach (var character in K)
            {
                double PR = character.Value / (double)message.Length;
                entropyValue -= PR * Math.Log(PR, 2);
            }
            return entropyValue;
        }
        public double ComputeDnsEntropy(string domain)
        {
            if (string.IsNullOrWhiteSpace(domain)) return 0;
            var parts = domain.Split('.');
            return parts.Select(ComputeEntropy).Max();
        }

        async Task DetectTor2(IObservable<FileInfo> sourceFiles, DetectTorConfiguration configuration)
        {
            var cancellationToken = _cancellationTokenSource.Token;
            Config.ForceRowBasedExecution = true;

            var windowSize = TimeSpan.FromMinutes(15);
            var windowHop = TimeSpan.FromMinutes(5);

            var convertor = new NetFlowCsvConvertor(_services.GetRequiredService<ILogger<NetFlowCsvConvertor>>());
            var loader = new ArtifactsLoader<RawIpfixRecord>();            
            var flowObservable = new Subject<RawIpfixRecord>();           
            loader.OnReadRecord += (object _, RawIpfixRecord value) => { flowObservable.OnNext(value); };



            var flowStream = GetEventStreamFromObservable(flowObservable, x=> DateTime.Parse(x.ts).Ticks, windowSize, windowHop);
            var contextStream = BuildFlowContext(flowStream, new BuildFlowContextConfiguration());
            var torFlowsStream = contextStream.Where(f => f.Context.Any(e => String.IsNullOrWhiteSpace(e.DomainName) && e.CommonName == "N/A" && e.ServerNameEntropy > configuration.DomainNameEntropy && e.DstPt >  443));


            // TODO: simplify loading from dump/csv file!!!
            // 1. enable to store dump file(s) to csv
            // 2. enable to load directly from csv

            void LoadRecordsFromFile(FileInfo fileInfo)
            {
                _logger.LogTrace($"Loading flows from {fileInfo.Name}...");
                using var stream = convertor.GetCsvStreamForDumpFile(fileInfo, "ipv4");
                loader.LoadFromCsvAsync(stream).Wait();
                Console.WriteLine($"Inserted flows: {loader.FlowCount}");
            }
            void PrintTorFlowRecords(StreamEvent<FlowAndContext<TlsHandshake>> obj)
            {
                if (obj.IsInterval || obj.IsEnd)
                {
                    Console.WriteLine($"- event: tor-flow-detected");
                    Console.WriteLine($"  kind: {obj.Kind}");
                    Console.WriteLine($"  time: {new DateTime(obj.StartTime)}");
                    Console.WriteLine(obj.Payload.ToYaml("  "));
                }
            }
            var processingTask = torFlowsStream.ToStreamEventObservable().ForEachAsync(PrintTorFlowRecords, cancellationToken);
            
            await sourceFiles
                .Do(f => Console.WriteLine($"source: {f.Name}"))
                .ForEachAsync(f => LoadRecordsFromFile(f), cancellationToken)
                .ContinueWith(_ => flowObservable.OnCompleted());
            Task.WaitAll(new[] { processingTask }, cancellationToken);
        }

        /// <summary>
        /// Gets the stream from the given observable. It uses <paramref name="getStartTime"/> to retrieve 
        /// start time timestamp of observable records to produce stream events. It also performs 
        /// hopping window time adjustement on all ingested events.
        /// </summary>
        /// <typeparam name="T">The type of event payloads.</typeparam>
        /// <param name="observable">The input observable.</param>
        /// <param name="getStartTime">The function to get start time of a record.</param>
        /// <param name="windowSize">Size of the hopping window.</param>
        /// <param name="windowHop">Hop size in ticks.</param>
        /// <returns>A stream of events with defined start times adjusted to hopping windows.</returns>
        private IStreamable<Empty, T> GetEventStreamFromObservable<T>(IObservable<T> observable, Func<T,long> getStartTime, TimeSpan windowSize, TimeSpan windowHop)
        {
            bool ValidateTimestamp((T Record,long StartTime) record) =>
                record.StartTime > DateTime.MinValue.Ticks && record.StartTime < DateTime.MaxValue.Ticks;            

            var source = observable
                .Select(x=> (Record:x,StartTime:getStartTime(x)))
                .Where(ValidateTimestamp)
                .Select(x => StreamEvent.CreatePoint(x.StartTime, x.Record));
            return source.ToStreamable(disorderPolicy: DisorderPolicy.Adjust(TimeSpan.FromMinutes(5).Ticks), FlushPolicy.FlushOnBatchBoundary).HoppingWindowLifetime(windowSize.Ticks, windowHop.Ticks);
        }
    }
}
