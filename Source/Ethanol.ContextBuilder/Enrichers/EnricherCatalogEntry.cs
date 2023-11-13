using Ethanol.ContextBuilder.Context;
using Ethanol.ContextBuilder.Enrichers;
using Ethanol.ContextBuilder.Observable;
using Npgsql;

namespace Ethanol.Catalogs
{
    /// <summary>
    /// Provides extension methods for the ContextTransformCatalog to create various enricher instances.
    /// </summary>
    public static class EnricherCatalogEntry
    {
        /// <summary>
        /// Creates an IP host context enricher using a Postgres database as the tag source.
        /// </summary>
        /// <param name="catalog">The context transform catalog to which this enricher will be added.</param>
        /// <param name="connectionString">The connection string to the Postgres database.</param>
        /// <param name="sourceTable">The name of the table in the Postgres database to be used as the tag source.</param>
        /// <returns>
        /// An observable transformer that takes an observable event of IpHostContext and transforms it into an observable event
        /// of IpHostContextWithTags using tags obtained from the specified Postgres database.
        /// </returns>
        public static IObservableTransformer<ObservableEvent<IpHostContext>, ObservableEvent<IpHostContextWithTags>> GetNetifyPostgresEnricher(this ContextTransformCatalog catalog, string connectionString, string sourceTable)
        {
            var connection = new NpgsqlConnection(connectionString);
            connection.Open();
            return new IpHostContextEnricher(new NetifyTagProvider(new PostgresTagDataSource(connection, sourceTable, catalog.Environment.Logger)), catalog.Environment.Logger);
        }

        /// <summary>
        /// Creates a void enricher that passes through IP host context without adding any tags.
        /// </summary>
        /// <param name="catalog">The context transform catalog to which this enricher will be added.</param>
        /// <returns>
        /// An observable transformer that takes an observable event of IpHostContext and transforms it into an observable event
        /// of IpHostContextWithTags without adding or modifying the tags.
        /// </returns>
        public static IObservableTransformer<ObservableEvent<IpHostContext>, ObservableEvent<IpHostContextWithTags>> GetVoidEnricher(this ContextTransformCatalog catalog)
        {
            return new VoidContextEnricher<ObservableEvent<IpHostContext>, ObservableEvent<IpHostContextWithTags>>(s => 
                new ObservableEvent<IpHostContextWithTags>(
                    new IpHostContextWithTags { HostAddress = s.Payload.HostAddress, Flows = s.Payload.Flows }, s.StartTime, s.EndTime));
        }
    }
}
