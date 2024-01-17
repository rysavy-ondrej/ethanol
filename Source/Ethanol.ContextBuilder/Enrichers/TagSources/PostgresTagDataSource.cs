using Ethanol.DataObjects;
using Ethanol.ContextBuilder.Enrichers;
using Ethanol.ContextBuilder.Enrichers.TagObjects;
using Ethanol.ContextBuilder.Helpers;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;
using System.Dynamic;
/// <summary>
/// Provides a concrete implementation of <see cref="ITagDataSource{T}"/> for PostgreSQL databases.
/// This class is responsible for fetching tag data from a PostgreSQL database, allowing for integration
/// between the application and a relational database system.
/// </summary>
/// <remarks>
/// The class uses Npgsql, a .NET data provider for PostgreSQL, to manage database connections and operations.
/// </remarks>
public class PostgresTagDataSource : ITagDataSource<TagObject>
{
    private readonly ILogger? _logger;
    private readonly NpgsqlConnection _connection;
    private readonly string _tableName;
    private readonly TimeSpan _reconnectionInterval = TimeSpan.FromSeconds(5);

    /// <summary>
    /// Creates the object using the provided connection string. An format of the string is:
    /// <para/>
    /// "Server=localhost;Port=5432;Database=mydatabase;User Id=myusername;Password=mypassword;"
    /// </summary>
    /// <param name="connectionString"></param>
    /// <returns></returns>
    public static ITagDataSource<TagObject>? Create(string connectionString, string tableName)
    {
        try
        {
            var connection = new NpgsqlConnection(connectionString);
            connection.Open();
            if (connection.State != ConnectionState.Open)
            {
                throw new InvalidOperationException($"Cannot open connection to the database: connectionString={connectionString}.");
            }

            var cmd = connection.CreateCommand();
            cmd.CommandText = $"SELECT COUNT(*) FROM {tableName}";
            var rowCount = cmd.ExecuteScalar();
            return new PostgresTagDataSource(connection, tableName);
        }
        catch (Exception)
        {
            return null;
        }
    }

