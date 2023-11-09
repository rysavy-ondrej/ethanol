using Ethanol.ContextBuilder.Context;
using Ethanol.ContextBuilder.Enrichers.TagObjects;
using Ethanol.ContextBuilder.Enrichers.TagProviders;
using Ethanol.ContextBuilder.Enrichers.TagSources;
using Ethanol.ContextBuilder.Helpers;
using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ethanol.ContextBuilder
{
    /// <summary>
    /// This class implements commands available in the CLI application.
    /// </summary>
    public class ProgramCommands : ConsoleAppBase
    {
          
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
        public void InsertNetify(string connectionString, string tableName, string appsFile, string ipsFile, string domainsFile, ILogger logger)
        {
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                PostgresTagDataSource.CreateTableIfNotExists(conn, tableName);

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
                var addressesInserted = PostgresTagDataSource.BulkInsert(conn, tableName, addressTags);
                logger.LogInformation($"Inserted {addressesInserted} addresses.");

                logger.LogInformation($"Loading domains from '{domainsFile}'...");
                var domains = CsvNetifyTagSource.LoadDomainsFromFile(domainsFile);
                var domainTags = ConvertToTag(applications, domains);
                var domainsInserted = PostgresTagDataSource.BulkInsert(conn, tableName, domainTags);
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
        public void InsertFlowTags(string connectionString, string tableName, string inputFile, ILogger logger)
        {
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();

                // test if the table exists:
                PostgresTagDataSource.CreateTableIfNotExists(conn, tableName);
                logger.LogInformation($"FlowTags table exists: {tableName}.");
                var records = CsvFlowTagSource.LoadFromFile(inputFile);
                logger.LogInformation($"Loading flow tags from '{inputFile}'...");
                var ftCount = PostgresTagDataSource.BulkInsert(conn, tableName, records);
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
        public void InsertHostTags(string connectionString, string tableName, string inputFile, ILogger logger)
        {
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                PostgresTagDataSource.CreateTableIfNotExists(conn, tableName);

                logger.LogInformation($"HostTags table exists: {tableName}.");
                var records = CsvHostTagSource.LoadFromFile(inputFile);
                logger.LogInformation($"Loading host tags from '{inputFile}'...");
                var count = PostgresTagDataSource.BulkInsert(conn, tableName, records);
                logger.LogInformation($"Inserted {count} host tags.");
            }
        }
    }
}
