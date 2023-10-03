using Ethanol.ContextBuilder.Builders;
using Ethanol.ContextBuilder.Enrichers;
using Ethanol.ContextBuilder.Pipeline;
using Ethanol.ContextBuilder.Plugins;
using Ethanol.ContextBuilder.Readers;
using Ethanol.ContextBuilder.Writers;
using NLog;
using NLog.Config;
using NLog.Targets;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;


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
                DetectConsoleAvailable = true,
                DetectOutputRedirected= true,
                StdErr= true,
                Layout = "${longdate}|${level}|${message}"
            };
            config.AddRule(LogLevel.Debug, LogLevel.Fatal, consoleTarget);


            var fileTarget = new FileTarget("file")
            {
                FileName = "Ethanol.ContextBuilder.trace"
            };
            config.AddRule(LogLevel.Trace, LogLevel.Fatal, fileTarget);


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
    public partial class ProgramCommands : ConsoleAppBase
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
            var configuration = PipelineConfigurationSerializer.LoadFromFile(configurationFile) ?? throw new CommandLineArgumentException(nameof(configurationFile), "Could not load the configuration.");

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

        [Command("Insert-Netify", "Inserts Netify data from the given CSV files to SQL database.")]
        public void InsertToNetifyTables(string connectionString, string tablePrefix, string appsFile, string ipsFile, string domainsFile)
        {
            var logger = NLog.LogManager.GetCurrentClassLogger();

            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                PostgresNetifyTagProvider.CreateTables(conn, tablePrefix, true);

                logger.Info($"Netify tables exist: {tablePrefix}_applications, {tablePrefix}_addresses,  {tablePrefix}_domains.");
                logger.Info($"Loading applications from '{appsFile}'...");
                var appsCount = PostgresNetifyTagProvider.ApplicationsBulkInsert(conn, appsFile, tablePrefix + "_applications");
                logger.Info($"Inserted {appsCount} applications.");

                logger.Info($"Loading ips from '{ipsFile}'...");
                var ipsCount = PostgresNetifyTagProvider.AddressesBulkInsert(conn, ipsFile, tablePrefix + "_addresses");
                logger.Info($"Inserted {ipsCount} addresses.");

                logger.Info($"Loading domains from '{domainsFile}'...");
                var domsCount = PostgresNetifyTagProvider.DomainsBulkInsert(conn, domainsFile, tablePrefix + "_domains");
                logger.Info($"Inserted {domsCount} domains.");
            }
        }

        /// <summary>
        /// Inserts flow tags from JSON file to SQL database.
        /// </summary>
        /// <param name="connectionString">String like this: "Server=localhost;Port=1605;Database=ethanol;User Id=postgres;Password=postgres;"</param>
        /// <param name="tableName"></param>
        /// <param name="inputfile"></param>
        [Command("Insert-FlowTags", "Inserts to SQL database FlowTag data from the given input file.")]
        public void InsertFlowTags(string connectionString, string tableName, string inputFile)
        {
            var logger = NLog.LogManager.GetCurrentClassLogger();
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();

                // test if the table exists:
                PostgresFlowTagProvider.CreateTableIfNotExists(conn, tableName);
                logger.Info($"FlowTags table exists: {tableName}.");

                logger.Info($"Loading flow tags from '{inputFile}'...");
                var ftCount = PostgresFlowTagProvider.BulkInsert(conn, inputFile, tableName);
                logger.Info($"Inserted {ftCount} flow tags.");
            }
        }

        /// <summary>
        /// Inserts flow tags from JSON file to SQL database.
        /// </summary>
        /// <param name="connectionString">String like this: "Server=localhost;Port=1605;Database=ethanol;User Id=postgres;Password=postgres;"</param>
        /// <param name="tableName"></param>
        /// <param name="inputfile"></param>
        [Command("Insert-HostTags", "Inserts to SQL database HostTag data from the given input file.")]
        public void InsertHostTags(string connectionString, string tableName, string inputFile)
        {
            var logger = NLog.LogManager.GetCurrentClassLogger();
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();

                // test if the table exists:
                PostgresHostTagProvider.CreateTableIfNotExists(conn, tableName);
                logger.Info($"HostTags table exists: {tableName}.");

                logger.Info($"Loading host tags from '{inputFile}'...");
                var count = PostgresHostTagProvider.BulkInsert(conn, inputFile, tableName);
                logger.Info($"Inserted {count} host tags.");
            }
        }
    }
}
