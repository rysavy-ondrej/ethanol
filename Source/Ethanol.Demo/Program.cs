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

        [Command("monitor-tor", "Monitor flows and provide near real-time information on TOR communication in network traffic.")]
        public async Task MonitorTorCommand(
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
            await DetectTor1(sourceFiles, new DetectTorConfiguration(3));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataPath"></param>
        /// <param name="csvPath"></param>
        /// <returns></returns>
        [Command("detect-tor", "Detect Tor in existing network traffic.")]
        public async Task DetectTorCommand(
        [Option("p", "path to data folder with source nfdump files.")]
                string dataPath,
        [Option("e", "minimum entropy of server name")]
                double entropy = 3.0
        )
        {
            _logger.LogTrace($"Start processing source folder: {dataPath}");
            var sourceFiles = Directory.GetFiles(dataPath).Select(fileName => new FileInfo(fileName)).OrderBy(f => f.Name).ToObservable();
            await DetectTor1(sourceFiles, new DetectTorConfiguration(entropy));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataPath"></param>
        /// <param name="csvPath"></param>
        /// <returns></returns>
        [Command("detect-tor2", "Detect Tor in existing network traffic.")]
        public async Task DetectTorCommand2(
        [Option("p", "path to data folder with source nfdump files.")]
                string dataPath,
        [Option("e", "minimum entropy of server name")]
                double entropy = 3.0
        )
        {
            _logger.LogTrace($"Start processing source folder: {dataPath}");
            var sourceFiles = Directory.GetFiles(dataPath).Select(fileName => new FileInfo(fileName)).OrderBy(f => f.Name).ToObservable();
            await DetectTor2(sourceFiles, new DetectTorConfiguration(entropy));
        }
    }
}
