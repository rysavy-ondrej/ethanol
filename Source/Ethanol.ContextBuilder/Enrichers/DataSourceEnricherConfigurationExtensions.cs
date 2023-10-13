using System;
using static Ethanol.ContextBuilder.Enrichers.IpHostContextEnricherPlugin;

namespace Ethanol.ContextBuilder.Enrichers
{
    public static class DataSourceEnricherConfigurationExtensions
    {
        public static ITagDataProvider<TagRecord> GetTagProvider(this DataSourceEnricherConfiguration config)
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
