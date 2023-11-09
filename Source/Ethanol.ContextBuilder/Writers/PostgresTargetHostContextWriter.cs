using Ethanol.ContextBuilder.Helpers;
using Ethanol.ContextBuilder.Observable;
using Ethanol.ContextBuilder.Polishers;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;
using System;

namespace Ethanol.ContextBuilder.Writers
{
    /// <summary>
    /// A writer plugin responsible for serializing context data to a PostgreSQL table.
    /// </summary>
    /// <remarks>
    /// The <see cref="PostgresTargetHostContextWriter"/> class is specifically designed to handle <see cref="ObservableEvent{IpTargetHostContext}"/> 
    /// and serialize them into a PostgreSQL table. The class provides functionality to establish connections, create tables if they 
    /// don't exist, and write context data.
    /// </remarks>
    public class PostgresTargetHostContextWriter : ContextWriter<ObservableEvent<IpTargetHostContext>>
    {

        protected readonly ILogger _logger;

        // Column definitions for the PostgreSQL table
        private static string[] __columns = new string[]
        {
            "id SERIAL PRIMARY KEY",
            "key VARCHAR(255) NOT NULL",
            "tags JSON",
            "connections JSON",
            "resolveddomains JSON",
            "weburls JSON",
            "tlshandshakes JSON",
            "validity TSRANGE"
        };

        // Connection to the PostgreSQL database
        private readonly NpgsqlConnection _connection;

        // Name of the target table in the PostgreSQL database
        private readonly string _tableName;

        /// <summary>
        /// Initializes a new instance of the <see cref="PostgresTargetHostContextWriter"/> class.
        /// </summary>
        /// <param name="connection">Connection to the PostgreSQL database.</param>
        /// <param name="tableName">Name of the target table in the database.</param>
        public PostgresTargetHostContextWriter(NpgsqlConnection connection, string tableName, ILogger logger)
        {
            _connection = connection;
            _tableName = tableName;
            _logger = logger;
        }

        /// <summary>
        /// Creates a new table in the database if it doesn't exist.
        /// </summary>
        /// <param name="connection">Connection to the PostgreSQL database.</param>
        /// <param name="tableName">Name of the target table in the database.</param>
        /// <returns>Returns true after successfully creating the table.</returns>
        public static bool CreateTableIfNotExists(NpgsqlConnection connection, string tableName)
        {
            connection.CreateTable(tableName, __columns);
            connection.CreateIndex(tableName, "key");
            return true;
        }

        /// <summary>
        /// Closes the database connection.
        /// </summary>
        protected override void Close()
        {
            _connection.Close();
        }

        /// <summary>
        /// Opens the database connection and ensures the required table exists.
        /// </summary>
        protected override void Open()
        {
            if (_connection.State != System.Data.ConnectionState.Open)
            {
                _logger?.LogInformation($"Open DB connection '{_connection.ConnectionString}'");
                _connection.Open();
            }
            else
            {
                _logger?.LogInformation($"Connection '{_connection.ConnectionString}' already opened.");
            }
            CreateTableIfNotExists(_connection, _tableName);
        }

        /// <summary>
        /// Writes the provided context data to the PostgreSQL table.
        /// </summary>
        /// <param name="entity">The context data to be written.</param>
        protected override void Write(ObservableEvent<IpTargetHostContext> entity)
        {
            using (var cmd = new NpgsqlCommand())
            {
                cmd.Connection = _connection;
                cmd.CommandText = $"INSERT INTO {_tableName} (key, connections, resolveddomains, weburls, tlshandshakes, validity) VALUES (@key, @connections, @resolveddomains, @weburls, @tlshandshakes, @validity)";

                cmd.Parameters.AddWithValue("key", NpgsqlTypes.NpgsqlDbType.Text, entity.Payload.HostAddress.ToString());
                cmd.Parameters.AddWithValue("connections", NpgsqlTypes.NpgsqlDbType.Json, entity.Payload.Connections);
                cmd.Parameters.AddWithValue("resolveddomains", NpgsqlTypes.NpgsqlDbType.Json, entity.Payload.ResolvedDomains);
                cmd.Parameters.AddWithValue("weburls", NpgsqlTypes.NpgsqlDbType.Json, entity.Payload.WebUrls);
                cmd.Parameters.AddWithValue("tlshandshakes", NpgsqlTypes.NpgsqlDbType.Json, entity.Payload.TlsHandshakes);
                cmd.Parameters.AddWithValue("validity", new NpgsqlRange<DateTime>(entity.StartTime, entity.EndTime));
                cmd.ExecuteNonQuery();
            }
        }
    }
}
