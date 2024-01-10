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
        /// <summary>
        /// Retrieves an enricher that uses a PostgreSQL database as a data source for tagging IP host contexts.
        /// </summary>
        /// <param name="catalog">The ContextBuilderCatalog instance.</param>
        /// <param name="connectionString">The connection string for the PostgreSQL database.</param>
        /// <param name="sourceTable">The name of the table in the PostgreSQL database that contains the source data.</param>
        /// <returns>An instance of IEnricher&lt;TimeRange&lt;IpHostContext&gt;, TimeRange&lt;IpHostContextWithTags&gt;&gt;.</returns>
        public static IEnricher<TimeRange<IpHostContext>, TimeRange<IpHostContextWithTags>> GetNetifyPostgresEnricher(this ContextBuilderCatalog catalog, string connectionString, string sourceTable)
        {
            var connection = new NpgsqlConnection(connectionString) ?? throw new Exception("Connection is null");
            connection.Open();
            if (connection.State != System.Data.ConnectionState.Open) throw new Exception("Connection is not open");            
            var tagProvider = new NetifyTagProvider(new PostgresTagDataSource(connection, sourceTable, catalog.Environment.Logger));
            return new IpHostContextEnricher(tagProvider!, null, catalog.Environment.Logger);
        }  

        /// <summary>
        /// Retrieves a void enricher from the catalog.
        /// </summary>
        /// <param name="catalog">The ContextBuilderCatalog instance.</param>
        /// <returns>The void enricher.</returns>
        public static IEnricher<TimeRange<IpHostContext>, TimeRange<IpHostContextWithTags>> GetVoidEnricher(this ContextBuilderCatalog catalog)
        {
            return new VoidContextEnricher<TimeRange<IpHostContext>, TimeRange<IpHostContextWithTags>>(p => new TimeRange<IpHostContextWithTags>(new IpHostContextWithTags { HostAddress = p.Value?.HostAddress, Flows = p.Value?.Flows, Tags = Array.Empty<TagObject>() }, p.StartTime, p.EndTime));
        }
        /// <summary>
        /// Retrieves an enricher that uses Netify data tanker for enriching the IP host context.
        /// </summary>
        /// <param name="catalog">The ContextBuilderCatalog instance.</param>
        /// <param name="dbPath">The path to the data tanker database.</param>
        /// <param name="collectionName">The name of the collection in the data tanker database.</param>
        /// <returns>An instance of IEnricher&lt;TimeRange&lt;IpHostContext&gt;, TimeRange&lt;IpHostContextWithTags&gt;&gt;.</returns>
        public static IEnricher<TimeRange<IpHostContext>, TimeRange<IpHostContextWithTags>> GetNetifyLiteDatabaseEnricher(this ContextBuilderCatalog catalog, string dbPath)
        {
            return new IpHostContextEnricher(new NetifyTagProvider(new LiteDatabaseTagDataSource(dbPath, catalog.Environment.Logger)), null, catalog.Environment.Logger);
        }
    }
}