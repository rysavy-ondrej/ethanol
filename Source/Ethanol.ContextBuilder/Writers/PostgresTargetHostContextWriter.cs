﻿using Ethanol.ContextBuilder.Helpers;
using Ethanol.DataObjects;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Ethanol.ContextBuilder.Writers
{
    /// <summary>
    /// A writer plugin responsible for serializing context data to a PostgreSQL table.
    /// </summary>
    /// <remarks>
    /// The <see cref="PostgresTargetHostContextWriter"/> class is specifically designed to handle <see cref="TimeRange{IpTargetHostContext}"/> 
    /// and serialize them into a PostgreSQL table. The class provides functionality to establish connections, create tables if they 
    /// don't exist, and write context data.
    /// </remarks>
    public class PostgresTargetHostContextWriter : ContextWriter<HostContext>
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

        private readonly int _maxChunkSize;

        /// <summary>
        /// Initializes a new instance of the <see cref="PostgresTargetHostContextWriter"/> class.
        /// </summary>
        /// <param name="connection">Connection to the PostgreSQL database.</param>
        /// <param name="tableName">Name of the target table in the database.</param>
        public PostgresTargetHostContextWriter(NpgsqlConnection connection, string tableName, int maxChunkSize, ILogger logger)
        {
            _connection = connection;
            _tableName = tableName;
            _logger = logger;
            _maxChunkSize = maxChunkSize;
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
        protected override void Write(HostContext entity)
        {
            if (entity.Key == null)
            {
                _logger?.LogWarning($"Skipping record with null key.");
                return;
            }

            if (!EnsureOpenConnection())
            {
                _logger?.LogError($"Postgres Writer: Cannot write to database. Connection is closed.");
                return;
            }
            try
            {
                using (var cmd = new NpgsqlCommand())
                {
                    cmd.Connection = _connection;
                    cmd.CommandText = $"INSERT INTO {_tableName} (key, connections, resolveddomains, weburls, tlshandshakes, validity) VALUES (@key, @connections, @resolveddomains, @weburls, @tlshandshakes, @validity)";

                    cmd.Parameters.AddWithValue("key", NpgsqlTypes.NpgsqlDbType.Text, entity.Key.ToString());
                    cmd.Parameters.AddWithValue("connections", NpgsqlTypes.NpgsqlDbType.Json, entity.Connections ?? Array.Empty<IpConnectionInfo>());
                    cmd.Parameters.AddWithValue("resolveddomains", NpgsqlTypes.NpgsqlDbType.Json, entity.ResolvedDomains ?? Array.Empty<ResolvedDomainInfo>());
                    cmd.Parameters.AddWithValue("weburls", NpgsqlTypes.NpgsqlDbType.Json, entity.WebUrls ?? Array.Empty<WebRequestInfo>());
                    cmd.Parameters.AddWithValue("tlshandshakes", NpgsqlTypes.NpgsqlDbType.Json, entity.TlsHandshakes ?? Array.Empty<TlsHandshakeInfo>());
                    cmd.Parameters.AddWithValue("validity", new NpgsqlRange<DateTimeOffset>(entity.Start.ToUniversalTime(), true, entity.End.ToUniversalTime(), false));
                    cmd.ExecuteNonQuery();
                }
            }
            catch(Exception ex)
            {
                _logger?.LogError(ex, $"Postgres Writer: Error writing record to database.");
            }            
        }

        /// <summary>
        /// Ensures that the database connection is open. If the connection is already open, returns true. 
        /// If the connection is closed, attempts to open it and returns true if successful. 
        /// If an exception occurs while opening the connection, logs the error and returns false.
        /// </summary>
        /// <returns>True if the connection is open or successfully opened, false otherwise.</returns>
        private bool EnsureOpenConnection()
        {
            if (_connection.State == System.Data.ConnectionState.Open)
            {
                return true;
            }            
            try
            {
                _logger?.LogInformation($"Postgres Writer: Open DB connection '{_connection.ConnectionString}'");
                _connection.Open();
            }
            catch(Exception e)
            {
                _logger?.LogError(e, $"Postgres Writer: Error opening connection '{_connection.ConnectionString}'");
                return false;
            }
            return _connection.State == System.Data.ConnectionState.Open;
        }

        public override string ToString()
        {
            return $"{nameof(PostgresTargetHostContextWriter)}({_connection.ConnectionString}))";
        }

        protected override void WriteBatch(IEnumerable<HostContext> records)
        {
            if (!EnsureOpenConnection())
            {
                _logger?.LogError($"Postgres Writer: Cannot write to database. Connection is closed.");
                return;
            }  
            try
            {
                foreach (var chunk in records.Chunk(_maxChunkSize))
                {
                    using (var writer = _connection.BeginBinaryImport($"COPY {_tableName} (key, connections, resolveddomains, weburls, tlshandshakes, validity) FROM STDIN (FORMAT BINARY)"))
                    {
                         _logger.LogInformation($"Postgres Context Writer: BulkInsert: {chunk.Length} records.");
                        foreach (var item in chunk)
                        {
                            writer.StartRow();
                            writer.Write(item.Key?.ToString() ?? string.Empty);
                            writer.Write(item.Connections ?? Array.Empty<IpConnectionInfo>(), NpgsqlDbType.Json);
                            writer.Write(item.ResolvedDomains ?? Array.Empty<ResolvedDomainInfo>(), NpgsqlDbType.Json);
                            writer.Write(item.WebUrls ?? Array.Empty<WebRequestInfo>(), NpgsqlDbType.Json);
                            writer.Write(item.TlsHandshakes ?? Array.Empty<TlsHandshakeInfo>(), NpgsqlDbType.Json);
                            writer.Write(new NpgsqlRange<DateTimeOffset>(item.Start.ToUniversalTime(), true, item.End.ToUniversalTime(), false));
                        }
                        writer.Complete();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Postgres Writer: Error writing batch to database.");
            }
        }

        public override void OnWindowClosed(DateTimeOffset start, DateTimeOffset end)
        {
            Write(new HostContext { Key = IPAddress.Any.ToString(), Start = start, End = end });
        }
    }
}
