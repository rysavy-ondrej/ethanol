using CsvHelper.Configuration;
using CsvHelper;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.ConstrainedExecution;
using System.Threading.Tasks;
using static Ethanol.ContextBuilder.ProgramCommands;
using CsvHelper.Configuration.Attributes;
using System.Reflection.Metadata.Ecma335;
using System.Text.Json;
using System.Linq;
using static Ethanol.ContextBuilder.Enrichers.PostgresHostTagProvider;

namespace Ethanol.ContextBuilder.Enrichers
{
    /// <summary>
    /// Represents a HostTag data source stored in PostgreSQL database.
    /// </summary>
    /// <remarks>
    /// It is expected that the table has the following structure:
    /// <code>
    /// CREATE TABLE hosttags (
    ///   KeyType VARCHAR(8),
    ///   KeyValue VARCHAR(32),
    ///   Source VARCHAR(40),
    ///   Reliability REAL,
    ///   Module VARCHAR(40),
    ///   Data JSON,
    ///   Validity TSRANGE
    /// );
    /// </code>
    /// </remarks>
    public class PostgresHostTagProvider : IHostDataProvider<HostTag>
    {
        static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly NpgsqlConnection _connection;
        private readonly string _tableName;

        /// <summary>
        /// Creates the object using the provided connection string. An format of the string is:
        /// <para/>
        /// "Server=localhost;Port=5432;Database=mydatabase;User Id=myusername;Password=mypassword;"
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public static PostgresHostTagProvider Create(string connectionString, string tableName)
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

