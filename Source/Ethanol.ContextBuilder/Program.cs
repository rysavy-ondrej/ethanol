using ConsoleAppFramework;              //----> https://github.com/Cysharp/ConsoleAppFramework
using Ethanol.ContextBuilder.Builders;
using Ethanol.ContextBuilder.Plugins;
using Ethanol.ContextBuilder.Readers;
using Ethanol.ContextBuilder.Writers;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Reactive.Linq;
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

        /// <summary>
        /// The entry point of the program.
        /// </summary>
        /// <param name="args">Input arguments.</param>
        /// <returns>A task that ends when the program is finished/terminated.</returns>
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
            var readerRecipe = PluginCreateRecipe.Parse(inputReader);
            var builderRecipe = PluginCreateRecipe.Parse(contextBuilder);
            var writerRecipe = PluginCreateRecipe.Parse(outputWriter);

            Console.Error.WriteLine($"[{sw.Elapsed}] Initializing modules:");

            int inputCount = 0;
            int outputCount = 0;
            async Task MyTimer(CancellationToken ct)
            {
                while (!ct.IsCancellationRequested)
                {
                    Console.Error.WriteLine($"[{sw.Elapsed}] in={inputCount}, out={outputCount}             \r");
                    await Task.Delay(1000); 
                }
            }


            var reader = ReaderFactory.Instance.CreatePluginObject(readerRecipe.Name, readerRecipe.ConfigurationString) ?? throw new KeyNotFoundException($"Reader {readerRecipe.Name} not found!");
            Console.Error.WriteLine($"                   Reader: {reader}");
            var builder = ContextBuilderFactory.Instance.CreatePluginObject(builderRecipe.Name, builderRecipe.ConfigurationString) ?? throw new KeyNotFoundException($"Builder {builderRecipe.Name} not found!");
            Console.Error.WriteLine($"                   Builder: {builder}");
            var writer = WriterFactory.Instance.CreatePluginObject(writerRecipe.Name, writerRecipe.ConfigurationString) ?? throw new KeyNotFoundException($"Writer {writerRecipe.Name} not found!");
            Console.Error.WriteLine($"                   Writer: {writer}");

            Console.Error.WriteLine($"[{sw.Elapsed}] Setting up the pipeline...");
            
            reader.Do(x=>inputCount++).Subscribe(builder);
            builder.Do(x=>outputCount++).Subscribe(writer);

            Console.Error.WriteLine($"[{sw.Elapsed}] Pipeline is ready, processing input flows...");

            var cts = new CancellationTokenSource();
            var t = MyTimer(cts.Token);

            await Task.WhenAll(reader.StartReading(), writer.Completed).ContinueWith( t => cts.Cancel());

            Console.Error.WriteLine($"[{sw.Elapsed}] Finished!");
        }

        /// <summary>
        /// Gets the list of all available modules. 
        /// </summary>
        [Command("List-Modules", "Provides information on available modules.")]
        public void ListModulesCommand()
        {
            Console.WriteLine("READERS:");
            foreach (var obj in ReaderFactory.Instance.PluginObjects)
            {
                Console.WriteLine($"  {obj.Name}    {obj.Description}");
            }
            Console.WriteLine("BUILDERS:");
            foreach (var obj in ContextBuilderFactory.Instance.PluginObjects)
            {
                Console.WriteLine($"  {obj.Name}    {obj.Description}");
            }
            Console.WriteLine("WRITERS:");
            foreach (var obj in WriterFactory.Instance.PluginObjects)
            {
                Console.WriteLine($"  {obj.Name}    {obj.Description}");
            }
        }
    }
}
