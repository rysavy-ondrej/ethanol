using Ethanol.Catalogs;
using Ethanol.DataObjects;
using Npgsql;

namespace Ethanol.ContextBuilder.Writers
{
    public static class PostgresContextWriterCatalogEntry
    {
        public static ContextWriter<HostContext> GetPostgresWriter(this ContextWriterCatalog catalog, NpgsqlConnection connection, string tableName, int chunkSize=1000)
        {
            return new PostgresTargetHostContextWriter(connection, tableName, chunkSize,catalog.Environment.Logger);
        }
    }
}
