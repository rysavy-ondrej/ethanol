using Npgsql;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Ethanol.ContextBuilder.Enrichers
{

    public class PostgresTagProvider : ITagDataProvider<TagRecord>
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
        public static ITagDataProvider<TagRecord> Create(string connectionString, string tableName)
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

                return new PostgresTagProvider(connection, tableName);
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
        public async Task<IEnumerable<TagRecord>> GetAsync(string key, DateTime start, DateTime end)
        {
            try
            {
                NpgsqlCommand cmd = PrepareCommand(key, start, end);
                var reader = await cmd.ExecuteReaderAsync();
                var rowList = new List<TagRecord>();
                while (await reader.ReadAsync())
                {
                    var row = TagRecord.Read(reader);
                    rowList.Add(row);
                }
                await reader.CloseAsync();
                _logger.Debug($"Query {cmd.CommandText} returned {rowList.Count} rows.");
                return rowList;
            }
            catch (Exception e)
            {
                _logger.Error(e);
                return Array.Empty<TagRecord>();
            }
        }
        /// <summary>
        /// Retrieves a collection of HostTag objects from the database for the specified host and time range.
        /// </summary>
        /// <param name="tagKey">The host name for which to retrieve the tags.</param>
        /// <param name="start">The start time of the time range.</param>
        /// <param name="end">The end time of the time range.</param>
        /// <returns>An IEnumerable of HostTag objects representing the tags associated with the specified host and time range.</returns>
        public IEnumerable<TagRecord> Get(string tagKey, DateTime start, DateTime end)
        {
            try
            {
                NpgsqlCommand cmd = PrepareCommand(tagKey, start, end);
                var reader = cmd.ExecuteReader();
                var rowList = new List<TagRecord>();
                while (reader.Read())
                {
                    var row = TagRecord.Read(reader);
                    rowList.Add(row);
                }
                reader.Close();
                _logger.Debug($"Query {cmd.CommandText} returned {rowList.Count} rows.");
                return rowList;
            }
            catch (Exception e)
            {
                _logger.Error(e);
                return Array.Empty<TagRecord>();
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
            connection.CreateTable(tableName, TagRecord.SqlColumns.Select(x => $" {x.Item1} {x.Item2}").ToArray());
            connection.CreateIndex(tableName, "key");
            return true;
        }

        public static int BulkInsert(NpgsqlConnection connection, string tableName, IEnumerable<TagRecord> records)
        {

            string Truncate(string input, int maxsize) => input.Substring(0, System.Math.Min(input.Length, maxsize));
            var recordCount = 0;
            using (var writer = connection.BeginBinaryImport($"COPY {tableName} (type, key, value, reliability, validity, details) FROM STDIN (FORMAT BINARY)"))
            {
                foreach (var record in records)
                {
                    recordCount++;
                    writer.StartRow();
                    writer.Write(Truncate(record.Type,TagRecord.TypeLength), NpgsqlTypes.NpgsqlDbType.Text);
                    writer.Write(Truncate(record.Key, TagRecord.KeyLength), NpgsqlTypes.NpgsqlDbType.Text);
                    writer.Write(Truncate(record.Value, TagRecord.ValueLength), NpgsqlTypes.NpgsqlDbType.Text);
                    writer.Write(record.Reliability, NpgsqlTypes.NpgsqlDbType.Real);
                    writer.Write(new NpgsqlTypes.NpgsqlRange<DateTime>(record.StartTime, record.EndTime), NpgsqlTypes.NpgsqlDbType.TimestampRange);
                    writer.Write(record.Details, NpgsqlTypes.NpgsqlDbType.Json);
                }

                writer.Complete();
            }
            return recordCount;
        }
    }
}
