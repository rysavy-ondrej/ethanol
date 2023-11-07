using Ethanol.Catalogs;
using Ethanol.ContextBuilder.Observable;
using Ethanol.ContextBuilder.Polishers;
using Npgsql;

namespace Ethanol.ContextBuilder.Writers
{
    public static class PostgresContextWriterCatalogEntry
    {
        public static ContextWriter<ObservableEvent<IpTargetHostContext>> GetPostgresWriter(this ContextWriterCatalog catalog, NpgsqlConnection connection, string tableName)
        {
            return new PostgresTargetHostContextWriter(connection, tableName, catalog.Environment.Logger);
        }
    }
}
