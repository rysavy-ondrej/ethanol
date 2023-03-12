using Ethanol.ContextBuilder.Builders;
using Ethanol.ContextBuilder.Enrichers;
using Ethanol.ContextBuilder.Plugins;
using Ethanol.ContextBuilder.Readers;
using Ethanol.ContextBuilder.Writers;
using NLog;
using NLog.Targets;
using NLog.Config;
using NLog.Layouts;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace Ethanol.ContextBuilder
{
    /// <summary>
    /// The program class containing the main entry point.
    /// </summary>
    public class Program : ConsoleAppBase
    {
        /// <summary>
        /// Entry point to the console application.
        /// </summary>
        /// <param name="args">Command line arguments.</param>
        public static void Main(string[] args)
        {
            try
            {
                AddLogging();
                ConsoleApp.Run<ProgramCommands>(args);
            }
            catch (Exception ex)
            {
                var logger = NLog.LogManager.GetCurrentClassLogger();
                logger.Fatal(ex, $"ERROR:{ex.Message}");
            }           
        }

        static void AddLogging()
        {
            var config = new LoggingConfiguration();

            // CONSOLE LOGGING:
            var consoleTarget = new ColoredConsoleTarget("console")
            {
                UseDefaultRowHighlightingRules = true,
                
                Layout = "${longdate}|${level}|${message}"
            };
            config.AddRule(LogLevel.Debug, LogLevel.Fatal, consoleTarget);


            var fileTarget = new FileTarget("file")
            {
                FileName = "Ethanol.ContextBuilder.log"
            };
            config.AddRule(LogLevel.Debug, LogLevel.Fatal, fileTarget);


            NLog.LogManager.Configuration = config;


            var logger = NLog.LogManager.GetCurrentClassLogger();
            logger.Info("Logging has been setup.");
        }
    }

    class CommandLineArgumentException : Exception
    {
        public CommandLineArgumentException(string argument, string reason) : base($"Invalid Argument: Argument:{argument}, Error: {reason}.")
        {
            Argument = argument;
            Reason = reason;
        }

        public string Argument { get; }
        public string Reason { get; }
    }
    public class ProgramCommands : ConsoleAppBase
    {
        /// <summary>
        /// Builds the context for input read by <paramref name="inputReader"/> using <paramref name="contextBuilder"/>. The output is written using <paramref name="outputWriter"/>.
        /// </summary>
        [Command("Build-Context", "Builds the context for flows.")]
        public async Task BuildContextCommand(
        [Option("r", "The reader module for processing input stream.")]
                string inputReader,
        [Option("c", "The configuration file used to configure the processing.")]
                string configurationFile,
        [Option("w", "The writer module for producing the output.")]
                string outputWriter
        )
        {
            var environment = new EthanolEnvironment();
            var readerRecipe = PluginCreateRecipe.Parse(inputReader) ?? throw new CommandLineArgumentException(nameof(inputReader), "Invalid recipe specified.");
            var writerRecipe = PluginCreateRecipe.Parse(outputWriter) ?? throw new CommandLineArgumentException(nameof(outputWriter), "Invalid recipe specified.");
            var configuration = PipelineConfiguration.LoadFrom(System.IO.File.ReadAllText(configurationFile)) ?? throw new CommandLineArgumentException(nameof(configurationFile), "Could not load the configuration.");

            var logger = NLog.LogManager.GetCurrentClassLogger();
            logger.Info("Initializing processing modules:");
            int inputCount = 0;
            int outputCount = 0;
            async Task MyTimer(CancellationToken ct)
            {
                while (!ct.IsCancellationRequested)
                {
                    logger.Info($"Status: consumed flows={inputCount}, produced contexts={outputCount}.");
                    await Task.Delay(10000);
                }
            }

            var reader = ReaderFactory.Instance.CreatePluginObject(readerRecipe.Name, readerRecipe.ConfigurationString) ?? throw new KeyNotFoundException($"Reader {readerRecipe.Name} not found!");
            logger.Info($"Created reader: {reader}, {readerRecipe}.");
            var writer = WriterFactory.Instance.CreatePluginObject(writerRecipe.Name, writerRecipe.ConfigurationString) ?? throw new KeyNotFoundException($"Writer {writerRecipe.Name} not found!");
            logger.Info($"Created writer: {writer}, {writerRecipe}.");

            logger.Info($"Setting up the processing pipeline.");

            var pipeline = environment.ContextBuilder.CreateIpHostContextBuilderPipeline(configuration, reader, writer, (x) => inputCount+=x, (x) => outputCount+=x);

            logger.Info($"Pipeline is ready, start processing input flows.");

            var cts = new CancellationTokenSource();
            var t = MyTimer(cts.Token);

            await Task.WhenAll(reader.StartReading(), writer.Completed).ContinueWith(t => cts.Cancel());

            logger.Info($"Processing of input stream completed.");
            logger.Info($"Processed {inputCount} input flows and wrote {outputCount} context objects.");

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
            Console.WriteLine("ENRICHERS:");
            foreach (var obj in ContextEnricherFactory.Instance.PluginObjects)
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

    public record PipelineConfiguration
    {
        static Deserializer deserializer = new YamlDotNet.Serialization.Deserializer();
        [YamlMember(Alias = "window-size", Description = "The time interval of the analysis window.")]
        public TimeSpan WindowSize { get; set; }
        
        
        [YamlMember(Alias = "window-hop", Description = "The hop interval of the window. ")]
        public TimeSpan WindowHop { get; set; }


        [YamlMember(Alias = "host-tag-enricher", Description = "The enricher configuration.")]
        public IpHostContextEnricherPlugin.DataSourceEnricherConfiguration HostTagEnricherConfiguration { get; set; }


        [YamlMember(Alias = "flow-tag-enricher", Description = "The enricher configuration.")]
        public IpHostContextEnricherPlugin.DataSourceEnricherConfiguration FlowTagEnricherConfiguration { get; set; }

        public static PipelineConfiguration LoadFrom(string text)
        {
            var config = deserializer.Deserialize<PipelineConfiguration>(text);
            return config;
        }
    }
}
