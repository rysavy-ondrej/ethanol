using CsvHelper.Configuration;
using CsvHelper;
using Npgsql;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using CsvHelper.Configuration.Attributes;
using CsvHelper.TypeConversion;

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
    ///     ip_version VARCHAR(10),
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
        private readonly string _domainTableName;

        /// <summary>
        /// Creates the object using the provided connection string. An format of the string is:
        /// <para/>
        /// "Server=localhost;Port=5432;Database=mydatabase;User Id=myusername;Password=mypassword;"
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public static PostgresNetifyTagProvider Create(string connectionString, string tablePrefix)
        {
            var applicationsTableName = tablePrefix + "_applications";
            var addressesTableName = tablePrefix + "_addresses";
            var domainTableName = tablePrefix + "_domains";
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
                return new PostgresNetifyTagProvider(connection, applicationsTableName, addressesTableName, domainTableName);
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
        PostgresNetifyTagProvider(NpgsqlConnection connection, string applicationsTableName, string addressesTableName, string domainTableName)
        {
            _connection = connection;
            _applicationsTableName = applicationsTableName;
            _addressesTableName = addressesTableName;
            this._domainTableName = domainTableName;
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
            catch (Exception e)
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
        /// <param name="dropTables">If set to true, the tables will be dropped and recreated.</param>
        public static void CreateTables(NpgsqlConnection connection, string tablePrefix, bool dropTables = false)
        {
            var applicationsTableName = tablePrefix + "_applications";
            var addressesTableName = tablePrefix + "_addresses";
            var domainsTableName = tablePrefix + "_domains";

            if (dropTables)
            {
                connection.DropTable(applicationsTableName);
                connection.DropTable(addressesTableName);
                connection.DropTable(domainsTableName);
            }

            connection.CreateTable(applicationsTableName,
                "id INT PRIMARY KEY",
                "tag VARCHAR(100)",
                "short_name VARCHAR(50)",
                "full_name VARCHAR(100)",
                "description TEXT",
                "url VARCHAR(255)",
                "category VARCHAR(50)");

            connection.CreateTable(addressesTableName,
                    "id INT PRIMARY KEY",
                    "value VARCHAR(50)",
                    "ip_version VARCHAR(10)",
                    "shared INT",
                    "app_id INT",
                    "platform_id VARCHAR(20)",
                    "asn_tag VARCHAR(20)",
                    "asn_label VARCHAR(128)",
                    "asn_route VARCHAR(50)",
                    "asn_entity_id INT"
                );
            connection.CreateIndex(addressesTableName, "value");
            connection.CreateIndex(addressesTableName, "app_id");

            connection.CreateTable(domainsTableName,
                    "id INT PRIMARY KEY",
                    "value VARCHAR(128)",
                    "app_id INT",
                    "platform_id  VARCHAR(20)"
                );
            connection.CreateIndex(domainsTableName, "value");
        }

        public class EmptyStringToIntConverter : Int32Converter
        {
            public override object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
            {
                if (string.IsNullOrWhiteSpace(text) || String.Equals(text, "none", StringComparison.InvariantCultureIgnoreCase))
                {
                    return 0;
                }
                return base.ConvertFromString(text, row, memberMapData);
            }
        }


        public class NetifyAppRecord
        {
            [Name("id")]
            public int Id { get; set; }

            [Name("tag")]
            public string Tag { get; set; }

            [Name("short_name")]
            public string ShortName { get; set; }

            [Name("full_name")]
            public string FullName { get; set; }

            [Name("description")]
            public string Description { get; set; }

            [Name("url")]
            public string Url { get; set; }

            [Name("category")]
            public string Category { get; set; }

            internal static void LoadAndStore(NpgsqlConnection connection, string appsFile, string tableName)
            {
                // Load from Apps
                var reader = new StreamReader(appsFile);
                var config = new CsvConfiguration(System.Globalization.CultureInfo.InvariantCulture) { Delimiter = ";", BadDataFound = null };
                var csv = new CsvReader(reader, config);
                var appsList = csv.GetRecords<NetifyAppRecord>();

                foreach (var app in appsList)
                {
                    var insertCommand = new NpgsqlCommand(
                        $@"INSERT INTO {tableName} (id, tag, short_name, full_name, description, url, category) 
                          VALUES (@id, @tag, @short_name, @full_name, @description, @url, @category)", connection);

                    insertCommand.Parameters.AddWithValue("@id", app.Id);
                    insertCommand.Parameters.AddWithValue("@tag", app.Tag ?? (object)DBNull.Value); // If app.Tag can be null
                    insertCommand.Parameters.AddWithValue("@short_name", app.ShortName ?? (object)DBNull.Value);
                    insertCommand.Parameters.AddWithValue("@full_name", app.FullName ?? (object)DBNull.Value);
                    insertCommand.Parameters.AddWithValue("@description", app.Description ?? (object)DBNull.Value);
                    insertCommand.Parameters.AddWithValue("@url", app.Url ?? (object)DBNull.Value);
                    insertCommand.Parameters.AddWithValue("@category", app.Category ?? (object)DBNull.Value);

                    insertCommand.ExecuteNonQuery();
                }
            }
        }
        internal static int ApplicationsBulkInsert(NpgsqlConnection connection, string inputFile, string tableName)
        {
            var recordCount = 0;
            using (var reader = new StreamReader(inputFile))
            {
                var config = new CsvConfiguration(System.Globalization.CultureInfo.InvariantCulture) { Delimiter = ";", BadDataFound = null };
                var csv = new CsvReader(reader, config);
                var appsList = csv.GetRecords<NetifyAppRecord>();

                using (var writer = connection.BeginBinaryImport($"COPY {tableName} (id, tag, short_name, full_name, description, url, category) FROM STDIN (FORMAT BINARY)"))
                {
                    foreach (var app in appsList)
                    {
                        recordCount++;

                        writer.StartRow();
                        writer.Write(app.Id, NpgsqlTypes.NpgsqlDbType.Integer);
                        writer.Write(app.Tag, NpgsqlTypes.NpgsqlDbType.Text);
                        writer.Write(app.ShortName, NpgsqlTypes.NpgsqlDbType.Text);
                        writer.Write(app.FullName, NpgsqlTypes.NpgsqlDbType.Text);
                        writer.Write(app.Description, NpgsqlTypes.NpgsqlDbType.Text);
                        writer.Write(app.Url, NpgsqlTypes.NpgsqlDbType.Text);
                        writer.Write(app.Category, NpgsqlTypes.NpgsqlDbType.Text);
                    }

                    writer.Complete();
                }
            }
            return recordCount;
        }

        public class NetifyIpsRecord
        {
            [Name("id")]
            public int Id { get; set; }

            [Name("value")]
            public string Value { get; set; }

            [Name("ip_version")]
            public string IpVersion { get; set; }

            [Name("shared")]
            [TypeConverter(typeof(EmptyStringToIntConverter))]
            public int Shared { get; set; }

            [Name("app_id")]
            public int AppId { get; set; }

            [Name("platform_id")]
            public string PlatformId { get; set; }

            [Name("asn_tag")]
            public string AsnTag { get; set; }

            [Name("asn_label")]
            public string AsnLabel { get; set; }

            internal static void LoadAndStore(NpgsqlConnection connection, string appsFile, string tableName)
            {
                // Load from Apps
                using (var reader = new StreamReader(appsFile))
                {
                    var config = new CsvConfiguration(System.Globalization.CultureInfo.InvariantCulture) { Delimiter = ";", BadDataFound = null };
                    var csv = new CsvReader(reader, config);
                    var appsList = csv.GetRecords<NetifyIpsRecord>();

                    foreach (var app in appsList)
                    {
                        var insertCommand = new NpgsqlCommand(
                            $@"INSERT INTO {tableName} (id, value, ip_version, shared, app_id, platform_id, asn_tag, asn_label) 
                      VALUES (@id, @value, @ip_version, @shared, @app_id, @platform_id, @asn_tag, @asn_label)", connection);

                        insertCommand.Parameters.AddWithValue("@id", app.Id);
                        insertCommand.Parameters.AddWithValue("@value", app.Value ?? (object)DBNull.Value);
                        insertCommand.Parameters.AddWithValue("@ip_version", app.IpVersion ?? (object)DBNull.Value);
                        insertCommand.Parameters.AddWithValue("@shared", app.Shared);
                        insertCommand.Parameters.AddWithValue("@app_id", app.AppId);
                        insertCommand.Parameters.AddWithValue("@platform_id", app.PlatformId);
                        insertCommand.Parameters.AddWithValue("@asn_tag", app.AsnTag ?? (object)DBNull.Value);
                        insertCommand.Parameters.AddWithValue("@asn_label", app.AsnLabel ?? (object)DBNull.Value);

                        insertCommand.ExecuteNonQuery();
                    }
                }
            }
        }
        internal static int AddressesBulkInsert(NpgsqlConnection connection, string inputFile, string tableName)
        {
            var recordCount = 0;
            using (var reader = new StreamReader(inputFile))
            {
                var config = new CsvConfiguration(System.Globalization.CultureInfo.InvariantCulture) { Delimiter = ";", BadDataFound = null };
                var csv = new CsvReader(reader, config);
                var appsList = csv.GetRecords<NetifyIpsRecord>();

                using (var writer = connection.BeginBinaryImport($"COPY {tableName}  (id, value, ip_version, shared, app_id, platform_id, asn_tag, asn_label) FROM STDIN (FORMAT BINARY)"))
                {
                    foreach (var app in appsList)
                    {
                        recordCount++;

                        writer.StartRow();
                        writer.Write(app.Id, NpgsqlTypes.NpgsqlDbType.Integer);
                        writer.Write(app.Value, NpgsqlTypes.NpgsqlDbType.Text);
                        writer.Write(app.IpVersion, NpgsqlTypes.NpgsqlDbType.Text);
                        writer.Write(app.Shared, NpgsqlTypes.NpgsqlDbType.Integer);
                        writer.Write(app.AppId, NpgsqlTypes.NpgsqlDbType.Integer);
                        writer.Write(app.PlatformId, NpgsqlTypes.NpgsqlDbType.Text);
                        writer.Write(app.AsnTag, NpgsqlTypes.NpgsqlDbType.Text);
                        writer.Write(app.AsnLabel, NpgsqlTypes.NpgsqlDbType.Text);
                    }

                    writer.Complete();
                }
            }
            return recordCount;
        }


        public class NetifyDomainRecord
        {
            [Name("id")]
            public int Id { get; set; }

            [Name("value")]
            public string Value { get; set; }

            [Name("app_id")]
            public int AppId { get; set; }

            [Name("platform_id")]
            public string PlatformId { get; set; }

            internal static void LoadAndStore(NpgsqlConnection connection, string inputFile, string tableName)
            {
                // Load from Apps
                using (var reader = new StreamReader(inputFile))
                {
                    var config = new CsvConfiguration(System.Globalization.CultureInfo.InvariantCulture) { Delimiter = ";", BadDataFound = null };
                    var csv = new CsvReader(reader, config);
                    var appsList = csv.GetRecords<NetifyDomainRecord>();

                    foreach (var app in appsList)
                    {
                        var insertCommand = new NpgsqlCommand(
                            $@"INSERT INTO {tableName} (id, value, app_id, platform_id) 
                      VALUES (@id, @value, @app_id, @platform_id)", connection);

                        insertCommand.Parameters.AddWithValue("@id", app.Id);
                        insertCommand.Parameters.AddWithValue("@value", app.Value ?? (object)DBNull.Value);
                        insertCommand.Parameters.AddWithValue("@app_id", app.AppId);
                        insertCommand.Parameters.AddWithValue("@platform_id", app.PlatformId);

                        insertCommand.ExecuteNonQuery();
                    }
                }
            }
        }

        public static int DomainsBulkInsert(NpgsqlConnection connection, string inputFile, string tableName)
        {
            var recordCount = 0;
            using (var reader = new StreamReader(inputFile))
            {
                var config = new CsvConfiguration(System.Globalization.CultureInfo.InvariantCulture) { Delimiter = ";", BadDataFound = null };
                var csv = new CsvReader(reader, config);
                var appsList = csv.GetRecords<NetifyDomainRecord>();

                using (var writer = connection.BeginBinaryImport($"COPY {tableName} (id, value, app_id, platform_id) FROM STDIN (FORMAT BINARY)"))
                {
                    foreach (var app in appsList)
                    {
                        recordCount++;

                        writer.StartRow();
                        writer.Write(app.Id, NpgsqlTypes.NpgsqlDbType.Integer);
                        writer.Write(app.Value, NpgsqlTypes.NpgsqlDbType.Text);
                        writer.Write(app.AppId, NpgsqlTypes.NpgsqlDbType.Integer);
                        writer.Write(app.PlatformId, NpgsqlTypes.NpgsqlDbType.Text);
                    }

                    writer.Complete();
                }
            }
            return recordCount;
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
