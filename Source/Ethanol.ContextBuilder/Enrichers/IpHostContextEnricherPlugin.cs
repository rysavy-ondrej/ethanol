using Ethanol.ContextBuilder.Context;
using Ethanol.ContextBuilder.Observable;
using Ethanol.ContextBuilder.Plugins.Attributes;
using System;
using System.Globalization;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;
using static Ethanol.ContextBuilder.Enrichers.IpHostContextEnricherPlugin.JsonConfiguration;

namespace Ethanol.ContextBuilder.Enrichers
{
    /// <summary>
    /// Enrich the computed context with additional known information.
    /// </summary>
    [Plugin(PluginCategory.Enricher, "IpHostContextEnricher", "Enriches the context for IP hosts from the provided data.")]
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

        public class DataSourceEnricherConfiguration
        {
            [YamlMember(Alias = "postgres", Description = "The Postres data source configuration.")]
            public PostgresCofiguration Postgres { get; set; }

            [YamlMember(Alias = "jsonfile", Description = "The JSON file data source configuration.")]
            public JsonConfiguration Json { get; set; }
        }

        public record CsvSourceConfiguration
        {
            [YamlMember(Alias = "filename", Description = "The name of the source JSON file.")]
            public string Filename { get; set; }
        }

        public record JsonConfiguration
        {
            [YamlMember(Alias = "filename", Description = "The name of the source JSON file.")]
            public string Filename { get; set; }

            [YamlMember(Alias = "collection", Description = "The name(s) of the collection(s) to read from the JSON file.")]
            public string Collection { get; set; }
        }

        public record PostgresCofiguration
        {
            [YamlMember(Alias = "server", Description = "The server ip address.")]
            public string Server { get; set; }
            
            [YamlMember(Alias = "port", Description = "The port on which the server listen.")]
            public string Port { get; set; }
            
            [YamlMember(Alias = "database", Description = "The database to open on the server.")]
            public string Database { get; set; }
            
            [YamlMember(Alias = "user", Description = "The user name used for login.")]
            public string User { get; set; }
            
            [YamlMember(Alias = "password", Description = "The password used for login.")]
            public string Password { get; set; }

            [YamlMember(Alias = "tableName", Description = "The name of the table in the database to get the data from.")]
            public string TableName { get; set; } = String.Empty;

            public PostgresCofiguration(string server, string port, string database, string user, string password)
            {
                Server = server;
                Port = port;
                Database = database;
                User = user;
                Password = password;
            }

            public PostgresCofiguration()
            {
            }

            public static PostgresCofiguration Empty => new PostgresCofiguration(String.Empty, String.Empty, String.Empty, String.Empty, String.Empty);

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
        internal static IObservableTransformer Create(DataSourceEnricherConfiguration hostTagConfiguraiton)
        {
            ITagDataProvider<TagRecord> tagsProvider = hostTagConfiguraiton.GetTagProvider();
          
            var enricher = new IpHostContextEnricher(tagsProvider);
            return new IpHostContextEnricherPlugin(enricher);
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
