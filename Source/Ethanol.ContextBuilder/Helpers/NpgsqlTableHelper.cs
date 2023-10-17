using Npgsql;

namespace Ethanol.ContextBuilder.Helpers
{
    /// <summary>
    /// Provides utility methods for performing operations on PostgreSQL tables using the Npgsql database connector.
    /// </summary>
    internal static class NpgsqlTableHelper
    {
        /// <summary>
        /// Checks if a specified table exists within the connected PostgreSQL database.
        /// </summary>
        /// <param name="connection">The Npgsql database connection.</param>
        /// <param name="tableName">The name of the table to check.</param>
        /// <returns><c>true</c> if the table exists; otherwise, <c>false</c>.</returns>
        internal static bool TableExists(this NpgsqlConnection connection, string tableName)
        {
            using var sqlcmd = connection.CreateCommand();
            sqlcmd.CommandText = $@"SELECT EXISTS(SELECT FROM information_schema.tables WHERE table_name = '{tableName}');";
            return (bool)sqlcmd.ExecuteScalar();
        }

        /// <summary>
        /// Creates a table in the connected PostgreSQL database.
        /// </summary>
        /// <param name="connection">The Npgsql database connection.</param>
        /// <param name="tableName">The name of the table to create.</param>
        /// <param name="columns">An array of SQL column definitions.</param>
        /// <returns><c>true</c> if the table was successfully created or already exists; otherwise, <c>false</c>.</returns>
        internal static bool CreateTable(this NpgsqlConnection connection, string tableName, params string[] columns)
        {
            using var cmd = connection.CreateCommand();
            cmd.CommandText = @$"CREATE TABLE IF NOT EXISTS {tableName} ( {string.Join(',', columns)} );";
            return cmd.ExecuteNonQuery() > 0;
        }

        /// <summary>
        /// Drops a specified table from the connected PostgreSQL database.
        /// </summary>
        /// <param name="connection">The Npgsql database connection.</param>
        /// <param name="tableName">The name of the table to drop.</param>
        /// <returns><c>true</c> if the table was successfully dropped or doesn't exist; otherwise, <c>false</c>.</returns>
        internal static bool DropTable(this NpgsqlConnection connection, string tableName)
        {
            using var cmd = connection.CreateCommand();
            cmd.CommandText = $"DROP TABLE IF EXISTS {tableName}";
            return cmd.ExecuteNonQuery() > 0;
        }

        /// <summary>
        /// Generates an index name based on the provided table and column names.
        /// </summary>
        /// <param name="tableName">The table name.</param>
        /// <param name="column">The column name.</param>
        /// <returns>The generated index name.</returns>
        internal static string GetIndexName(string tableName, string column)
        {
            return $"{tableName}_{column}_idx";
        }

        /// <summary>
        /// Checks if a specified index exists on a table within the connected PostgreSQL database.
        /// </summary>
        /// <param name="connection">The Npgsql database connection.</param>
        /// <param name="tableName">The table name.</param>
        /// <param name="column">The column name associated with the index.</param>
        /// <returns><c>true</c> if the index exists; otherwise, <c>false</c>.</returns>
        internal static bool IndexExists(this NpgsqlConnection connection, string tableName, string column)
        {
            var indexName = GetIndexName(tableName, column);
            using var cmd = connection.CreateCommand();
            cmd.CommandText = $@"SELECT EXISTS(SELECT indexname FROM pg_indexes WHERE tablename = '{tableName}' AND indexname = '{indexName}');";
            return (bool)cmd.ExecuteScalar();

        }
        /// <summary>
        /// Creates an index on a specified column of a table within the connected PostgreSQL database.
        /// </summary>
        /// <param name="connection">The Npgsql database connection.</param>
        /// <param name="tableName">The table name.</param>
        /// <param name="column">The column name on which to create the index.</param>
        /// <returns><c>true</c> if the index was successfully created or already exists; otherwise, <c>false</c>.</returns>
        internal static bool CreateIndex(this NpgsqlConnection connection, string tableName, string column)
        {
            var indexName = GetIndexName(tableName, column);
            using var cmd = connection.CreateCommand();
            cmd.CommandText = @$"CREATE INDEX IF NOT EXISTS {indexName} ON {tableName} ({column})";
            return cmd.ExecuteNonQuery() > 0;
        }
        /// <summary>
        /// Deletes rows from a table based on a specified SQL WHERE clause.
        /// </summary>
        /// <param name="connection">The Npgsql database connection.</param>
        /// <param name="tableName">The table name.</param>
        /// <param name="whereClause">The SQL WHERE clause to determine which rows to delete.</param>
        /// <returns>The number of rows affected by the delete operation.</returns>
        internal static int Delete(NpgsqlConnection connection, string tableName, string whereClause)
        {
            using var cmd = connection.CreateCommand();
            cmd.CommandText = @$"DELETE FROM {tableName} WHERE {whereClause}";
            return cmd.ExecuteNonQuery();
        }
    }
}
