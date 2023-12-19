using Ethanol;
using Ethanol.ContextBuilder.Context;
using Ethanol.ContextBuilder.Enrichers.TagObjects;
using Ethanol.ContextBuilder.Enrichers.TagSources;
using Ethanol.ContextBuilder.Helpers;
using Microsoft.Extensions.Logging;
using Npgsql;

/// <summary>
/// This class implements commands available in the CLI application.
/// </summary>
[Command("tags", "Provides commands for working with tags.")]
public class TagsHelperCommands : ConsoleAppBase
{

    private readonly ILogger _logger;
    private readonly EthanolEnvironment _environment;

    public TagsHelperCommands(ILogger<TagsHelperCommands> logger, EthanolEnvironment environment)
    {
        _logger = logger;
        _environment = environment;
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
    [Command("insert-netify", "Inserts netify tags from the provided CSV files into the specified table in an SQL database.")]
    public void InsertNetify(string connectionString, string tableName, string appsFile, string ipsFile, string domainsFile)
    {
        using (var conn = new NpgsqlConnection(connectionString))
        {
            conn.Open();
            PostgresTagDataSource.CreateTableIfNotExists(conn, tableName);

            _logger.LogInformation($"Removing existing netify tags.");
            var removedRows = NpgsqlTableHelper.Delete(conn, tableName, $"type = 'NetifyDomain' OR type = 'NetifyIp'");
            _logger.LogInformation($"{removedRows} rows deleted.");
            _logger.LogInformation($"Tag table: {tableName}");
            _logger.LogInformation($"Loading applications from '{appsFile}'...");
            var applications = CsvNetifyTagSource.LoadApplicationsFromFile(appsFile);
            _logger.LogInformation($"Loaded {applications.Count} applications.");

            _logger.LogInformation($"Loading ips from '{ipsFile}'...");
            var addresses = CsvNetifyTagSource.LoadAddressesFromFile(ipsFile);
            var addressTags = ConvertToTag(applications, addresses);
            var addressesInserted = PostgresTagDataSource.BulkInsert(conn, tableName, addressTags);
            _logger.LogInformation($"Inserted {addressesInserted} addresses.");

            _logger.LogInformation($"Loading domains from '{domainsFile}'...");
            var domains = CsvNetifyTagSource.LoadDomainsFromFile(domainsFile);
            var domainTags = ConvertToTag(applications, domains);
            var domainsInserted = PostgresTagDataSource.BulkInsert(conn, tableName, domainTags);
            _logger.LogInformation($"Inserted {domainsInserted} domains.");
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
                Type = "NetifyIp",
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
                Type = "NetifyDomain",
                Value = app?.Tag ?? String.Empty,
                Reliability = 1.0,
                StartTime = DateTime.MinValue,
                EndTime = DateTime.MaxValue
            };
            record.Details = CsvNetifyTagSource.ConvertToTag(app);
            return record;
        });
    }
}
