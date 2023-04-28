using Npgsql;
using System;
using System.Collections.Generic;
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
    public class PostgresNetifyTagProvider : IHostDataProvider<NetifyTag>
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
        public static PostgresNetifyTagProvider Create(string connectionString, string applicationsTableName, string addressesTableName)
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
                cmd.CommandText = $"SELECT COUNT(*) FROM {applicationsTableName}";
                var appsRowCount = cmd.ExecuteScalar();
                cmd.CommandText = $"SELECT COUNT(*) FROM {addressesTableName}";
                var ipsRowCount = cmd.ExecuteScalar();
                _logger.Info($"Postgres connected '{connectionString}'.");
                _logger.Info($"Available {appsRowCount} records in table '{applicationsTableName}'. ");
                _logger.Info($"Available {ipsRowCount} records in table '{addressesTableName}'. ");
                return new PostgresNetifyTagProvider(connection, applicationsTableName, addressesTableName);
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
        /// <param name="applicationsTableName">The name of the table to read application records from.</param>
        /// <param name="addressesTableName">The name of the table to read address records from.</param>
        PostgresNetifyTagProvider(NpgsqlConnection connection, string applicationsTableName, string addressesTableName)
        {
            _connection = connection;
            _applicationsTableName = applicationsTableName;
            _addressesTableName = addressesTableName;
        }

        /// <summary>
        /// Retrieves a collection of FlowTag objects from the database for the specified host and time range.
        /// </summary>
        /// <param name="host">The host name for which to retrieve the tags.</param>
        /// <param name="start">The start time of the time range.</param>
        /// <param name="end">The end time of the time range.</param>
        /// <returns>An IEnumerable of FlowTag objects representing the tags associated with the specified host and time range.</returns>
        public async Task<IEnumerable<NetifyTag>> GetAsync(string host, DateTime start, DateTime end)
        {
            try
            {
                var cmd = PrepareSelectCommand(host);
                var reader = await cmd.ExecuteReaderAsync();
                var rowList = new List<NetifyTag>();
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
                return Array.Empty<NetifyTag>();
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
        /// Retrieves a collection of <see cref="NetifyTag"/> objects from the database for the given host.
        /// </summary>
        /// <param name="hostAddress">The host ip address for which to retrieve the tags.</param>
        /// <param name="start">The start time of the time range.</param>
        /// <param name="end">The end time of the time range.</param>
        /// <returns>An IEnumerable of FlowTag objects representing the tags associated with the specified host and time range.</returns>
        public IEnumerable<NetifyTag> Get(string host, DateTime start, DateTime end)
        {
            try
            {
                var addressSelectCmd = PrepareSelectCommand(host);
                var reader = addressSelectCmd.ExecuteReader();
                var addressList = new List<NetifyTag>();
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
                return Array.Empty<NetifyTag>();
            }
        }
        /// <summary>
        /// Reads a single row of flow data from the specified NpgsqlDataReader and returns it as a NetifyApplication object.
        /// </summary>
        /// <param name="start">The start time of the flow data.</param>
        /// <param name="end">The end time of the flow data.</param>
        /// <param name="reader">The NpgsqlDataReader to read the flow data from.</param>
        /// <returns>A NetifyApplication object containing the flow tag data from the current row of the NpgsqlDataReader.</returns>
        private static NetifyTag ReadNetifyApplication(NpgsqlDataReader reader)
        {
                return new NetifyTag
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
                
                string sqlCreateAppIdIndex = @$"CREATE INDEX ips_app_id_idx ON {ipsTableName} (app_id)";
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
    public record NetifyTag
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
}
