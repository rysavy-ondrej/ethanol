using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Xml;
using Npgsql;
using CsvHelper;
using Elastic.Clients.Elasticsearch;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading.Tasks;
using System.Reflection.PortableExecutable;

namespace Ethanol.ContextBuilder.Enrichers
{
    /// <summary>
    /// Represents an environment data stored in Postgre SQL database.
    /// </summary>
    /// <remarks>
    /// It is expected that the table has the following structure:
    /// <code>
    /// CREATE TABLE SomeTableName (
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
            var connection = new NpgsqlConnection(connectionString);
            connection.Open();
            if (connection.State != System.Data.ConnectionState.Open) throw new InvalidOperationException("Cannot open connectio to the database.");
            return new PostgresHostTagProvider(connection, tableName);
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
        /// Gets records for the given key only valid within the specified interval.
        /// </summary>
        /// <param name="host">The host key of the records.</param>
        /// <param name="start">The start time of the interval.</param>
        /// <param name="end">The end time of the interval.</param>
        /// <returns>An enumerable collection of records.</returns>
        public async Task<IEnumerable<HostTag>> GetAsync(string host, DateTime start, DateTime end)
        {
            var cmd = _connection.CreateCommand();
            // SELECT * FROM smartads WHERE Host = '192.168.1.32' AND Validity @> '[2022-06-01T14:00:00,2022-06-01T14:05:00)';
            cmd.CommandText = $"SELECT * FROM {_tableName} WHERE KeyValue ='{host}' AND Validity @> '[{start},{end})'";
            var reader = await cmd.ExecuteReaderAsync();
            var rowList = new List<HostTag>();
            while (await reader.ReadAsync())
            {
                var row = new HostTag(reader["KeyValue"] as string,
                                      reader["Source"] as string,
                                      reader["Reliability"] as double? ?? 1.0,
                                      reader["Data"] as string);
                rowList.Add(row);
            }
            return rowList;
        }
        public IEnumerable<HostTag> Get(string host, DateTime start, DateTime end)
        {
            var cmd = _connection.CreateCommand();
            // SELECT * FROM smartads WHERE Host = '192.168.1.32' AND Validity @> '[2022-06-01T14:00:00,2022-06-01T14:05:00)';
            cmd.CommandText = $"SELECT * FROM {_tableName} WHERE KeyValue ='{host}' AND Validity @> '[{start},{end})'";
            var reader = cmd.ExecuteReader();
            var rowList = new List<HostTag>();
            while (reader.Read())
            {
                var row = new HostTag(reader["KeyValue"] as string,
                                      reader["Source"] as string,
                                      reader["Reliability"] as double? ?? 1.0,
                                      reader["Data"] as string);
                rowList.Add(row);
            }
            return rowList;
        }
    }
}
