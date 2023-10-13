using Ethanol.ContextBuilder.Context;
using Ethanol.ContextBuilder.Helpers;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Ethanol.ContextBuilder.Enrichers
{
    /// <summary>
    /// Provides a concrete implementation of <see cref="ITagDataProvider{T}"/> for PostgreSQL databases.
    /// This class is responsible for fetching tag data from a PostgreSQL database, allowing for integration
    /// between the application and a relational database system.
    /// </summary>
    /// <remarks>
    /// The class uses Npgsql, a .NET data provider for PostgreSQL, to manage database connections and operations.
    /// </remarks>
    public class PostgresTagProvider : ITagDataProvider<TagObject>
    {
        static ILogger _logger = LogManager.GetCurrentClassLogger();
        private readonly NpgsqlConnection _connection;
        private readonly string _tableName;

        /// <summary>
        /// Creates the object using the provided connection string. An format of the string is:
        /// <para/>
        /// "Server=localhost;Port=5432;Database=mydatabase;User Id=myusername;Password=mypassword;"
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public static ITagDataProvider<TagObject> Create(string connectionString, string tableName)
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
                _logger.LogInformation($"Postgres connected '{connectionString}'. Available {rowCount} records in table '{tableName}'.");

                return new PostgresTagProvider(connection, tableName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Cannot create {nameof(PostgresTagProvider)}: {ex.Message}. {ex.InnerException?.Message}");
                return null;
            }
        }

        /// <summary>
        /// Creates the new object base on the provided connection string.
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="tableName">The name of the table to read records from.</param>
        PostgresTagProvider(NpgsqlConnection connection, string tableName)
        {
            _connection = connection;
            _tableName = tableName;
        }

        /// <summary>
        /// Retrieves a collection of HostTag objects from the database for the specified host and time range.
        /// </summary>
        /// <param name="key">The host name for which to retrieve the tags.</param>
        /// <param name="start">The start time of the time range.</param>
        /// <param name="end">The end time of the time range.</param>
        /// <returns>An IEnumerable of HostTag objects representing the tags associated with the specified host and time range.</returns>
        public async Task<IEnumerable<TagObject>> GetAsync(string key, DateTime start, DateTime end)
        {
            try
            {
                NpgsqlCommand cmd = PrepareCommand(key, start, end);
                var reader = await cmd.ExecuteReaderAsync();
                var rowList = new List<TagObject>();
                while (await reader.ReadAsync())
                {
                    var row = ReadRow(reader);
                    rowList.Add(row);
                }
                await reader.CloseAsync();
                _logger.LogDebug($"Query {cmd.CommandText} returned {rowList.Count} rows.");
                return rowList;
            }
            catch (Exception e)
            {
                _logger.LogError(e, null);
                return Array.Empty<TagObject>();
            }
        }
        /// <summary>
        /// Retrieves a collection of HostTag objects from the database for the specified host and time range.
        /// </summary>
        /// <param name="tagKey">The host name for which to retrieve the tags.</param>
        /// <param name="start">The start time of the time range.</param>
        /// <param name="end">The end time of the time range.</param>
        /// <returns>An IEnumerable of HostTag objects representing the tags associated with the specified host and time range.</returns>
        public IEnumerable<TagObject> Get(string tagKey, DateTime start, DateTime end)
        {
            try
            {
                NpgsqlCommand cmd = PrepareCommand(tagKey, start, end);
                var reader = cmd.ExecuteReader();
                var rowList = new List<TagObject>();
                while (reader.Read())
                {
                    var row = ReadRow(reader);
                    rowList.Add(row);
                }
                reader.Close();
                _logger.LogDebug($"Query {cmd.CommandText} returned {rowList.Count} rows.");
                return rowList;
            }
            catch (Exception e)
            {
                _logger.LogError(e, null);
                return Array.Empty<TagObject>();
            }
        }

        private NpgsqlCommand PrepareCommand(string tagKey, DateTime start, DateTime end)
        {
            var startString = start.ToString("o", CultureInfo.InvariantCulture);
            var endString = end.ToString("o", CultureInfo.InvariantCulture);
            var cmd = _connection.CreateCommand();
            // SELECT * FROM smartads WHERE Host = '192.168.1.32' AND Validity @> '[2022-06-01T14:00:00,2022-06-01T14:05:00)';
            cmd.CommandText = $"SELECT * FROM {_tableName} WHERE key ='{tagKey}' AND validity && '[{startString},{endString})'";
            return cmd;
        }

        /// <summary>
        /// Creates a new table for storing <see cref="FlowTag"/> records in the database if it does not alrady exist.
        /// </summary>
        /// <param name="tableName">The name of the table to create.</param>
        /// <returns>True if the table exsists or was created.</returns>
        public static bool CreateTableIfNotExists(NpgsqlConnection connection, string tableName)
        {
            connection.CreateTable(tableName, SqlColumns.Select(x => $" {x.Item1} {x.Item2}").ToArray());
            connection.CreateIndex(tableName, "type");
            connection.CreateIndex(tableName, "key");
            return true;
        }

        public static int BulkInsert(NpgsqlConnection connection, string tableName, IEnumerable<TagObject> records)
        {

            string Truncate(string input, int maxsize) => input.Substring(0, System.Math.Min(input.Length, maxsize));
            var recordCount = 0;
            using (var writer = connection.BeginBinaryImport($"COPY {tableName} (type, key, value, reliability, validity, details) FROM STDIN (FORMAT BINARY)"))
            {
                foreach (var record in records)
                {
                    recordCount++;
                    writer.StartRow();
                    writer.Write(Truncate(record.Type, ColumnTypeLength), NpgsqlTypes.NpgsqlDbType.Text);
                    writer.Write(Truncate(record.Key, ColumnKeyLength), NpgsqlTypes.NpgsqlDbType.Text);
                    writer.Write(Truncate(record.Value, ColumnValueLength), NpgsqlTypes.NpgsqlDbType.Text);
                    writer.Write(record.Reliability, NpgsqlTypes.NpgsqlDbType.Real);
                    writer.Write(new NpgsqlTypes.NpgsqlRange<DateTime>(record.StartTime, record.EndTime), NpgsqlTypes.NpgsqlDbType.TimestampRange);
                    writer.Write(record.Details, NpgsqlTypes.NpgsqlDbType.Json);
                }

                writer.Complete();
            }
            return recordCount;
        }

        /// <summary>
        /// Represents the SQL columns and their types for the TagRecord.
        /// </summary>
        static (string, string)[] SqlColumns =>
            new (string, string)[]
            {
            ("type", $"VARCHAR({ColumnTypeLength}) NOT NULL"),
            ("key", $"VARCHAR({ColumnKeyLength}) NOT NULL"),
            ("value", $"VARCHAR({ColumnValueLength})"),
            ("reliability", "REAL"),
            ("validity", "TSRANGE"),
            ("details", "JSON")
            };
        static int ColumnTypeLength = 32;
        static int ColumnKeyLength = 64;
        static int ColumnValueLength = 128;

        /// <summary>
        /// Reads a TagRecord from the provided NpgsqlDataReader.
        /// </summary>
        /// <param name="reader">The NpgsqlDataReader containing the tag data.</param>
        /// <returns>The TagRecord extracted from the reader.</returns>
        static TagObject ReadRow(NpgsqlDataReader reader)
        {
            var validity = reader.GetFieldValue<NpgsqlRange<DateTime>>("validity");
            return new TagObject
            {
                Type = reader.GetString("type"),
                Key = reader.GetString("key"),
                Value = reader.GetString("value"),
                Reliability = reader.GetFloat("reliability"),
                StartTime = validity.LowerBound,
                EndTime = validity.UpperBound,
                Details = JsonSerializer.Deserialize<ExpandoObject>(reader.GetString("details"))
            };
        }
    }
}
