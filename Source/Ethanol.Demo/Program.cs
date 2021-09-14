using ConsoleAppFramework;
using Ethanol.Artifacts;
using Ethanol.Context;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.StreamProcessing;
using Microsoft.StreamProcessing.Aggregates;
using NRules;
using NRules.Fluent;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Reactive.Linq;
using System.Reflection;
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
            await DetectTorInFiles(sourceFiles);
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
                string dataPath)
        {
            _logger.LogTrace($"Start processing source folder: {dataPath}");

            var sourceFiles = Directory.GetFiles(dataPath).Select(fileName => new FileInfo(fileName)).OrderBy(f => f.Name).ToObservable();
            await DetectTorInFiles(sourceFiles);
        }

        async Task DetectTorInFiles(IObservable<FileInfo> sourceFiles)
        { 
            var cancellationToken = _cancellationTokenSource.Token;

            var artifactSourceLong = new ArtifactSourceObservable<ArtifactLong>();
            var artifactSourceDns = new ArtifactSourceObservable<ArtifactDns>();
            var artifactSourceTls = new ArtifactSourceObservable<ArtifactTls>();

            
            var convertor = new NetFlowCsvConvertor(_services.GetRequiredService< ILogger<NetFlowCsvConvertor>>(),
                new ArtifactDataSource<ArtifactLong>("flow", "proto tcp", artifactSourceLong),
                new ArtifactDataSource<ArtifactDns>("dns", "proto udp and port 53", artifactSourceDns),
                new ArtifactDataSource<ArtifactTls>("tls", "not tls-cver \"N/A\"", artifactSourceTls));

            var windowSize = TimeSpan.FromMinutes(5);
            var windowHop = TimeSpan.FromMinutes(1);

            var streamOfFlow = GetStreamOfFlows(artifactSourceLong, windowSize, windowHop); 
            var streamOfDns = GetStreamOfFlows(artifactSourceDns, windowSize, windowHop); 
            var streamOfTls = GetStreamOfFlows(artifactSourceTls,windowSize, windowHop);
            
            var outputObservable = streamOfFlow.Count().ToStreamEventObservable().Where(e=> e.IsEnd || e.IsInterval); 

            void FetchRecords(CsvSourceFile obj)
            {
                _logger.LogTrace($"Loading flows from {obj.Filename} to {obj.Source.ArtifactName} observable.");
                obj.Source.LoadFromAsync(obj.Stream, cancellationToken).ContinueWith(x => _logger.LogTrace($"Loaded {x.Result} flows from {obj.Filename}."));     // fire & forget?
            }
            void CloseStreams(Task t)
            {
                convertor.Close();
            }

            try
            {
                _logger.LogTrace("Setting up the processing pipeline.");
                var outputTask = outputObservable.ForEachAsync(PrintRecords, cancellationToken);
                
                _logger.LogTrace("Start fetching data from dump files.");
                await sourceFiles.SelectMany(convertor.Generate).ForEachAsync(FetchRecords, cancellationToken).ContinueWith(CloseStreams);
                _logger.LogTrace("All files fetched, waiting for processing pipeline to complete.");
                Task.WaitAll(new[] { outputTask }, cancellationToken);
            }
            catch (TaskCanceledException)
            {
                _logger.LogInformation("Termination requested by the user.");
            }
            _logger.LogTrace("Program finished.");
        }



        private IStreamable<Empty, T> GetStreamOfFlows<T>(ArtifactSourceObservable<T> source, TimeSpan windowSize, TimeSpan windowHop) where T : IpfixArtifact
        {
            return source.ToTemporalStreamable(x => x.StartTime, x => x.EndTime, disorderPolicy: DisorderPolicy.Adjust(TimeSpan.FromMinutes(1).Ticks), periodicPunctuationPolicy: PeriodicPunctuationPolicy.Time((ulong)TimeSpan.FromSeconds(30).Ticks)).HoppingWindowLifetime(windowSize.Ticks, windowHop.Ticks);
        }

        private void PrintRecords(StreamEvent<ulong> evt)
        {
            _logger.LogInformation($"New result received: [{new DateTime(evt.StartTime)},{new DateTime(evt.EndTime)}): Flow count={evt.Payload}");
        }

        record HostFlows(string Host, ulong Flows, int Packets, int  Bytes);

        public double ComputeEntropy(string message)
        {
            Dictionary<char, int> K = message.GroupBy(c => c).ToDictionary(g => g.Key, g => g.Count());
            double entropyValue = 0;
            foreach (var character in K)
            {
                double PR = character.Value / (double)message.Length;
                entropyValue -= PR * Math.Log(PR, 2);
            }
            return entropyValue;
        }
    }
}
