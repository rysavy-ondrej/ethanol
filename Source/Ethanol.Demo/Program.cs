using ConsoleAppFramework;
using Ethanol.Artifacts;
using Ethanol.Context;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NRules;
using NRules.Fluent;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Ethanol.Demo
{
    partial class Program : ConsoleAppBase
    {
        static readonly CancellationTokenSource _cts = new CancellationTokenSource();
        readonly ArtifactServiceCollection _artifactServiceCollection;

        public Program()
        {
            _artifactServiceCollection = new ArtifactServiceCollection(Assembly.GetExecutingAssembly());
        }

        static async Task Main(string[] args)
        {
            Console.CancelKeyPress += new ConsoleCancelEventHandler(cancelHandler);
            await Host.CreateDefaultBuilder().ConfigureServices(ConfigureServices).RunConsoleAppFrameworkAsync<Program>(args);
        }

        private static void ConfigureServices(HostBuilderContext context, IServiceCollection collection)
        {
        }

        private static void cancelHandler(object sender, ConsoleCancelEventArgs e)
        {
            Console.WriteLine("Cancel key pressed. Exiting...");
            _cts.Cancel();
            System.Environment.Exit(0);
        }

        [Command("monitor-tor", "Monitor flows and provide near real-time information on TOR communication in etwork traffic.")]
        public async Task MonitorTor(
            [Option("p", "path to data folder with source nfdump files.")]
            string dataPath,
            [Option("c", "path to a folder where csv files will be created.")]
            string csvPath)
        {
            var cancellRequested = _cts.Token;
            Console.WriteLine($"Monitor source folder: {dataPath}");
            var fileProvider = new NetflowDumpMonitor(dataPath, csvPath,
                new SourceRecipe<ArtifactLong>("flow", "proto tcp", new ArtifactSource<ArtifactLong>()),
                new SourceRecipe<ArtifactDns>("dns", "proto udp and port 53", new ArtifactSource<ArtifactDns>()), 
                new SourceRecipe<ArtifactTls>("tls", "not tls-cver \"N/A\"", new ArtifactSource<ArtifactTls>()));

            try
            {
                // how and where to process objects in observables above?


                await fileProvider.ForEachAsync(x => x.Source.LoadFrom(x.Filename), cancellRequested);
            }
            catch(TaskCanceledException)
            {
                Console.WriteLine("Terminating the process.");
            }
            // all is event-driven, so we can suspend this threat.
            cancellRequested.WaitHandle.WaitOne();
            Console.WriteLine("Finished.");
        }

        [Command("detect-tor", "Detect Tor in existing network traffic.")]
        public async Task DetectTor(
        [Option("p", "path to data folder with source nfdump files.")]
                string dataPath,
        [Option("c", "path to a folder where csv files will be created.")]
            string csvPath)
        {
            var cancellRequested = _cts.Token;
            Console.WriteLine($"Monitor source folder: {dataPath}");
            var fileProvider = new NetflowDumpProcessor(dataPath, csvPath,
                new SourceRecipe<ArtifactLong>("flow", "proto tcp", new ArtifactSource<ArtifactLong>()),
                new SourceRecipe<ArtifactDns>("dns", "proto udp and port 53", new ArtifactSource<ArtifactDns>()),
                new SourceRecipe<ArtifactTls>("tls", "not tls-cver \"N/A\"", new ArtifactSource<ArtifactTls>()));

            try
            {
                // how and where to process objects in observables above?
                await fileProvider.ForEachAsync(x => x.Source.LoadFrom(x.Filename), cancellRequested);
            }
            catch (TaskCanceledException)
            {
                Console.WriteLine("Terminating the process.");
            }
            // all is event-driven, so we can suspend this threat.
            cancellRequested.WaitHandle.WaitOne();
            Console.WriteLine("Finished.");
        }


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