    /// <summary>
    /// Creates the new object base on the provided connection string.
    /// </summary>
    /// <param name="connectionString"></param>
    /// <param name="tableName">The name of the table to read records from.</param>
    public PostgresTagDataSource(NpgsqlConnection connection, string tableName, ILogger? logger = null)
    {
        _connection = connection;
        _tableName = tableName;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves a collection of HostTag objects from the database for the specified host and time range.
    /// </summary>
    /// <param name="key">The host name for which to retrieve the tags.</param>
    /// <param name="start">The start time of the time range.</param>
    /// <param name="end">The end time of the time range.</param>
    /// <returns>An IEnumerable of HostTag objects representing the tags associated with the specified host and time range.</returns>
    public async Task<IEnumerable<TagObject>> GetAsync(string key, DateTimeOffset start, DateTimeOffset end)
    {
        try
        {
            NpgsqlCommand cmd = PrepareCommand(key, start, end);
            return await ReadObjectsAsync(cmd);
        }
        catch (Exception e)
        {
            _logger?.LogError(e, "Postgres tag source: Error executing reader.");
            return Array.Empty<TagObject>();
        }
    }

    private async Task<IList<TagObject>> ReadObjectsAsync(NpgsqlCommand cmd)
    {
        if (!EnsureOpenConnection(_connection))
            return new List<TagObject>();

        using var reader = await cmd.ExecuteReaderAsync();
        var rowList = new List<TagObject>();
        while (await reader.ReadAsync())
        {
            var row = ReadRow(reader);
            rowList.Add(row);
        }
        await reader.CloseAsync();
        _logger?.LogDebug($"Query {cmd.CommandText} returned {rowList.Count} rows.");
        return rowList;
    }

    /// <summary>
    /// Retrieves a collection of HostTag objects from the database for the specified host and time range.
    /// </summary>
    /// <param name="tagKey">The host name for which to retrieve the tags.</param>
    /// <param name="start">The start time of the time range.</param>
    /// <param name="end">The end time of the time range.</param>
    /// <returns>An IEnumerable of HostTag objects representing the tags associated with the specified host and time range.</returns>
    public IEnumerable<TagObject> Get(string tagKey, DateTimeOffset start, DateTimeOffset end)
    {
        try
        {
            using var cmd = PrepareCommand(tagKey, start, end);
            return ReadObjects(cmd);
        }
        catch (Exception e)
        {
            _logger?.LogError(e, "Postgres tag source: Error executing reader.");
            return new List<TagObject>();
        }
    }

    private IList<TagObject> ReadObjects(NpgsqlCommand cmd)
    {
        if (!EnsureOpenConnection(_connection))
            return new List<TagObject>();

        using var reader = cmd.ExecuteReader();
        var rowList = new List<TagObject>();
        while (reader.Read())
        {
            var row = ReadRow(reader);
            rowList.Add(row);
        }
        reader.Close();
        _logger?.LogDebug($"Query {cmd.CommandText} returned {rowList.Count} rows.");
        return rowList;
    }

    /// <summary>
    /// Represents the last connection attempt made by the PostgresTagDataSource.
    /// </summary>
    DateTime _lastConnectionAttempt = DateTime.MinValue;
    private bool EnsureOpenConnection(NpgsqlConnection connection)
    {
        if (connection.State != ConnectionState.Open && _lastConnectionAttempt + _reconnectionInterval < DateTime.Now)
        {   // try open:
            _logger?.LogWarning($"Postgres tag source: Connection closed. Trying to reconnecting...");
            try
            {
                _lastConnectionAttempt = DateTime.Now;
                connection.Open();
            }
            catch(Exception e)
            {
                _logger?.LogError(e, $"Postgres tag source: Error opening connection. Trying to reconnect in {_reconnectionInterval.TotalSeconds}s.");
            }
        }
        return connection.State == ConnectionState.Open;
    }

    public IEnumerable<TagObject> Get(string tagKey, string tagType, DateTimeOffset start, DateTimeOffset end)
    {
        try
        {
            using var cmd = PrepareCommand(tagKey, tagType, start, end);
            return ReadObjects(cmd);
        }
        catch (Exception e)
        {
            _logger?.LogError(e, "Postgres tag source: Error executing reader.");
            return new List<TagObject>();
        }
    }

    public Task<IEnumerable<TagObject>> GetAsync(string key, string tagType, DateTimeOffset start, DateTimeOffset end)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<TagObject> GetMany(IEnumerable<string> keys, string tagType, DateTimeOffset start, DateTimeOffset end)
    {
        if (keys is null || keys.Count() == 0) return Enumerable.Empty<TagObject>();
        try
        {
            using var cmd = PrepareCommand(keys, tagType, start, end);
            return ReadObjects(cmd);
        }
        catch (Exception e)
        {
            _logger?.LogError(e, "Postgres tag source: Error executing reader.");
            return new List<TagObject>();
        }      
    }
    public IEnumerable<TagObject> GetMany(IEnumerable<string> keys, DateTimeOffset start, DateTimeOffset end)
    {
        if (keys is null || keys.Count() == 0) return Enumerable.Empty<TagObject>();
        try
        {
            using var cmd = PrepareCommand(keys, start, end);
            return ReadObjects(cmd);
        }
        catch (Exception e)
        {
            _logger?.LogError(e, "Postgres tag source: Error executing reader.");
            return new List<TagObject>();
        }      
    }
    /// <summary>
    /// Prepares a NpgsqlCommand for retrieving data based on the provided tag key and time range.
    /// </summary>
    /// <param name="tagKey">The key associated with the tag for which data is to be retrieved.</param>
    /// <param name="start">The start time of the desired data retrieval range.</param>
    /// <param name="end">The end time of the desired data retrieval range.</param>
    /// <returns>A NpgsqlCommand object with the appropriate SQL query set as its CommandText property.</returns>
    /// <remarks>
    /// The generated SQL query will fetch all records from a specified table where the key matches the provided tagKey and
    /// the validity period overlaps with the provided time range. The validity period is represented using PostgreSQL's range type.
    /// <para/>
    /// </remarks>
    private NpgsqlCommand PrepareCommand(string tagKey, DateTimeOffset start, DateTimeOffset end)
    {
        var startString = start.UtcDateTime.ToString("o", CultureInfo.InvariantCulture);
        var endString = end.UtcDateTime.ToString("o", CultureInfo.InvariantCulture);
        var cmd = _connection.CreateCommand();

        cmd.CommandText = $"SELECT * FROM {_tableName} WHERE key ='{tagKey}' AND validity && '[{startString},{endString})'";
        return cmd;
    }
    private NpgsqlCommand PrepareCommand(string tagKey, string tagType, DateTimeOffset start, DateTimeOffset end)
    {
        var startString = start.UtcDateTime.ToString("o", CultureInfo.InvariantCulture);
        var endString = end.UtcDateTime.ToString("o", CultureInfo.InvariantCulture);
        var cmd = _connection.CreateCommand();

        cmd.CommandText = $"SELECT * FROM {_tableName} WHERE type='{tagType}' AND validity && '[{startString},{endString})' AND key ='{tagKey}'";
        return cmd;
    }
    private NpgsqlCommand PrepareCommand(IEnumerable<string> tagKeys, DateTimeOffset start, DateTimeOffset end)
    {
        var startString = start.UtcDateTime.ToString("o", CultureInfo.InvariantCulture);
        var endString = end.UtcDateTime.ToString("o", CultureInfo.InvariantCulture);
        var cmd = _connection.CreateCommand();
        var tagKeysExpr = String.Join(',', tagKeys.Select(x => $"'{x}'").ToArray());

        cmd.CommandText = $"SELECT * FROM {_tableName} WHERE validity && '[{startString},{endString})' AND key IN ({tagKeysExpr})";

        return cmd;
    }
    private NpgsqlCommand PrepareCommand(IEnumerable<string> tagKeys, string tagType, DateTimeOffset start, DateTimeOffset end)
    {
        var startString = start.UtcDateTime.ToString("o", CultureInfo.InvariantCulture);
        var endString = end.UtcDateTime.ToString("o", CultureInfo.InvariantCulture);
        var cmd = _connection.CreateCommand();

        var tagKeysExpr = String.Join(',', tagKeys.Select(x => $"'{x}'").ToArray());
        cmd.CommandText = $"SELECT * FROM {_tableName} WHERE type='{tagType}' AND validity && '[{startString},{endString})' AND key IN ({tagKeysExpr})";
        return cmd;
    }
    /// <summary>
    /// Creates a new table for storing <see cref="TcpFlowTag"/> records in the database if it does not alrady exist.
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

    /// <summary>
    /// Inserts a bulk of TagObject records into a PostgreSQL database table using binary import.
    /// </summary>
    /// <param name="connection">The active NpgsqlConnection to the PostgreSQL database.</param>
    /// <param name="tableName">The name of the table to which the records should be inserted.</param>
    /// <param name="records">An enumerable of TagObject records to be inserted.</param>
    /// <returns>The number of records inserted.</returns>
    /// <remarks>
    /// This method uses the Npgsql binary import functionality to optimize the insertion of large volumes of data.
    /// Each record field is truncated as needed to fit the database table's column constraints.
    /// </remarks>
    public static int BulkInsert(NpgsqlConnection connection, string tableName, IEnumerable<TagObject> records)
    {

        string Truncate(string? input, int maxsize) => input?.Substring(0, Math.Min(input.Length, maxsize)) ?? string.Empty;

        var recordCount = 0;
        using (var writer = connection.BeginBinaryImport($"COPY {tableName} (type, key, value, reliability, details, validity) FROM STDIN (FORMAT BINARY)"))
        {
            foreach (var record in records)
            {
                recordCount++;
                writer.StartRow();
                writer.Write(Truncate(record.Type, ColumnTypeLength), NpgsqlDbType.Text);
                writer.Write(Truncate(record.Key, ColumnKeyLength), NpgsqlDbType.Text);
                writer.Write(Truncate(record.Value, ColumnValueLength), NpgsqlDbType.Text);
                writer.Write(record.Reliability, NpgsqlDbType.Real);
                writer.Write(record.Details, NpgsqlDbType.Json);
                writer.Write(new NpgsqlRange<DateTimeOffset>(record.StartTime.UtcDateTime, record.EndTime.UtcDateTime), NpgsqlDbType.TimestampTzRange);
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
            ("id","SERIAL PRIMARY KEY"),
            ("type", $"VARCHAR({ColumnTypeLength}) NOT NULL"),
            ("key", $"VARCHAR({ColumnKeyLength}) NOT NULL"),
            ("value", $"VARCHAR({ColumnValueLength})"),
            ("reliability", "REAL"),
            ("validity", "TSTZRANGE"),
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
        var validity = reader.GetFieldValue<NpgsqlRange<DateTimeOffset>>("validity");
        var details = reader.GetFieldValue<string>("details");

        return new TagObject
        {
            Type = reader.GetString("type"),
            Key = reader.GetString("key"),
            Value = reader.GetString("value"),
            Reliability = reader.GetFloat("reliability"),
            StartTime = validity.LowerBound,
            EndTime = validity.UpperBound,
            Details = details != null ? JsonSerializer.Deserialize<ExpandoObject>(reader.GetString("details")) : null
        };
    }

    public void Dispose()
    {
        _connection.Close();
        _connection.Dispose();
    }
}

