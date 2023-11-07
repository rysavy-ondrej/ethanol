using Ethanol.ContextBuilder.Enrichers;
using Ethanol.ContextBuilder.Enrichers.TagProviders;
using Npgsql;

namespace Ethanol.Catalogs
{
    public static class IpHostContextEnricherCatalogItems
    {
        public static IpHostContextEnricher GetNetifyPostgresEnricher(this ContextEnricherCatalog catalog, string connectionString, string sourceTable)
        {
            var connection = new NpgsqlConnection(connectionString);
            connection.Open();
            return new IpHostContextEnricher(new PostgresTagProvider(connection, sourceTable, catalog.Environment.Logger), catalog.Environment.Logger);
        }
    }
}
