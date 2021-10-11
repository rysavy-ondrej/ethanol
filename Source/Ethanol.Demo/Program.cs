using ConsoleAppFramework;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.StreamProcessing;
using System;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
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

        public enum OutputFormat { Json, Yaml }
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
        [Command("analyze-flows", "Create flow context and analyze it.")]
        public async Task AnalyzeFlowsCommand(
        [Option("dumpSource", "path to data folder with source nfdump files.")]
                string dumpSource = null,
        [Option("csvSource", "path to data folder with source nfdump files.")]
                string csvSource = null,
        [Option("csvTarget", "write intermediate CSV files to the given folder")]
                string csvTarget=null,
        [Option("outFormat", "the format for generated output")]
                OutputFormat outFormat =OutputFormat.Yaml
        )
        {
            var dumpInput = dumpSource != null;
            var sourcePath = dumpSource ?? csvSource ?? throw new ArgumentException($"One of {nameof(dumpSource)} or {nameof(csvSource)} must be specified.");
            var sourceFiles = Directory.GetFiles(sourcePath).Select(fileName => new FileInfo(fileName)).OrderBy(f => f.Name).ToObservable();
            var configuration = new FlowProcessor.Configuration(!dumpInput, csvTarget != null, csvTarget);
            await AnalyzeFlowsInFiles(sourceFiles, configuration, outFormat);
        }
    }
}
