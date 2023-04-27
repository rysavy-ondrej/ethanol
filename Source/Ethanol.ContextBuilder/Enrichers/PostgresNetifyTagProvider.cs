using Microsoft.CodeAnalysis.CSharp.Syntax;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Threading.Tasks;

namespace Ethanol.ContextBuilder.Enrichers
{
    /// <summary>
    /// Represents a Netify-provided data stored in PostgreSQL database.
    /// </summary>
    /// <remarks>
    /// It is expected that the application table has the following structure:
    /// <code>
    /// CREATE TABLE netify_applications (
    ///     id INT PRIMARY KEY,
    ///     tag VARCHAR(100),
    ///     short_name VARCHAR(50),
    ///     full_name VARCHAR(100),
    ///     description TEXT,
    ///     url VARCHAR(255),
    ///     category VARCHAR(50)
    /// );
    /// </code>
    /// The ip address table used to first lookup has the following structure:
    /// <code>
    /// CREATE TABLE netify_addresses (
    ///     id INT PRIMARY KEY,
    ///     value VARCHAR(50),
    ///     ip_version INT,
    ///     shared INT,
    ///     app_id INT,
    ///     platform_id INT,
    ///     asn_tag VARCHAR(20),
    ///     asn_label VARCHAR(128),
    ///     asn_route VARCHAR(50),
    ///     asn_entity_id INT
    /// );
    /// </code>
    /// </remarks>
    public class PostgresNetifyTagProvider : IHostDataProvider<NetifyApplication>
    {
        static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly NpgsqlConnection _connection;
        private readonly string _applicationsTableName;
        private readonly string _addressesTableName;

        /// <summary>
        /// Creates the object using the provided connection string. An format of the string is:
        /// <para/>
        /// "Server=localhost;Port=5432;Database=mydatabase;User Id=myusername;Password=mypassword;"
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public static PostgresNetifyTagProvider Create(string connectionString, string tableName)
        {
            try
            {
                var connection = new NpgsqlConnection(connectionString);
                connection.Open();
                if (connection.State != System.Data.ConnectionState.Open)
                {
                    throw new InvalidOperationException($"Cannot open connection to the database: connectionString={connectionString}.");
                }

                var cmd = connection.CreateCommand();
                cmd.CommandText = $"SELECT COUNT(*) FROM {tableName}";
                var rowCount = cmd.ExecuteScalar();
                _logger.Info($"Postgres connected '{connectionString}'. Available {rowCount} records in table '{tableName}'.");

                return new PostgresNetifyTagProvider(connection, tableName);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Cannot create {nameof(PostgresFlowTagProvider)}: {ex.Message}. {ex.InnerException?.Message}");
                return null;
            }            
        }

        /// <summary>
        /// Creates the new object base on the provided connection string.
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="tableName">The name of the table to read records from.</param>
        PostgresNetifyTagProvider(NpgsqlConnection connection, string tableName)
        {
            _connection = connection;
            _applicationsTableName = tableName;
        }

        /// <summary>
        /// Retrieves a collection of FlowTag objects from the database for the specified host and time range.
        /// </summary>
        /// <param name="host">The host name for which to retrieve the tags.</param>
        /// <param name="start">The start time of the time range.</param>
        /// <param name="end">The end time of the time range.</param>
        /// <returns>An IEnumerable of FlowTag objects representing the tags associated with the specified host and time range.</returns>
        public async Task<IEnumerable<NetifyApplication>> GetAsync(string host, DateTime start, DateTime end)
        {
            try
            {
                var cmd = PrepareSelectCommand(host);
                var reader = await cmd.ExecuteReaderAsync();
                var rowList = new List<NetifyApplication>();
                while (await reader.ReadAsync())
                {
                    var row = ReadNetifyApplication(reader);
                    rowList.Add(row);
                }
                await reader.CloseAsync();
                _logger.Debug($"Query {cmd.CommandText} returned {rowList.Count} rows.");
                return rowList;
            }
            catch (Exception e)
            {
                _logger.Error(e);
                return Array.Empty<NetifyApplication>();
            }
        }

        private NpgsqlCommand PrepareSelectCommand(string hostAddress)
        {
            var cmd = _connection.CreateCommand();
            // SELECT ips.value, app.tag, app.short_name, app.full_name, app.description, app.url, app.category 
            // FROM netify_addresses ips JOIN applicationsTableName app ON ips.app_id = app.id WHERE ips.value = '2.5.34.2';
            cmd.CommandText = $"SELECT ips.value, app.tag, app.short_name, app.full_name, app.description, app.url, app.category FROM {_addressesTableName} ips JOIN {_applicationsTableName} app ON ips.app_id = app.id WHERE ips.value='{hostAddress}'";
            return cmd;
        }

