using static Ethanol.ContextBuilder.Enrichers.IpHostContextEnricherPlugin;

namespace Ethanol.ContextBuilder.Enrichers
{
    public static class DataSourceEnricherConfigurationExtensions
    {
        public static IHostDataProvider<HostTag> GetHostTagProvider(this DataSourceEnricherConfiguration config)
        {
            if (config?.Postgres != null)
            {
                var postgres = PostgresHostTagProvider.Create(config.Postgres.ToPostgresConnectionString(), config.Postgres.TableName);
                return postgres;
            }
            if (config?.Json != null)
            {
                var json = JsonDbHostTagProvider.Create(config.Json);
                return json;
            }
            return null;
        }
        public static IHostDataProvider<FlowTag> GetFlowTagProvider(this DataSourceEnricherConfiguration config)
        {
            if (config?.Postgres != null)
            {
                var postgres = PostgresFlowTagProvider.Create(config.Postgres.ToPostgresConnectionString(), config.Postgres.TableName);
                return postgres;
            }
            if (config?.Json != null)
            {
                var json = JsonDbFlowTagProvider.Create(config.Json);
                return json;
            }

            return null;
        }
    }

}
