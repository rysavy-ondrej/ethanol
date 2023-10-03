using Npgsql;
using System;

namespace Ethanol.ContextBuilder.Enrichers
{
    public static class NpgsqlTableHelper
    { 
        public static bool TableExists(this NpgsqlConnection connection, string tableName)
        {
            using var sqlcmd = connection.CreateCommand();
            sqlcmd.CommandText = $@"SELECT EXISTS(SELECT FROM information_schema.tables WHERE table_name = '{tableName}');";
            return (bool)sqlcmd.ExecuteScalar();
        }

        public static bool CreateTable(this NpgsqlConnection connection, string tableName, params string[] columns)
        {
            using var cmd = connection.CreateCommand();
            cmd.CommandText = @$"CREATE TABLE IF NOT EXISTS {tableName} ( {String.Join(',', columns)} );";
            return cmd.ExecuteNonQuery() > 0;
        }

        public static bool DropTable(this NpgsqlConnection connection, string tableName)
        {
            using var cmd = connection.CreateCommand();
            cmd.CommandText = $"DROP TABLE IF EXISTS {tableName}";
            return cmd.ExecuteNonQuery() > 0;
        }

        private static string GetIndexName(string tableName, string column)
        {
            return $"{tableName}_{column}_idx";
        }

        public static bool IndexExists(this NpgsqlConnection connection, string tableName, string column)
        {
            var indexName = GetIndexName(tableName, column);
            using var cmd = connection.CreateCommand();
            cmd.CommandText = $@"SELECT EXISTS(SELECT indexname FROM pg_indexes WHERE tablename = '{tableName}' AND indexname = '{indexName}');";
            return (bool)cmd.ExecuteScalar();

        }
        public static bool CreateIndex(this NpgsqlConnection connection, string tableName, string column)
        {
            var indexName = GetIndexName(tableName, column);
            using var cmd = connection.CreateCommand();
            cmd.CommandText = @$"CREATE INDEX IF NOT EXISTS {indexName} ON {tableName} ({column})";
            return cmd.ExecuteNonQuery() > 0;
        }


    }
}
