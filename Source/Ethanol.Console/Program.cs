using ConsoleAppFramework;  // https://github.com/Cysharp/ConsoleAppFramework
using Ethanol.ContextBuilder.Builders;
using Ethanol.ContextBuilder.Readers;
using Ethanol.ContextBuilder.Writers;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Ethanol.ContextBuilder
{
    /// <summary>
    /// The program class. It sets up the environment and registers available commands.
    /// </summary>
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
            System.Console.CancelKeyPress += new ConsoleCancelEventHandler(CancelHandler);
            var verbose = args.Contains("-v");
            var hostBuilder = Host.CreateDefaultBuilder()
                .ConfigureLogging(b => ConfigureLogging(b, verbose ? LogLevel.Trace : LogLevel.Information))
                .UseConsoleAppFramework<Program>(args);
            var host = hostBuilder.Build();
            _services = host.Services;
            try
            {
                await host.RunAsync(_cancellationTokenSource.Token);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error: {ex.Message}$");
            }
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
        /// Builds the context for input read by <paramref name="inputReader"/> using <paramref name="contextBuilder"/>. The output is written using <paramref name="outputWriter"/>.
        /// </summary>
        [Command("Build-Context", "Builds the context for flows.")]
        public async Task BuildContextCommand(
        [Option("r", "The reader module for processing input stream.")]
                string inputReader,
        [Option("b", "The builder module to create a context.")]
                string contextBuilder,
        [Option("w", "The writer module for producing the output.")]
                string outputWriter
        )
        {
            var sw = new Stopwatch();
            sw.Start();
            var inputModule = ModuleSpecification.Parse(inputReader);
            var builderModule = ModuleSpecification.Parse(contextBuilder);
            var outputModule = ModuleSpecification.Parse(outputWriter);

            Console.Error.WriteLine($"[{sw.Elapsed}] Initializing modules:");

            var reader = FlowReaderFactory.GetReader(inputModule) ?? throw new KeyNotFoundException($"Reader {inputModule.Name} not found!");
            Console.Error.WriteLine($"                   Reader: {reader}");
            var builder = ContextBuilderFactory.GetBuilder(builderModule) ?? throw new KeyNotFoundException($"Builder {builderModule.Name} not found!");
            Console.Error.WriteLine($"                   Builder: {builder}");
            var writer = WriterFactory.GetWriter(outputModule) ?? throw new KeyNotFoundException($"Writer {outputModule.Name} not found!");
            Console.Error.WriteLine($"                   Writer: {writer}");

            Console.Error.WriteLine($"[{sw.Elapsed}] Setting up the pipeline...");
            using var d1 = reader.Subscribe(builder);
            using var d2 = builder.Subscribe(writer);
            Console.Error.WriteLine($"[{sw.Elapsed}] Pipeline is ready, processing input flows...");
            await Task.WhenAll(reader.StartReading(), writer.Completed);
            Console.Error.WriteLine($"[{sw.Elapsed}] Finished!");
        }

        [Command("List-Modules", "Provides information on available modules.")]
        public async Task ListModulesCommand(
            )
        {
            Console.WriteLine("READERS:");
            Console.WriteLine($"  {nameof(FlowmonJsonReader)}      reads JSON file with IPFIX data produced by flowmonexp5 tool.");
            
            Console.WriteLine("BUILDERS:");
            Console.WriteLine($"  {nameof(TlsFlowContextBuilder)}  builds the context for TLS flows in the source IPFIX stream.");
            Console.WriteLine($"  {nameof(IpHostContextBuilder)}   builds the context for Ip hosts identified in the source IPFIX stream.");

            Console.WriteLine("WRITERS:");
            Console.WriteLine($"  {nameof(JsonDataWriter)}         writes NDJSON formatted file for computed context.");
            Console.WriteLine($"  {nameof(YamlDataWriter)}         writes YAML formatted file for computed context.");
        }
    }
}
