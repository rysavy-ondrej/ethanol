using Ethanol.ContextBuilder.Context;
using Ethanol.ContextBuilder.Enrichers;
using Ethanol.ContextBuilder.Enrichers.TagProviders;
using Ethanol.ContextBuilder.Observable;
using Npgsql;
using System.Runtime.CompilerServices;

namespace Ethanol.Catalogs
{
    public static class IpHostContextEnricherCatalogEntry
    {
        public static IObservableTransformer<ObservableEvent<IpHostContext>, ObservableEvent<IpHostContextWithTags>> GetNetifyPostgresEnricher(this ContextTransformCatalog catalog, string connectionString, string sourceTable)
        {
            var connection = new NpgsqlConnection(connectionString);
            connection.Open();
            return new IpHostContextEnricher(new NetifyTagProvider(new PostgresTagDataSource(connection, sourceTable, catalog.Environment.Logger)), catalog.Environment.Logger);
        }

        public static IObservableTransformer<ObservableEvent<IpHostContext>, ObservableEvent<IpHostContextWithTags>> GetVoidEnricher(this ContextTransformCatalog catalog)
        {
            return new VoidContextEnricher<ObservableEvent<IpHostContext>, ObservableEvent<IpHostContextWithTags>>(s => 
                new ObservableEvent<IpHostContextWithTags>(
                    new IpHostContextWithTags { HostAddress = s.Payload.HostAddress, Flows = s.Payload.Flows }, s.StartTime, s.EndTime));
        }
    }
}
