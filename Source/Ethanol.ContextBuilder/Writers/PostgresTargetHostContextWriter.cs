using Ethanol.ContextBuilder.Enrichers;
using Ethanol.ContextBuilder.Helpers;
using Ethanol.ContextBuilder.Observable;
using Ethanol.ContextBuilder.Plugins.Attributes;
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
    [Plugin(PluginCategory.Writer, "PostgresWriter", "Writes context to PostgreSQL table.")]
    public class PostgresTargetHostContextWriter : ContextWriter<ObservableEvent<IpTargetHostContext>>
    {

        static protected readonly ILogger __logger = LogManager.GetCurrentClassLogger();

        // Column definitions for the PostgreSQL table
        private static string[] __columns = new string[]
        {
            "key VARCHAR(255) NOT NULL",
            "tags JSON",
            "initiatedconnections JSON",
            "acceptedconnections JSON",
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
        public PostgresTargetHostContextWriter(NpgsqlConnection connection, string tableName)
        {
            _connection = connection;
            _tableName = tableName;
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
            _connection.Open();
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
                cmd.CommandText = $"INSERT INTO {_tableName} (key, initiatedconnections, acceptedconnections, resolveddomains, weburls, tlshandshakes, validity) VALUES (@key, @initiatedconnections, @acceptedconnections, @resolveddomains, @weburls, @tlshandshakes, @validity)";

                cmd.Parameters.AddWithValue("key", NpgsqlTypes.NpgsqlDbType.Text, entity.Payload.HostAddress.ToString());
                cmd.Parameters.AddWithValue("initiatedconnections", NpgsqlTypes.NpgsqlDbType.Json, entity.Payload.InitiatedConnections);
                cmd.Parameters.AddWithValue("acceptedconnections", NpgsqlTypes.NpgsqlDbType.Json, entity.Payload.AcceptedConnections);
                cmd.Parameters.AddWithValue("resolveddomains", NpgsqlTypes.NpgsqlDbType.Json, entity.Payload.ResolvedDomains);
                cmd.Parameters.AddWithValue("weburls", NpgsqlTypes.NpgsqlDbType.Json, entity.Payload.WebUrls);
                cmd.Parameters.AddWithValue("tlshandshakes", NpgsqlTypes.NpgsqlDbType.Json, entity.Payload.TlsHandshakes);
                cmd.Parameters.AddWithValue("validity", new NpgsqlRange<DateTime>(entity.StartTime, entity.EndTime));
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Creates a new instance of <see cref="PostgresTargetHostContextWriter"/> based on the provided configuration.
        /// </summary>
        /// <param name="configuration">Configuration settings for the writer.</param>
        /// <returns>Returns an instance of <see cref="PostgresTargetHostContextWriter"/>.</returns>
        [PluginCreate]
        public static PostgresTargetHostContextWriter Create(EnricherConfiguration.PostgresConfiguration configuration)
        {
            var connection = new NpgsqlConnection(configuration.ToPostgresConnectionString());
            return new PostgresTargetHostContextWriter(connection, configuration.TableName);
        }
    }
}