                return new PostgresHostTagProvider(connection, tableName);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Cannot create {nameof(PostgresHostTagProvider)}: {ex.Message}. {ex.InnerException?.Message}");
                return null;
            }
        }

        /// <summary>
        /// Creates the new object base on the provided connection string.
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="tableName">The name of the table to read records from.</param>
        PostgresHostTagProvider(NpgsqlConnection connection, string tableName)
        {
            _connection = connection;
            _tableName = tableName;
        }

        /// <summary>
        /// Retrieves a collection of HostTag objects from the database for the specified host and time range.
        /// </summary>
        /// <param name="host">The host name for which to retrieve the tags.</param>
        /// <param name="start">The start time of the time range.</param>
        /// <param name="end">The end time of the time range.</param>
        /// <returns>An IEnumerable of HostTag objects representing the tags associated with the specified host and time range.</returns>
        public async Task<IEnumerable<HostTag>> GetAsync(string host, DateTime start, DateTime end)
        {
            try
            {
                NpgsqlCommand cmd = PrepareCommand(host, start, end);
                var reader = await cmd.ExecuteReaderAsync();
                var rowList = new List<HostTag>();
                while (await reader.ReadAsync())
                {
                    var row = new HostTag(start, end,
                                          reader["KeyValue"] as string,
                                          reader["Source"] as string,
                                          reader["Reliability"] as double? ?? 1.0,
                                          reader["Data"] as string);
                    rowList.Add(row);
                }
                await reader.CloseAsync();
                _logger.Debug($"Query {cmd.CommandText} returned {rowList.Count} rows.");
                return rowList;
            }
            catch (Exception e)
            {
                _logger.Error(e);
                return Array.Empty<HostTag>();
            }
        }
        /// <summary>
        /// Retrieves a collection of HostTag objects from the database for the specified host and time range.
        /// </summary>
        /// <param name="host">The host name for which to retrieve the tags.</param>
        /// <param name="start">The start time of the time range.</param>
        /// <param name="end">The end time of the time range.</param>
        /// <returns>An IEnumerable of HostTag objects representing the tags associated with the specified host and time range.</returns>
        public IEnumerable<HostTag> Get(string host, DateTime start, DateTime end)
        {
            try
            {
                NpgsqlCommand cmd = PrepareCommand(host, start, end);
                var reader = cmd.ExecuteReader();
                var rowList = new List<HostTag>();
                while (reader.Read())
                {
                    HostTag row = ReadHostTag(start, end, reader);
                    rowList.Add(row);
                }
                reader.Close();
                _logger.Debug($"Query {cmd.CommandText} returned {rowList.Count} rows.");
                return rowList;
            }
            catch (Exception e)
            {
                _logger.Error(e);
                return Array.Empty<HostTag>();
            }
        }

        private static HostTag ReadHostTag(DateTime start, DateTime end, NpgsqlDataReader reader)
        {
            return new HostTag(start, end,
                                  reader["KeyValue"] as string,
                                  reader["Source"] as string,
                                  TryConvert.ToFloat(reader["Reliability"], out var reliability) ? reliability : 1.0,
                                  reader["Data"] as string);
        }

        private NpgsqlCommand PrepareCommand(string host, DateTime start, DateTime end)
        {
            var startString = start.ToString("o", CultureInfo.InvariantCulture);
            var endString = end.ToString("o", CultureInfo.InvariantCulture);
            var cmd = _connection.CreateCommand();
            // SELECT * FROM smartads WHERE Host = '192.168.1.32' AND Validity @> '[2022-06-01T14:00:00,2022-06-01T14:05:00)';
            cmd.CommandText = $"SELECT * FROM {_tableName} WHERE KeyValue ='{host}' AND Validity && '[{startString},{endString})'";
            return cmd;
        }

        /// <summary>
        /// Creates a new table for storing <see cref="FlowTag"/> records in the database if it does not alrady exist.
        /// </summary>
        /// <param name="tableName">The name of the table to create.</param>
        /// <returns>True if the table exsists or was created.</returns>
        public static bool CreateTableIfNotExists(NpgsqlConnection connection, string tableName)
        {
            var testCmd = connection.CreateCommand();
            testCmd.CommandText = $@"SELECT EXISTS(SELECT FROM information_schema.tables WHERE table_name = '{tableName}');";
            var exists = (bool)testCmd.ExecuteScalar();

            if (!exists)
            {
                string sqlCreateTable = @$"
                CREATE TABLE {tableName}(
                    KeyType VARCHAR(16),
                    KeyValue VARCHAR(64),
                    Source VARCHAR(80),
                    Reliability REAL,
                    Module VARCHAR(80),
                    Data JSON,
                    Validity TSRANGE
                );";
                var createCmd = connection.CreateCommand();
                createCmd.CommandText = sqlCreateTable;
                return createCmd.ExecuteNonQuery() > 0;
            }
            else
            {
                return true;
            }
        }

        internal static int BulkInsert(NpgsqlConnection connection, string inputFile, string tableName)
        {
            var recordCount = 0;
            using (var reader = new StreamReader(inputFile))
            {

                var records = ParseCsv(reader).ToList();

                var sample = records[1282];

                using (var writer = connection.BeginBinaryImport($"COPY {tableName} (keytype, keyvalue, source, reliability, module, data, validity) FROM STDIN (FORMAT BINARY)"))
                {
                    foreach (var record in records)
                    {
                        recordCount++;

                        var json = JsonSerializer.Deserialize<dynamic>(record.Data);
                        var jsonText = json.ToString();

                        writer.StartRow();
                        writer.Write(record.KeyType, NpgsqlTypes.NpgsqlDbType.Text);
                        writer.Write(record.KeyValue, NpgsqlTypes.NpgsqlDbType.Text);
                        writer.Write(record.Source, NpgsqlTypes.NpgsqlDbType.Text);
                        writer.Write(record.Reliability, NpgsqlTypes.NpgsqlDbType.Real);
                        writer.Write(record.Module, NpgsqlTypes.NpgsqlDbType.Text);
                        writer.Write(json, NpgsqlTypes.NpgsqlDbType.Json);
                        writer.Write(new NpgsqlTypes.NpgsqlRange<DateTime>(record.StartTime, record.EndTime), NpgsqlTypes.NpgsqlDbType.TimestampRange);
                    }

                    writer.Complete();
                }
            }
            return recordCount;
        }
        internal record CsvHostTagRecord(
            [Index(0)] string KeyType,
            [Index(1)] string KeyValue,
            [Index(2)] string Source,
            [Index(3)] DateTime StartTime,
            [Index(4)] DateTime EndTime,
            [Index(5)] double Reliability,
            [Index(6)] string Module,
            [Index(7)] string Data
        );

        /// <summary>
        /// Parses the input data using custom CSV parser. The custom parser is needed as source CSV is not properly quoted.
        /// </summary>
        /// <param name="reader">The source reader.</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        static IEnumerable<CsvHostTagRecord> ParseCsv(TextReader reader)
        {
            string line;

            while ((line = reader.ReadLine()) != null)
            {
                var parts = line.Split(',', 8);
                // shorter lines are silently ignored...
                if (parts.Length != 8)
                {
                    _logger.Warn($"Invalid line: {line}");
                   continue;
                }

                var record = new CsvHostTagRecord(
                    KeyType: parts[0],
                    KeyValue: parts[1],
                    Source: parts[2],
                    StartTime: DateTime.Parse(parts[3]),
                    EndTime: DateTime.Parse(parts[4]),
                    Reliability: double.Parse(parts[5]),
                    Module: parts[6],
                    Data: parts[7]
                );

                yield return record;
            }
        }
    }
}
