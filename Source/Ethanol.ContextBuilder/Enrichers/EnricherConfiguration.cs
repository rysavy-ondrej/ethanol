using System;
using YamlDotNet.Serialization;

namespace Ethanol.ContextBuilder.Enrichers
{
    /// <summary>
    /// Represents the configuration for data enrichers, containing settings related to different data sources like PostgreSQL and JSON files.
    /// </summary>
    public class EnricherConfiguration
    {
        /// <summary>
        /// Gets or sets the PostgreSQL data source configuration.
        /// </summary>
        [YamlMember(Alias = "postgres", Description = "The Postres data source configuration.")]
        public PostgresConfiguration Postgres { get; set; }

        /// <summary>
        /// Gets or sets the JSON file data source configuration.
        /// </summary>
        [YamlMember(Alias = "jsonfile", Description = "The JSON file data source configuration.")]
        public JsonConfiguration Json { get; set; }

        /// <summary>
        /// Represents the configuration specific to sourcing data from a JSON file.
        /// </summary>
        public record JsonConfiguration
        {
            /// <summary>
            /// Gets or sets the name of the source JSON file.
            /// </summary>
            [YamlMember(Alias = "filename", Description = "The name of the source JSON file.")]
            public string Filename { get; set; }

            /// <summary>
            /// Gets or sets the name(s) of the collection(s) to read from the JSON file.
            /// </summary>
            [YamlMember(Alias = "collection", Description = "The name(s) of the collection(s) to read from the JSON file.")]
            public string Collection { get; set; }
        }

        /// <summary>
        /// Represents the configuration specific to sourcing data from a PostgreSQL database.
        /// </summary>
        public record PostgresConfiguration
        {
            /// <summary>
            /// Gets or sets the IP address of the PostgreSQL server.
            /// </summary>
            [YamlMember(Alias = "server", Description = "The server ip address.")]
            public string Server { get; set; }

            /// <summary>
            /// Gets or sets the port number on which the PostgreSQL server is listening.
            /// </summary>
            [YamlMember(Alias = "port", Description = "The port on which the server listens.")]
            public string Port { get; set; }

            /// <summary>
            /// Gets or sets the name of the database on the PostgreSQL server.
            /// </summary>
            [YamlMember(Alias = "database", Description = "The database to open on the server.")]
            public string Database { get; set; }

            /// <summary>
            /// Gets or sets the username used to log into the PostgreSQL server.
            /// </summary>
            [YamlMember(Alias = "user", Description = "The username used for login.")]
            public string User { get; set; }

            /// <summary>
            /// Gets or sets the password used to log into the PostgreSQL server.
            /// </summary>
            [YamlMember(Alias = "password", Description = "The password used for login.")]
            public string Password { get; set; }

            /// <summary>
            /// Gets or sets the name of the table in the database from which data should be retrieved.
            /// </summary>
            [YamlMember(Alias = "tableName", Description = "The name of the table in the database to get the data from.")]
            public string TableName { get; set; } = String.Empty;

            /// <summary>
            /// Initializes a new instance of the <see cref="PostgresConfiguration"/> class using the provided parameters.
            /// </summary>
            /// <param name="server">The IP address or hostname of the PostgreSQL server.</param>
            /// <param name="port">The port number on which the PostgreSQL server is listening.</param>
            /// <param name="database">The name of the database on the PostgreSQL server.</param>
            /// <param name="user">The username used to authenticate to the PostgreSQL server.</param>
            /// <param name="password">The password associated with the specified username for authentication.</param>
            public PostgresConfiguration(string server, string port, string database, string user, string password)
            {
                Server = server;
                Port = port;
                Database = database;
                User = user;
                Password = password;
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="PostgresConfiguration"/> class with default values.
            /// </summary>
            public PostgresConfiguration()
            {
            }

            /// <summary>
            /// Provides an empty instance of <see cref="PostgresConfiguration"/> where all properties are set to empty strings.
            /// Useful for scenarios where a default or null configuration is required.
            /// </summary>
            public static PostgresConfiguration Empty => new PostgresConfiguration(String.Empty, String.Empty, String.Empty, String.Empty, String.Empty);

            /// <summary>
            /// Converts the current configuration into a valid PostgreSQL connection string.
            /// </summary>
            /// <example>
            /// For example: "Server=localhost;Port=5432;Database=mydatabase;User Id=myusername;Password=mypassword;"
            /// </example>
            /// <returns>The constructed PostgreSQL connection string based on the current configuration.</returns>
            public string ToPostgresConnectionString()
            {
                return $"Server={Server};Port={Port};Database={Database};User Id={User};Password={Password};";
            }
        }
    }   
}
