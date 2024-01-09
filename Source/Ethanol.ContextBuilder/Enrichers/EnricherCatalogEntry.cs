using System;
using Ethanol.ContextBuilder.Enrichers;
using Ethanol.DataObjects;
using Npgsql;
namespace Ethanol.Catalogs
{
    /// <summary>
    /// Provides extension methods for the ContextTransformCatalog to create various enricher instances.
    /// </summary>
    public static class EnricherCatalogEntry
    {
        public static IEnricher<TimeRange<IpHostContext>, TimeRange<IpHostContextWithTags>> GetNetifyPostgresEnricher2(this ContextBuilderCatalog catalog, string connectionString, string sourceTable)
        {
            var connection = new NpgsqlConnection(connectionString);
            if (connection == null || connection.State != System.Data.ConnectionState.Open) throw new Exception("Connection is not open");
            connection.Open();
            return new IpHostContextEnricher(new NetifyTagProvider(new PostgresTagDataSource(connection, sourceTable, catalog.Environment.Logger)), null, catalog.Environment.Logger);
        }  

        public static IEnricher<TimeRange<IpHostContext>, TimeRange<IpHostContextWithTags>> GetVoidEnricher2(this ContextBuilderCatalog catalog)
        {
            return new VoidContextEnricher<TimeRange<IpHostContext>?, TimeRange<IpHostContextWithTags>>(p => new TimeRange<IpHostContextWithTags>(new IpHostContextWithTags { HostAddress = p.Value?.HostAddress, Flows = p.Value?.Flows, Tags = Array.Empty<TagObject>() }, p.StartTime, p.EndTime));
        }
    }
}