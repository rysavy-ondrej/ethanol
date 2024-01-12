using FastEndpoints;
using Microsoft.Extensions.Options;
using Npgsql;

namespace Ethanol.ContextProvider.Endpoints
{
    /// <summary>
    /// Provides an endpoint to retrieve the count of host-contexts based on the specified time window and IP address criteria.
    /// </summary>
    /// <remarks>
    /// The endpoint listens to GET requests on "/api/v1/host-context/contexts/count" and returns the count of matching host-contexts.
    /// </remarks>
    [HttpGet("/api/v1/host-context/contexts/count")]
    public class ContextsCountEndpoint : Endpoint<ContextsQuery, int>
    {
        private readonly ILogger _logger;
        private readonly NpgsqlDataSource _dataSource;
        private readonly EthanolConfiguration _configuration;

        public ContextsCountEndpoint(NpgsqlDataSource datasource, EthanolConfiguration configuration, ILogger logger)
        {
            _dataSource = datasource;
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Handles incoming requests and responds with the count of host-contexts based on the provided query parameters.
        /// </summary>
        /// <param name="query">The query parameters specifying time window and IP address.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>Asynchronous task signalizing the completion of the operation.</returns>
        public override async Task HandleAsync(ContextsQuery query, CancellationToken ct)
        {
            try
            {
                using var conn = _dataSource.OpenConnection();
                using var cmd = conn.CreateCommand();
                var whereExpr = query.GetWhereExpression();
                cmd.CommandText = $"SELECT COUNT(*) FROM \"public\".\"{_configuration.HostContextTable}\" WHERE {whereExpr}";

                var result = await cmd.ExecuteScalarAsync(ct);
                var count = int.TryParse(result?.ToString(), out var val) ? val : 0;
                await SendAsync(count, 200, ct);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Endpoint '{nameof(ContextsCountEndpoint)}' cannot create a response for the query {0}.", query);
                await SendErrorsAsync(500, ct);
            }
        }
    }
}