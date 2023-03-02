using Ethanol.ContextBuilder.Builders;
using Ethanol.ContextBuilder.Context;
using Ethanol.ContextBuilder.Observable;
using Ethanol.ContextBuilder.Plugins.Attributes;
using System;
using YamlDotNet.Serialization;

namespace Ethanol.ContextBuilder.Enrichers
{
    /// <summary>
    /// Enrich the computed context with additional known information.
    /// </summary>
    [Plugin(PluginType.Enricher, "IpHostContextEnricher", "Enriches the context for IP hosts from the provided data.")]
    public class IpHostContextEnricherPlugin : IObservableTransformer
    {
        private IpHostContextEnricher _enricher;

        public IpHostContextEnricherPlugin(IpHostContextEnricher enricher)
        {
            _enricher = enricher;
        }

        public string TransformerName => nameof(IpHostContextEnricherPlugin);

        public Type SourceType => typeof(ObservableEvent<IpHostContext>);

        public Type TargetType => typeof(ObservableEvent<IpRichHostContext>);

        public class Configuration
        {
            [YamlMember(Alias = "source", Description = "The data source (postgres).")]
            public string DataSource { get; set; } = String.Empty;
            [YamlMember(Alias = "connection", Description = "The connection string for connecting to the data source.")]
            public DatabaseConnection Connection { get; set; } = DatabaseConnection.Empty;
            [YamlMember(Alias = "tableName", Description = "The name of the table in the database to get the host tags from.")]
            public string TableName { get; set; } = String.Empty;
        }
        public record DatabaseConnection
        {
            public string Server { get; set; }
            public int Port { get; set; }
            public string Database { get; set; }
            public string User { get; set; }
            public string Password { get; set; }

            public DatabaseConnection(string server, int port, string database, string user, string password)
            {
                Server = server;
                Port = port;
                Database = database;
                User = user;
                Password = password;
            }

            public DatabaseConnection()
            {
            }

            public static DatabaseConnection Empty => new DatabaseConnection(String.Empty, 0, String.Empty, String.Empty, String.Empty);

            /// <summary>
            /// Gets the connection informaiton as a Postres connection string, e.g.:
            /// <para/>
            /// "Server=localhost;Port=5432;Database=mydatabase;User Id=myusername;Password=mypassword;"
            /// </summary>
            /// <returns></returns>
            public string ToPostgresConnectionString()
            {
                return $"Server={Server};Port={Port};Database={Database};User Id={User};Password={Password};";
            }
        }
        [PluginCreate]
        internal static IObservableTransformer Create(Configuration configuration)
        {
            if (configuration.DataSource == null) throw new ArgumentNullException($"{nameof(configuration.DataSource)} cannot be null!");
            // depending on the provided configuration we need to instantiate the enricher:
            switch (configuration.DataSource.ToLowerInvariant())
            {
                case "postgres":
                    var postgres = PostgresHostTagProvider.Create(configuration.Connection.ToPostgresConnectionString(), configuration.TableName);
                    var enricher = new IpHostContextEnricher(postgres, null);
                    return new IpHostContextEnricherPlugin(enricher);
                default:
                    throw new NotImplementedException($"Data source '{configuration.DataSource}' is not supported (yet).");
            }
        }

        public void OnCompleted()
        {
            _enricher.OnCompleted();
        }

        public void OnError(Exception error)
        {
            _enricher.OnError(error);
        }

        public void OnNext(object value)
        {
            _enricher.OnNext((ObservableEvent<IpHostContext>)value);
        }

        public IDisposable Subscribe(IObserver<object> observer)
        {
            return _enricher.Subscribe(observer);
        }
    }
}
