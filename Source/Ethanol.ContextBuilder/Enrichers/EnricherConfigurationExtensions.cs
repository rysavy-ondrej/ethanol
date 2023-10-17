using System;
using Ethanol.ContextBuilder.Context;
using Ethanol.ContextBuilder.Enrichers.TagProviders;
using static Ethanol.ContextBuilder.Enrichers.IpHostContextEnricherPlugin;

namespace Ethanol.ContextBuilder.Enrichers
{
    /// <summary>
    /// Provides extension methods for the <see cref="EnricherConfiguration"/> class.
    /// </summary>
    public static class EnricherConfigurationExtensions
    {
        /// <summary>
        /// Retrieves a tag provider based on the specified configuration.
        /// </summary>
        /// <param name="config">The enricher configuration containing either PostgreSQL or JSON data source settings.</param>
        /// <returns>
        /// An instance of <see cref="ITagDataProvider{TagObject}"/>. 
        /// Returns a <see cref="PostgresTagProvider"/> if the configuration specifies a PostgreSQL source,
        /// a <see cref="JsonDbTagProvider"/> if the configuration specifies a JSON source,
        /// or null if neither configuration is provided.
        /// </returns>
        public static ITagDataProvider<TagObject> GetTagProvider(this EnricherConfiguration config)
        {
            if (config?.Postgres != null)
            {
                var postgres = PostgresTagProvider.Create(config.Postgres.ToPostgresConnectionString(), config.Postgres.TableName);
                return postgres;
            }
            if (config?.Json != null)
            {
                var json = JsonDbTagProvider.Create(config.Json);
                return json;
            }
            return null;
        }
    }
}
