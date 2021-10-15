using ConsoleAppFramework;
using CsvHelper;
using Ethanol.Providers;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.StreamProcessing;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

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
        /// <summary>
        /// Represents a possible file format of a data file.
        /// </summary>
        public enum DataFileFormat { Json, Yaml, Csv, Nfd }


        /// <summary>
        /// YAML serializer used to produce output.
        /// </summary>
        readonly ISerializer yamlSerializer = new SerializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).DisableAliases().Build();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataPath"></param>
        /// <param name="csvPath"></param>
        /// <returns></returns>
        [Command("Test-CsvFlows", "Tests and analyses flows from the given Csv files.")]
        public async Task TestCsvFlows(
        [Option("s", "path to data folder with source csv files.")]
                string source,
        [Option("f", "the format for generated output")]
                DataFileFormat outputFormat = DataFileFormat.Yaml
        )
        {
            var sourceFiles = TestAndGetFiles(source);
            await AnalyzeFlowsInFiles(sourceFiles, DataFileFormat.Csv, outputFormat);
        }

        /// <summary>
        /// Test if the given <paramref name="source"/> path points to the valid directory with source files.
        /// </summary>
        /// <param name="source">the source path.</param>
        /// <returns>An observable collection of files in the given <paramref name="source"/> path.</returns>
        /// <exception cref="ArgumentException">If the path does not point to the existing directory.</exception>
        private static IObservable<FileInfo> TestAndGetFiles(string source)
        {
            if (!Directory.Exists(source)) throw new ArgumentException($"Argument {nameof(source)} must be specified and point to existing folder.");
            var sourceFiles = Directory.GetFiles(source).Select(fileName => new FileInfo(fileName)).OrderBy(f => f.Name).ToObservable();
            return sourceFiles;
        }

        [Command("ConvertFrom-Nfd", "Converts nfdump files to the specified format.")]
        public async Task ConvertFromNfd(
            [Option("s", "a path to source folder with nfdump files.")]
                string source,
            [Option("t", "a path to folder where target files will be created.")]
                string target,
            [Option("f", "a file format of the target files")]
                DataFileFormat format = DataFileFormat.Csv
            )
        {
            if (format == DataFileFormat.Nfd) throw new ArgumentException($"The specified file format ({format}) is not supported in this operation.");
            var sourceFiles = TestAndGetFiles(source);
            
            
            var ethanol = new EthanolEnvironment();
            var loader = new CsvLoader<IpfixRecord>();
                var records = new List<IpfixRecord>();
                loader.OnStartLoading += (_, filename) => { records.Clear(); };
                loader.OnReadRecord += (_, record) => { records.Add(record); };
                loader.OnFinish += (_, filename) => { WriteAllRecords(Path.Combine(target, $"{filename}.{format.ToString().ToLowerInvariant()}"), records); };

            await ethanol.DataLoader.LoadFromNfdFiles(sourceFiles, loader, _cancellationTokenSource.Token);

            void WriteAllRecords(string filename, List<IpfixRecord> records)
            {
                switch (format)
                {
                    case DataFileFormat.Csv:
                        using (var writer = new StreamWriter(filename))
                        using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                        {
                            csv.WriteRecords(records);
                        }
                        break;
                    case DataFileFormat.Yaml:
                        using (var writer = new StreamWriter(filename))
                        {
                            writer.Write(yamlSerializer.Serialize(records));
                        }
                        break;
                    case DataFileFormat.Json:
                        using (var writer = new StreamWriter(filename))
                        {
                            writer.Write(JsonSerializer.Serialize(records));
                        }
                        break;
                }
            }
        }
    }
}
