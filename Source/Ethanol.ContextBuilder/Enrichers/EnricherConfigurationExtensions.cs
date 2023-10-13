using System;
using Ethanol.ContextBuilder.Context;
using static Ethanol.ContextBuilder.Enrichers.IpHostContextEnricherPlugin;

namespace Ethanol.ContextBuilder.Enrichers
{
    public static class EnricherConfigurationExtensions
    {
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
