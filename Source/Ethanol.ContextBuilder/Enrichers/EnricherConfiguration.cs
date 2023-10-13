using System;
using YamlDotNet.Serialization;

namespace Ethanol.ContextBuilder.Enrichers
{
    public class EnricherConfiguration
    {
        [YamlMember(Alias = "postgres", Description = "The Postres data source configuration.")]
        public PostgresConfiguration Postgres { get; set; }

        [YamlMember(Alias = "jsonfile", Description = "The JSON file data source configuration.")]
        public JsonConfiguration Json { get; set; }


        public record JsonConfiguration
        {
            [YamlMember(Alias = "filename", Description = "The name of the source JSON file.")]
            public string Filename { get; set; }

            [YamlMember(Alias = "collection", Description = "The name(s) of the collection(s) to read from the JSON file.")]
            public string Collection { get; set; }
        }

        public record PostgresConfiguration
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

            public PostgresConfiguration(string server, string port, string database, string user, string password)
            {
                Server = server;
                Port = port;
                Database = database;
                User = user;
                Password = password;
            }

            public PostgresConfiguration()
            {
            }

            public static PostgresConfiguration Empty => new PostgresConfiguration(String.Empty, String.Empty, String.Empty, String.Empty, String.Empty);

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
    }   
}
