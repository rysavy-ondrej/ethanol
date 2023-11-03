using Ethanol.ContextBuilder.Builders;
using Ethanol.ContextBuilder.Context;
using Ethanol.ContextBuilder.Enrichers;
using Ethanol.ContextBuilder.Enrichers.TagObjects;
using Ethanol.ContextBuilder.Enrichers.TagProviders;
using Ethanol.ContextBuilder.Enrichers.TagSources;
using Ethanol.ContextBuilder.Helpers;
using Ethanol.ContextBuilder.Pipeline;
using Ethanol.ContextBuilder.Plugins;
using Ethanol.ContextBuilder.Readers;
using Ethanol.ContextBuilder.Writers;
using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Ethanol.ContextBuilder
{
    /// <summary>
    /// This class implements commands available in the CLI application.
    /// </summary>
    public class ProgramCommands : ConsoleAppBase
    {
        /// <summary>
        /// Builds the context for input read by <paramref name="inputReader"/> using the configuration in <paramref name="configurationFile"/>. The output, after building the context, is written using <paramref name="outputWriter"/>.
        /// </summary>
        /// <remarks>
        /// This method is intended to be used for configuring and building the context of flows. It takes input from the specified reader, processes it based on the given configuration, 
        /// and writes the output using the specified writer.
        /// <para/>
        /// The processing can be controlled by configuration file. For more information on the configuration file see Configuration-file.md.
        /// </remarks>
        /// <param name="inputReader">The reader module for processing input stream. This specifies which reader to use for reading the input data.</param>
        /// <param name="configurationFile">The configuration file used to configure the processing. This file contains settings and parameters that dictate 
        /// how the input is to be processed.</param>
        /// <param name="outputWriter">The writer module for producing the output. This specifies which writer to use for writing the processed output.</param>
        [Command("run", "Starts the application.")]
        public async Task RunCommand(
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

            var logger = LogManager.GetCurrentClassLogger();
            logger.LogInformation("Initializing processing modules:");
            int inputCount = 0;
            int outputCount = 0;
            async Task MyTimer(CancellationToken ct)
            {
                while (!ct.IsCancellationRequested)
                {
                    logger.LogInformation($"Status: consumed flows={inputCount}, produced contexts={outputCount}.");
                    await Task.Delay(10000);
                }
            }

            var reader = ReaderFactory.Instance.CreatePluginObject(readerRecipe.Name, readerRecipe.ConfigurationString) ?? throw new KeyNotFoundException($"Reader {readerRecipe.Name} not found!");
            logger.LogInformation($"Created reader: {reader}, {readerRecipe}.");
            var writer = WriterFactory.Instance.CreatePluginObject(writerRecipe.Name, writerRecipe.ConfigurationString) ?? throw new KeyNotFoundException($"Writer {writerRecipe.Name} not found!");
            logger.LogInformation($"Created writer: {writer}, {writerRecipe}.");

            logger.LogInformation($"Setting up the processing pipeline.");

            var pipeline = environment.ContextBuilder.CreateIpHostContextBuilderPipeline(configuration, reader, writer, (x) => inputCount+=x, (x) => outputCount+=x);

            logger.LogInformation($"Pipeline is ready, start processing input flows.");

            var cts = new CancellationTokenSource();
            var t = MyTimer(cts.Token);
            reader.StartReading(cts.Token);

            await pipeline.Completed.ContinueWith(t => cts.Cancel());

            logger.LogInformation($"Processing of input stream completed.");
            logger.LogInformation($"Processed {inputCount} input flows and wrote {outputCount} context objects.");

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
        /// <summary>
        /// Updates (deletes and then inserts) Netify tags into the specified SQL table using data from the provided CSV files.
        /// </summary>
        /// <remarks>
        /// This command is specifically designed for handling Netify tags. The existing tags in the table will be deleted, 
        /// and then the new tags from the CSV files will be inserted. The operation is based on the connection string to the SQL 
        /// database and uses the provided table name.
        /// </remarks>
        /// <param name="connectionString">The connection string for the SQL database where the updates will occur.</param>
        /// <param name="tableName">The name of the table in the SQL database where the Netify tags will be updated.</param>
        /// <param name="appsFile">The CSV file containing data related to applications for the Netify tags.</param>
        /// <param name="ipsFile">The CSV file containing data related to IPs for the Netify tags.</param>
        /// <param name="domainsFile">The CSV file containing data related to domains for the Netify tags.</param>
        [Command("Insert-Netify", "Updates (deletes and then inserts) Netify tags into the specified SQL table using data from the provided CSV files.")]
        public void InsertNetify(string connectionString, string tableName, string appsFile, string ipsFile, string domainsFile)
        {
            var logger = LogManager.GetCurrentClassLogger();

            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                PostgresTagProvider.CreateTableIfNotExists(conn, tableName);

                logger.LogInformation($"Removing existing netify tags.");
                var removedRows = NpgsqlTableHelper.Delete(conn, tableName, $"type = '{nameof(NetifyTag)}'");
                logger.LogInformation($"{removedRows} rows deleted.");
                logger.LogInformation($"Tag table: {tableName}");
                logger.LogInformation($"Loading applications from '{appsFile}'...");
                var applications = CsvNetifyTagSource.LoadApplicationsFromFile(appsFile);
                logger.LogInformation($"Loaded {applications.Count} applications.");

                logger.LogInformation($"Loading ips from '{ipsFile}'...");
                var addresses = CsvNetifyTagSource.LoadAddressesFromFile(ipsFile);
                var addressTags = ConvertToTag(applications, addresses);
                var addressesInserted = PostgresTagProvider.BulkInsert(conn, tableName, addressTags);
                logger.LogInformation($"Inserted {addressesInserted} addresses.");

                logger.LogInformation($"Loading domains from '{domainsFile}'...");
                var domains = CsvNetifyTagSource.LoadDomainsFromFile(domainsFile);
                var domainTags = ConvertToTag(applications, domains);
                var domainsInserted = PostgresTagProvider.BulkInsert(conn, tableName, domainTags);
                logger.LogInformation($"Inserted {domainsInserted} domains.");
            }
        }

        private static IEnumerable<TagObject> ConvertToTag(IDictionary<int, CsvNetifyTagSource.NetifyAppRecord> applications, IEnumerable<CsvNetifyTagSource.NetifyIpsRecord> addresses)
        {
            return addresses.Select(item =>
            {
                var app = applications.TryGetValue(item.AppId, out var appRec) ? appRec : null;
                var record = new TagObject 
                { 
                    Key = item.Value, 
                    Type = nameof(NetifyTag), 
                    Value = app?.Tag ?? String.Empty, 
                    Reliability = 1.0, 
                    StartTime = DateTime.MinValue, 
                    EndTime = DateTime.MaxValue  
                };
                record.Details = CsvNetifyTagSource.ConvertToTag(app);
                return record;
            });
        }
        private static IEnumerable<TagObject> ConvertToTag(IDictionary<int, CsvNetifyTagSource.NetifyAppRecord> applications, IEnumerable<CsvNetifyTagSource.NetifyDomainRecord> domains)
        {
            return domains.Select(item =>
            {
                var app = applications.TryGetValue(item.AppId, out var appRec) ? appRec : null;
                var record = new TagObject
                {
                    Key = item.Value,
                    Type = nameof(NetifyTag),
                    Value = app?.Tag ?? String.Empty,
                    Reliability = 1.0,
                    StartTime = DateTime.MinValue,
                    EndTime = DateTime.MaxValue
                };
                record.Details = CsvNetifyTagSource.ConvertToTag(app);
                return record;
            });
        }

        /// <summary>
        /// Inserts flow tags from the provided JSON file into the specified table in an SQL database.
        /// </summary>
        /// <remarks>
        /// This command is designed to facilitate the insertion of FlowTag data, typically used for network flows, 
        /// from a given JSON file format. The insertion is done to the SQL table specified by the provided table name and connection string.
        /// </remarks>
        /// <param name="connectionString">The connection string for the SQL database, typically in the format: "Server=localhost;Port=1605;Database=ethanol;User Id=postgres;Password=postgres;". This string provides all the necessary details to establish a connection to the desired database.</param>
        /// <param name="tableName">The name of the table in the SQL database where the flow tags from the JSON file will be inserted. This table should have a schema that matches the structure of the FlowTag data in the JSON file.</param>
        /// <param name="inputFile">The path to the JSON file containing the FlowTag data to be inserted into the SQL table. Ensure the file is accessible and in a valid JSON format that matches the expected FlowTag structure.</param>

        [Command("Insert-FlowTags", "Inserts flow tags from the provided JSON file into the specified table in an SQL database.")]
        public void InsertFlowTags(string connectionString, string tableName, string inputFile)
        {
            var logger = LogManager.GetCurrentClassLogger();
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();

                // test if the table exists:
                PostgresTagProvider.CreateTableIfNotExists(conn, tableName);
                logger.LogInformation($"FlowTags table exists: {tableName}.");
                var records = CsvFlowTagSource.LoadFromFile(inputFile);
                logger.LogInformation($"Loading flow tags from '{inputFile}'...");
                var ftCount = PostgresTagProvider.BulkInsert(conn, tableName, records);
                logger.LogInformation($"Inserted {ftCount} flow tags.");
            }
        }

        /// <summary>
        /// Inserts host tags from the provided JSON file into the specified table in an SQL database.
        /// </summary>
        /// <remarks>
        /// This command is crafted to facilitate the insertion of HostTag data, which is primarily used to denote specific characteristics or details of hosts. The data is read from a specified JSON file and is then inserted into the SQL table determined by the provided table name and connection string.
        /// </remarks>
        /// <param name="connectionString">The connection string for the SQL database, often in the format: "Server=localhost;Port=1605;Database=ethanol;User Id=postgres;Password=postgres;". This string encompasses all the vital details needed to initiate a connection to the target database.</param>
        /// <param name="tableName">The name of the table within the SQL database where the host tags from the JSON file will be inserted. It is important to ensure that this table's schema aligns with the structure of the HostTag data in the JSON input.</param>
        /// <param name="inputFile">The path to the JSON file containing the HostTag data. This file will be used as the data source for insertion into the SQL table. It's imperative to confirm that the file is both accessible and adheres to a valid JSON format, consistent with the expected HostTag structure.</param>
        [Command("Insert-HostTags", "Inserts host tags from the provided JSON file into the specified table in an SQL database.")]
        public void InsertHostTags(string connectionString, string tableName, string inputFile)
        {
            var logger = LogManager.GetCurrentClassLogger();
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                PostgresTagProvider.CreateTableIfNotExists(conn, tableName);

                logger.LogInformation($"HostTags table exists: {tableName}.");
                var records = CsvHostTagSource.LoadFromFile(inputFile);
                logger.LogInformation($"Loading host tags from '{inputFile}'...");
                var count = PostgresTagProvider.BulkInsert(conn, tableName, records);
                logger.LogInformation($"Inserted {count} host tags.");
            }
        }
    }
}
