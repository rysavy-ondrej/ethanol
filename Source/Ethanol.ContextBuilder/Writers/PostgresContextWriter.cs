using Ethanol.ContextBuilder.Enrichers;
using Ethanol.ContextBuilder.Observable;
using Ethanol.ContextBuilder.Plugins.Attributes;
using Ethanol.ContextBuilder.Polishers;
using NLog;
using Npgsql;
using NpgsqlTypes;
using System;
using static Ethanol.ContextBuilder.Enrichers.IpHostContextEnricherPlugin;

namespace Ethanol.ContextBuilder.Writers
{
    [Plugin(PluginCategory.Writer, "PostgresWriter", "Writes context to Postres HostContext table.")]
    public class PostgresContextWriter : ContextWriter<ObservableEvent<IpTargetHostContext>>
    {
        static protected readonly Logger __logger = LogManager.GetCurrentClassLogger();
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
        private readonly NpgsqlConnection _connection;
        private readonly string _tableName;

        public PostgresContextWriter(NpgsqlConnection connection, string tableName)
        {
            _connection = connection;
            _tableName = tableName;
        }

        public static bool CreateTableIfNotExists(NpgsqlConnection connection, string tableName)
        {
            connection.CreateTable(tableName, __columns);
            connection.CreateIndex(tableName, "key");
            return true;
        }

        protected override void Close()
        {
            _connection.Close();
        }

        protected override void Open()
        {
            _connection.Open();
            CreateTableIfNotExists(_connection, _tableName);
        }

        protected override void Write(ObservableEvent<IpTargetHostContext> entity)
        {
            using (var cmd = new NpgsqlCommand())
            {
                cmd.Connection = _connection;
                cmd.CommandText = $"INSERT INTO {_tableName} (key, tags, initiatedconnections, acceptedconnections, resolveddomains, weburls, tlshandshakes, validity) VALUES (@key, @tags, @initiatedconnections, @acceptedconnections, @resolveddomains, @weburls, @tlshandshakes, @validity)";
                
                cmd.Parameters.AddWithValue("key", NpgsqlTypes.NpgsqlDbType.Text,entity.Payload.HostAddress.ToString());                
                cmd.Parameters.AddWithValue("tags", NpgsqlTypes.NpgsqlDbType.Json, entity.Payload.CustomAttributes);               
                cmd.Parameters.AddWithValue("initiatedconnections", NpgsqlTypes.NpgsqlDbType.Json, entity.Payload.InitiatedConnections);                
                cmd.Parameters.AddWithValue("acceptedconnections", NpgsqlTypes.NpgsqlDbType.Json, entity.Payload.AcceptedConnections);
                cmd.Parameters.AddWithValue("resolveddomains", NpgsqlTypes.NpgsqlDbType.Json, entity.Payload.ResolvedDomains);
                cmd.Parameters.AddWithValue("weburls", NpgsqlTypes.NpgsqlDbType.Json, entity.Payload.WebUrls);
                cmd.Parameters.AddWithValue("tlshandshakes", NpgsqlTypes.NpgsqlDbType.Json, entity.Payload.TlsHandshakes);
                cmd.Parameters.AddWithValue("validity", new NpgsqlRange<DateTime>(entity.StartTime, entity.EndTime));
                cmd.ExecuteNonQuery();
            }
        }

        [PluginCreate]
        public static PostgresContextWriter Create(PostgresCofiguration configuration)
        {
            var connection = new NpgsqlConnection(configuration.ToPostgresConnectionString());
            return new PostgresContextWriter(connection, configuration.TableName);
        }
    }
}