        /// <summary>
        /// Retrieves a collection of <see cref="NetifyApplication"/> objects from the database for the given host.
        /// </summary>
        /// <param name="hostAddress">The host ip address for which to retrieve the tags.</param>
        /// <param name="start">The start time of the time range.</param>
        /// <param name="end">The end time of the time range.</param>
        /// <returns>An IEnumerable of FlowTag objects representing the tags associated with the specified host and time range.</returns>
        public IEnumerable<NetifyApplication> Get(string host, DateTime start, DateTime end)
        {
            try
            {
                var addressSelectCmd = PrepareSelectCommand(host);
                var reader = addressSelectCmd.ExecuteReader();
                var addressList = new List<NetifyApplication>();
                while (reader.Read())
                {
                    var row = ReadNetifyApplication(reader);
                    addressList.Add(row);
                }
                reader.Close();
                _logger.Debug($"Query {addressSelectCmd.CommandText} returned {addressList.Count} rows.");
                return addressList;    
            }
            catch(Exception e)
            {
                _logger.Error(e);
                return Array.Empty<NetifyApplication>();
            }
        }
        /// <summary>
        /// Reads a single row of flow data from the specified NpgsqlDataReader and returns it as a NetifyApplication object.
        /// </summary>
        /// <param name="start">The start time of the flow data.</param>
        /// <param name="end">The end time of the flow data.</param>
        /// <param name="reader">The NpgsqlDataReader to read the flow data from.</param>
        /// <returns>A NetifyApplication object containing the flow tag data from the current row of the NpgsqlDataReader.</returns>
        private static NetifyApplication ReadNetifyApplication(NpgsqlDataReader reader)
        {
                return new NetifyApplication
                {
                    Tag = reader.GetString(reader.GetOrdinal("tag")),
                    ShortName = reader.GetString(reader.GetOrdinal("short_name")),
                    FullName = reader.GetString(reader.GetOrdinal("full_name")),
                    Description = reader.GetString(reader.GetOrdinal("description")),
                    Url = reader.GetString(reader.GetOrdinal("url")),
                    Category = reader.GetString(reader.GetOrdinal("category"))
                };
        }

        /// <summary>
        /// Creates new Netify tables in the database if they do not alrady exist.
        /// </summary>
        /// <param name="applicationsTableName">The name of the netify application table to create.</param>
        /// <param name="ipsTableName">The name of the netify address table to create.</param>
        /// <returns>True if the table exsists or was created.</returns>
        public static bool CreateTablesIfNotExists(NpgsqlConnection connection, string applicationsTableName, string ipsTableName)
        {
            var sucess = true;
            var testApplicationTableExistCmd = connection.CreateCommand();
            testApplicationTableExistCmd.CommandText = $@"SELECT EXISTS(SELECT FROM information_schema.tables WHERE table_name = '{applicationsTableName}');";
            var applicationTableExists = (bool)testApplicationTableExistCmd.ExecuteScalar();

            if (!applicationTableExists)
            {
                string sqlCreateTable = @$"
                CREATE TABLE {applicationsTableName} (
                    id INT PRIMARY KEY,
                    tag VARCHAR(100),
                    short_name VARCHAR(50),
                    full_name VARCHAR(100),
                    description TEXT,
                    url VARCHAR(255),
                    category VARCHAR(50)
                );";
                var createCmd = connection.CreateCommand();
                createCmd.CommandText = sqlCreateTable;
                sucess &= createCmd.ExecuteNonQuery() > 0;
            }

            var testIpsTableExistCmd = connection.CreateCommand();
            testIpsTableExistCmd.CommandText = $@"SELECT EXISTS(SELECT FROM information_schema.tables WHERE table_name = '{ipsTableName}');";
            var ipsTableExists = (bool)testIpsTableExistCmd.ExecuteScalar();

            if (!ipsTableExists)
            {
                string sqlCreateTable = @$"
                CREATE TABLE {ipsTableName} (
                    id INT PRIMARY KEY,
                    value VARCHAR(50),
                    ip_version INT,
                    shared INT,
                    app_id INT,
                    platform_id INT,
                    asn_tag VARCHAR(20),
                    asn_label VARCHAR(128),
                    asn_route VARCHAR(50),
                    asn_entity_id INT
                );";
                var createCmd = connection.CreateCommand();
                createCmd.CommandText = sqlCreateTable;
                sucess &= createCmd.ExecuteNonQuery() > 0;

                string sqlCreateValueIndex = @$"CREATE INDEX ips_value_idx ON {ipsTableName} (value)";
                var createValueIndexCmd = connection.CreateCommand();
                createValueIndexCmd.CommandText = sqlCreateValueIndex;
                sucess &= createValueIndexCmd.ExecuteNonQuery() > 0;
                
                string sqlCreateAppIdIndex = @$"CREATE INDEX ips_app_id_idx ON netify_addresses (app_id)";
                var createAppIndexCmd = connection.CreateCommand();
                createAppIndexCmd.CommandText = sqlCreateAppIdIndex;
                sucess &= createAppIndexCmd.ExecuteNonQuery() > 0;
            }
            return sucess;
        }
    }

    /// <summary>
    /// Represents a web application record.
    /// </summary>
    public record NetifyApplication
    {
        /// <summary>
        /// The tag or label associated with the application.
        /// </summary>
        public string Tag { get; init; }

        /// <summary>
        /// A short name or abbreviation for the application.
        /// </summary>
        public string ShortName { get; init; }

        /// <summary>
        /// The full name or title of the application.
        /// </summary>
        public string FullName { get; init; }

        /// <summary>
        /// A longer description or summary of the application.
        /// </summary>
        public string Description { get; init; }

        /// <summary>
        /// The URL or web address for the application.
        /// </summary>
        public string Url { get; init; }

        /// <summary>
        /// The category or type of the application.
        /// </summary>
        public string Category { get; init; }
    }

    /// <summary>
    /// Represents a network address with additional metadata.
    /// </summary>
    public record NetifyAddress
    {
        /// <summary>
        /// The unique identifier for the address.
        /// </summary>
        public int Id { get; init; }

        /// <summary>
        /// The value of the network address (e.g. IP address or network prefix).
        /// </summary>
        public IPAddress Value { get; init; }

        /// <summary>
        /// Indicates whether the address is shared or dedicated.
        /// </summary>
        public int Shared { get; init; }

        /// <summary>
        /// The ID of the web application associated with the address.
        /// </summary>
        public int AppId { get; init; }
    }
}
