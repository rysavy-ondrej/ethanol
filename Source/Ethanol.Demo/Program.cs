using ConsoleAppFramework;
using Ethanol.Artifacts;
using Ethanol.Context;
using Ethanol.Providers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NRules;
using NRules.Fluent;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Ethanol.Demo
{
    class Program : ConsoleAppBase
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

        private static void cancelHandler(object sender, ConsoleCancelEventArgs e)
        {
            Console.WriteLine("Cancel key pressed. Exiting...");
            _cts.Cancel();
            System.Environment.Exit(0);
        }

        private static void ConfigureServices(HostBuilderContext context, IServiceCollection services)
        {                     
        }

        [Command("monitor-tor", "Monitor flows and provide near real-time information on TOR communication in etwork traffic.")]
        public async Task CreateContext(
            [Option("p", "path to data folder with source nfdump files.")]
            string dataPath,
            [Option("c", "path to a folder where csv files will be created.")]
            string csvPath)
        {
            var cancellRequested = _cts.Token;
            Console.WriteLine($"Monitor source folder: {dataPath}");
            var fileProvider = new SourceFileProvider(dataPath, csvPath,
                new SourceRecipe<ArtifactLong>("flow", "ipv4", new ArtifactSource<ArtifactLong>()),
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

   

        public abstract record SourceRecipe(string ArtifactName, string FilterExpression)
        {
            public abstract Type ArtifactType { get; }

            public abstract void LoadFrom(string filename);
        }
        public record SourceRecipe<T>(string ArtifactName, string FilterExpression, ArtifactSource<T> ArtifactSource) : SourceRecipe(ArtifactName, FilterExpression) where T : IpfixArtifact
        {
            public override void LoadFrom(string filename)
            {
                ArtifactSource.LoadFrom(filename);
            }

            public override Type ArtifactType => typeof(T);
        }

        public record CsvSourceFile(SourceRecipe Source, string Filename);
        
        public class ArtifactSource<T> : IObservable<T> where T : IpfixArtifact
        {
            IObservable<T> _observable;

            public ArtifactSource()
            {
                _observable = Observable.FromEvent<T>(
                    fsHandler => RecordFetched += fsHandler,
                    fsHandler => RecordFetched -= fsHandler);
            }

            event Action<T> RecordFetched;

            public void LoadFrom(string filename)
            {
                foreach(var obj in CsvArtifactProvider<T>.LoadFrom(filename))
                {
                    RecordFetched?.Invoke(obj);
                }
            }
            public IDisposable Subscribe(IObserver<T> observer)
            {
                return _observable.Subscribe(observer);
            }
        }

        public delegate void ArtifactHandler<T>(T value) where T : IpfixArtifact;


        public class SourceFileProvider : IObservable<CsvSourceFile>
        {
            FileSystemWatcher _watcher;
            string _inPath;
            private readonly string _outPath;
            SourceRecipe[] _recipes;
            private IObservable<CsvSourceFile> _observable;

            /// <summary>
            /// Creates a new instance watching the specified folder for newly created files.
            /// </summary>
            /// <param name="inPath"></param>
            public SourceFileProvider(string inPath, string outPath, params SourceRecipe[] recipes)
            {
                _inPath = inPath ?? throw new ArgumentNullException(nameof(inPath));
                this._outPath = outPath;
                _recipes = recipes;
                _watcher = new FileSystemWatcher
                {
                    Path = _inPath,
                    Filter = "*.*",
                    EnableRaisingEvents = true
                };
                _watcher.Created += new FileSystemEventHandler(OnCreated);

                _observable = Observable.FromEvent<CsvSourceFile>(
                    fsHandler => _SourceFileCreated += fsHandler,
                    fsHandler => _SourceFileCreated -= fsHandler);

            }

            private event Action<CsvSourceFile> _SourceFileCreated;

            /// <summary>
            /// Called when a new file was created in the given folder.
            /// </summary>
            /// <param name="source"></param>
            /// <param name="e"></param>
            private void OnCreated(object source, FileSystemEventArgs e)
            {
                Console.WriteLine($"New flow dump observed: {e.FullPath}.");
               
                foreach (var filter in _recipes)
                {
                    var filename = Path.GetFileName(e.FullPath);
                    var outputFile = Path.Combine(_outPath, $"{filename}.{filter.ArtifactName}");
                    var success = ExecuteNfdump(e.FullPath, outputFile, filter.FilterExpression);
                    if (success)
                    {
                        Console.WriteLine($"  Source file converted to CSV: {e.FullPath} -> {outputFile}");
                    }
                    _SourceFileCreated?.Invoke(new CsvSourceFile(filter, outputFile));
                }                
            }

            private bool ExecuteNfdump(string sourceFile, string targetFile, string filter)
            {
                return ShellExecute(Path.GetTempPath(), "nfdump", $"-R {sourceFile}", "-o csv", $"'{filter}' > {targetFile}") == 0;
            }
            private int ShellExecute(string path, string command, params string[] arguments)
            {
                var argumentString = string.Join(" ", arguments);
                Console.WriteLine($"{command} {argumentString}");
                try
                {
                    using (var process = Process.Start(new ProcessStartInfo { WorkingDirectory = path, FileName = command, Arguments = argumentString, UseShellExecute = false }))
                    {
                        process.WaitForExit(10000);
                        if (!process.HasExited)
                        {
                            process.Kill();
                        }
                        return process.ExitCode;
                    }
                }
                catch(Exception e)
                {
                    Console.Error.WriteLine($"Error executing nfdump {e.Message}");
                    return -1;
                }
            }

            public IDisposable Subscribe(IObserver<CsvSourceFile> observer)
            {
                return _observable.Subscribe(observer);
            }
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
