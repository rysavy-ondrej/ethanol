using ConfigurationSubstitution;
using Microsoft.Extensions.Configuration;
using System.Text.Json.Serialization;


public class ContextBuilderConfiguration
{
    internal static Root LoadFromFile(string configurationFilePath)
    {
        var configurationBuilder = new ConfigurationBuilder();
        var configuration = configurationBuilder.AddJsonFile(configurationFilePath).AddEnvironmentVariables().EnableSubstitutions("${", "}", UnresolvedVariableBehaviour.IgnorePattern).Build();

        var contextBuilderConfiguration = new Root();
        configuration.Bind(contextBuilderConfiguration);
        return contextBuilderConfiguration;
    }

    // Root myDeserializedClass = JsonSerializer.Deserialize<Root>(myJsonResponse);
    public class ContextBuilder
    {
        [JsonPropertyName("windowsize")]
        public string WindowSize { get; set; }

        [JsonPropertyName("windowhop")]
        public string WindowHop { get; set; }

        [JsonPropertyName("networks")]
        public List<string> Networks { get; set; }
    }

    public class Input
    {
        [JsonPropertyName("tcp")]
        public Tcp Tcp { get; set; }

        [JsonPropertyName("stdin")]
        public Stdin Stdin { get; set; }
    }

    public class Output
    {
        [JsonPropertyName("postgres")]
        public Postgres Postgres { get; set; }

        [JsonPropertyName("stdout")]
        public Stdout Stdout { get; set; }
    }

    public class Postgres
    {
        [JsonPropertyName("server")]
        public string Server { get; set; }

        [JsonPropertyName("port")]
        public string Port { get; set; }

        [JsonPropertyName("database")]
        public string Database { get; set; }

        [JsonPropertyName("user")]
        public string User { get; set; }

        [JsonPropertyName("password")]
        public string Password { get; set; }

        [JsonPropertyName("tablename")]
        public string TableName { get; set; }

        public string GetConnectionString()
        {
            return $"Server={Server};Port={Port};Database={Database};User Id={User};Password={Password};";
        }
    }

    public class Root
    {
        [JsonPropertyName("input")]
        public Input Input { get; set; }

        [JsonPropertyName("builder")]
        public ContextBuilder Builder { get; set; }

        [JsonPropertyName("enricher")]
        public Enrichers Enrichers { get; set; }

        [JsonPropertyName("output")]
        public Output Output { get; set; }
    }
    public class Enrichers
    {
        [JsonPropertyName("netify")]
        public NetifyEnricher Netify { get; set; }
    }


    public class NetifyEnricher
    {
        [JsonPropertyName("postgres")]
        public Postgres Postgres { get; set; }
    }

    public class Stdin
    {
        [JsonPropertyName("format")]
        public string Format { get; set; }
    }

    public class Stdout
    {
        [JsonPropertyName("format")]
        public string Format { get; set; }
    }

    public class Tcp
    {
        [JsonPropertyName("listen")]
        public string Listen { get; set; }

        [JsonPropertyName("port")]
        public string Port { get; set; }

        [JsonPropertyName("format")]
        public string Format { get; set; }
    }
}
