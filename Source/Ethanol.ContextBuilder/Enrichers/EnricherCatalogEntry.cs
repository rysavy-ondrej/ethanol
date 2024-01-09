using System;
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
        public static IEnricher<ObservableEvent<IpHostContext>, ObservableEvent<IpHostContextWithTags>> GetNetifyPostgresEnricher2(this ContextTransformCatalog catalog, string connectionString, string sourceTable)
        {
            var connection = new NpgsqlConnection(connectionString);
            if (connection == null || connection.State != System.Data.ConnectionState.Open) throw new Exception("Connection is not open");
            connection.Open();
            return new IpContextEnricher(new NetifyTagProvider(new PostgresTagDataSource(connection, sourceTable, catalog.Environment.Logger)), null, catalog.Environment.Logger);
        }  

        public static IEnricher<ObservableEvent<IpHostContext>, ObservableEvent<IpHostContextWithTags>> GetVoidEnricher2(this ContextTransformCatalog catalog)
        {
            return new VoidContextEnricher<ObservableEvent<IpHostContext>?, ObservableEvent<IpHostContextWithTags>>(p => new ObservableEvent<IpHostContextWithTags>(new IpHostContextWithTags { HostAddress = p.Payload?.HostAddress, Flows = p.Payload?.Flows, Tags = Array.Empty<TagObject>() }, p.StartTime, p.EndTime));
        }
    }
}