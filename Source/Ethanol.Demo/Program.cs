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
        [Command("detect-tor", "Detect Tor in existing network traffic.")]
        public async Task DetectTorCommand(
        [Option("dumpSource", "path to data folder with source nfdump files.")]
                string dumpSource = null,
        [Option("csvSource", "path to data folder with source nfdump files.")]
                string csvSource = null,
        [Option("entropy", "minimum entropy of server name")]
                double entropy = 3.0,
        [Option("outputFormat", "output format, can be 'Yaml' or 'Json'")]
                OutputFormat outputFormat = OutputFormat.Yaml,
        [Option("csvTarget", "write intermediate CSV files to the given folder")]
                string csvTarget=null
        )
        {
            var dumpInput = dumpSource != null;
            var sourcePath = dumpSource ?? csvSource ?? throw new ArgumentException($"One of {nameof(dumpSource)} or {nameof(csvSource)} must be specified.");
            var sourceFiles = Directory.GetFiles(sourcePath).Select(fileName => new FileInfo(fileName)).OrderBy(f => f.Name).ToObservable();
            var configuration = new DetectTorConfiguration(entropy, outputFormat, !dumpInput, csvTarget != null, csvTarget);
            await DetectTor(sourceFiles, configuration);
        }
        [Command("detect-sshcure", "Detect SshCure activities in network traffic.")]
        public async Task DetectSshCureCommand(
        [Option("dumpSource", "path to data folder with source nfdump files.")]
                        string dumpSource = null,
        [Option("csvSource", "path to data folder with source nfdump files.")]
                        string csvSource = null,
        [Option("outputFormat", "output format, can be 'Yaml' or 'Json'")]
                        OutputFormat outputFormat = OutputFormat.Yaml,
        [Option("csvTarget", "write intermediate CSV files to the given folder")]
                        string csvTarget=null
        )
        {
            var dumpInput = dumpSource != null;
            var sourcePath = dumpSource ?? csvSource ?? throw new ArgumentException($"One of {nameof(dumpSource)} or {nameof(csvSource)} must be specified.");
            var sourceFiles = Directory.GetFiles(sourcePath).Select(fileName => new FileInfo(fileName)).OrderBy(f => f.Name).ToObservable();
            var configuration = new DetectSshCureConfiguration(outputFormat, !dumpInput, csvTarget != null, csvTarget);
            await DetectSshCure(sourceFiles, configuration);
        }
    }
}
