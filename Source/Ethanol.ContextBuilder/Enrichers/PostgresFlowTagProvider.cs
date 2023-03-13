﻿using Microsoft.CodeAnalysis.CSharp.Syntax;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace Ethanol.ContextBuilder.Enrichers
{
    /// <summary>
    /// Represents a FLowTag data source stored in PostgreSQL database.
    /// </summary>
    /// <remarks>
    /// It is expected that the table has the following structure:
    /// <code>
    /// CREATE TABLE flowtags (
    ///   LocalAddress VARCHAR(32),
    ///   LocalPort INTEGER,
    ///   RemoteAddress VARCHAR(32),
    ///   RemotePort INTEGER,
    ///   ProcessName VARCHAR(128)
    ///   Validity TSRANGE
    /// );
    /// </code>
    /// </remarks>
    public class PostgresFlowTagProvider : IHostDataProvider<FlowTag>
    {
        static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly NpgsqlConnection _connection;
        private readonly string _tableName;

        /// <summary>
        /// Creates the object using the provided connection string. An format of the string is:
        /// <para/>
        /// "Server=localhost;Port=5432;Database=mydatabase;User Id=myusername;Password=mypassword;"
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public static PostgresFlowTagProvider Create(string connectionString, string tableName)
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
                logger.Info($"Postgres connected '{connectionString}', available {rowCount} records.");

                

                return new PostgresFlowTagProvider(connection, tableName);
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Cannot create {nameof(PostgresFlowTagProvider)}: {ex.Message}. {ex.InnerException?.Message}");
                return null;
            }            
        }

        /// <summary>
        /// Creates the new object base on the provided connection string.
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="tableName">The name of the table to read records from.</param>
        PostgresFlowTagProvider(NpgsqlConnection connection, string tableName)
        {
            _connection = connection;
            _tableName = tableName;
        }

        /// <summary>
        /// Retrieves a collection of FlowTag objects from the database for the specified host and time range.
        /// </summary>
        /// <param name="host">The host name for which to retrieve the tags.</param>
        /// <param name="start">The start time of the time range.</param>
        /// <param name="end">The end time of the time range.</param>
        /// <returns>An IEnumerable of FlowTag objects representing the tags associated with the specified host and time range.</returns>
        public async Task<IEnumerable<FlowTag>> GetAsync(string host, DateTime start, DateTime end)
        {
            try
            {
                var cmd = _connection.CreateCommand();
                // SELECT * FROM smartads WHERE Host = '192.168.1.32' AND Validity @> '[2022-06-01T14:00:00,2022-06-01T14:05:00)';
                cmd.CommandText = $"SELECT * FROM {_tableName} WHERE LocalAddress='{host}' AND Validity && '[{start},{end})'";
                var reader = await cmd.ExecuteReaderAsync();
                var rowList = new List<FlowTag>();
                while (await reader.ReadAsync())
                {
                    var row = new FlowTag(start, end,
                                          reader["LocalAddress"] as string,
                                          reader["LocalPort"] as ushort? ?? 0,
                                          reader["RemoteAddress"] as string,
                                          reader["RemotePort"] as ushort? ?? 0,
                                          reader["ProcessName"] as string
                                          );
                    rowList.Add(row);
                }
                return rowList;
            }
            catch (Exception e)
            {
                logger.Error(e);
                return Array.Empty<FlowTag>();
            }
        }
        /// <summary>
        /// Retrieves a collection of FlowTag objects from the database for the specified host and time range.
        /// </summary>
        /// <param name="host">The host name for which to retrieve the tags.</param>
        /// <param name="start">The start time of the time range.</param>
        /// <param name="end">The end time of the time range.</param>
        /// <returns>An IEnumerable of FlowTag objects representing the tags associated with the specified host and time range.</returns>
        public IEnumerable<FlowTag> Get(string host, DateTime start, DateTime end)
        {
            try
            {
                var startString = start.ToString("o", CultureInfo.InvariantCulture);
                var endString = end.ToString("o", CultureInfo.InvariantCulture);
                var cmd = _connection.CreateCommand();
                // SELECT * FROM smartads WHERE Host = '192.168.1.32' AND Validity @> '[2022-06-01T14:00:00,2022-06-01T14:05:00)';
                cmd.CommandText = $"SELECT * FROM {_tableName} WHERE LocalAddress='{host}' AND Validity && '[{startString},{endString})'";
                var reader = cmd.ExecuteReader();
                var rowList = new List<FlowTag>();
                while (reader.Read())
                {
                    var row = new FlowTag(start, end,
                                          reader["LocalAddress"] as string,
                                          reader["LocalPort"] as ushort? ?? 0,
                                          reader["RemoteAddress"] as string,
                                          reader["RemotePort"] as ushort? ?? 0,
                                          reader["ProcessName"] as string
                                          );
                    rowList.Add(row);
                }
                reader.Close();
                return rowList;
            }
            catch(Exception e)
            {
                logger.Error(e);
                return Array.Empty<FlowTag>();
            }
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
                    StartTime TIMESTAMP NOT NULL,
                    EndTime TIMESTAMP NOT NULL,
                    LocalAddress VARCHAR(32) NOT NULL,
                    LocalPort SMALLINT NOT NULL,
                    RemoteAddress VARCHAR(32) NOT NULL,
                    RemotePort SMALLINT NOT NULL,
                    ProcessName VARCHAR(128) NOT NULL
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
    }
}
