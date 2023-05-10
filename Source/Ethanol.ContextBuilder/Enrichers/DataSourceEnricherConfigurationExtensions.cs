using System;
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
        public static IHostDataProvider<NetifyTag> GetNetifyTagProvider(this DataSourceEnricherConfiguration config)
        {
            if (config?.Postgres != null)
            {
                var tableNames = config.Postgres.TableName?.Split(',',';');
                var appTableName = tableNames?.Length > 0 && !String.IsNullOrWhiteSpace(tableNames[0]) ? tableNames[0] : "netify_applications";
                var ipsTableName = tableNames?.Length > 1 && !String.IsNullOrWhiteSpace(tableNames[1]) ? tableNames[1] : "netify_addresses";
                var postgres = PostgresNetifyTagProvider.Create(config.Postgres.ToPostgresConnectionString(), appTableName, ipsTableName);
                return postgres;
            }
            if (config?.Json != null)
            {
                var json = JsonDbNetifyTagProvider.Create(config.Json);
                return json;
            }

            return null;
        }
    }

}
