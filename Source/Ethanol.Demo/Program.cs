using ConsoleAppFramework;
using Ethanol.Streaming;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.StreamProcessing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Ethanol.Demo
{
    partial class Program : ConsoleAppBase
    {
        static readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private readonly ILogger _logger;
        static IServiceProvider _services;
        public Program(ILogger<Program> logger)
        {
            _logger = logger;
        }

        static async Task Main(string[] args)
        {
            Console.CancelKeyPress += new ConsoleCancelEventHandler(CancelHandler);
            var verbose = args.Contains("-v");
            var hostBuilder = Host.CreateDefaultBuilder()
                .ConfigureLogging(b => ConfigureLogging(b, verbose ? LogLevel.Trace : LogLevel.Information))
                .UseConsoleAppFramework<Program>(args);
            var host = hostBuilder.Build();
            _services = host.Services;
            await host.RunAsync(_cancellationTokenSource.Token);
        }

        private static void ConfigureLogging(ILoggingBuilder loggingBuilder, LogLevel logLevel)
        {
            loggingBuilder.ClearProviders();
            loggingBuilder.AddSimpleConsole().SetMinimumLevel(logLevel); 
        }

        private static void CancelHandler(object sender, ConsoleCancelEventArgs e)
        {
            _cancellationTokenSource.Cancel();
        }

        [Command("monitor-tor", "Monitor flows and provide near real-time information on TOR communication in etwork traffic.")]
        public async Task MonitorTor(
            [Option("p", "path to data folder with source nfdump files.")]
            string dataPath
            )
        {
            var fsw = new FileSystemWatcher(dataPath, "*.*")
            {
                EnableRaisingEvents = true
            };
            var sourceFiles = Observable.FromEvent<FileSystemEventHandler, FileInfo>(handler =>
                {
                    void fsHandler(object sender, FileSystemEventArgs e)
                    {
                        handler(new FileInfo(e.FullPath));
                    }

                    return fsHandler;
                },
                fsHandler => fsw.Created += fsHandler,
                fsHandler => fsw.Created -= fsHandler);
            await DetectTorInFiles(sourceFiles, new TorDetectorConfiguration(3));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataPath"></param>
        /// <param name="csvPath"></param>
        /// <returns></returns>
        [Command("detect-tor", "Detect Tor in existing network traffic.")]
        public async Task DetectTor(
        [Option("p", "path to data folder with source nfdump files.")]
                string dataPath,
        [Option("e", "minimum entropy of server name")]
                double entropy = 3.0
        )
        {
            _logger.LogTrace($"Start processing source folder: {dataPath}");

            var sourceFiles = Directory.GetFiles(dataPath).Select(fileName => new FileInfo(fileName)).OrderBy(f => f.Name).ToObservable();
            await DetectTorInFiles(sourceFiles, new TorDetectorConfiguration(entropy));
        }

        record TorDetectorConfiguration(double DomainNameEntropy);

        async Task DetectTorInFiles(IObservable<FileInfo> sourceFiles, TorDetectorConfiguration configuration)
        {

            //
            // This tweak is to force Trill to output the results during the processing, see more:
            //
            // https://github.com/microsoft/Trill/issues/129
            //

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
            var streamOfTls = GetStreamOfFlows(artifactSourceTls.Where(f=>f.Payload?.SourcePort > f.Payload?.DestinationPort), windowSize, windowHop).Multicast(2);


            var tlsWithDomainStream = streamOfTls[0].LeftOuterJoin(
                streamOfDns,
                f => new { HOST = f.SrcIp, DA = f.DstIp },
                f => new { HOST = f.DstIp, DA = f.DnsResponseData },
                l => new { TlsFlow = l, ServerName = l.TlsServerName, CommonName = l.TlsSubjectCommonName, DomainName = string.Empty, Entropy = ComputeDnsEntropy(l.TlsServerName) },
                (l, r) => new { TlsFlow = l, ServerName = l.TlsServerName, CommonName = l.TlsSubjectCommonName, DomainName = r.DnsQuestionName, Entropy = ComputeDnsEntropy(l.TlsServerName) });

            var identifiedExitNodesStream = tlsWithDomainStream
                .Where(e => String.IsNullOrWhiteSpace(e.DomainName) && e.CommonName == "N/A" && e.Entropy > configuration.DomainNameEntropy && e.TlsFlow.DestinationPort > 443)
                .Select(f => new { DA = f.TlsFlow.DstIp, DP = f.TlsFlow.DstPt, ServerName = f.ServerName, JA3 = f.TlsFlow.Ja3Fingerprint });

            var allTorExitNodesStream = identifiedExitNodesStream.Join(streamOfTls[1],
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
                    (k, g1, g2, g3, g4, g6) => new TorClient(k.Key,(int)g1,g2, g3, g4.Distinct().OrderBy(x => x).ToArray(), g6.Distinct().OrderBy(x => x).ToArray()));

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
                return $"TorClient {{ Host = {Host}, Flows = {Flows}, Packets = {Packets}, Bytes = {Bytes}, ExitNodes = [{String.Join(',',ExitNodes)}], FlowRefs =  [{String.Join(',', FlowRefs)}] }}"; 
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

        record HostFlows(string Host, ulong Flows, int Packets, int  Bytes);

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
    }
}
