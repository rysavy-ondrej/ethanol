using Ethanol.ContextBuilder;
using Ethanol.ContextBuilder.Polishers;
using FastEndpoints;
using Microsoft.Extensions.Options;
using Npgsql;
using NpgsqlTypes;
using System.Data;
using System.Text.Json;

namespace Ethanol.ContextProvider.Endpoints
{

    /// <summary>
    /// Provides an endpoint to retrieve a list of host-contexts based on the specified time window and IP address criteria.
    /// </summary>
    /// <remarks>
    /// The endpoint listens to GET requests on "/api/v1/host-context/contexts" and returns a list of matching host-contexts.
    /// </remarks>
    [HttpGet("/api/v1/host-context/contexts")]
    public class ContextsEndpoint : Endpoint<ContextsQuery, List<HostContext>>
    {
        static protected readonly ILogger __logger = LogManager.GetCurrentClassLogger();

        private readonly NpgsqlDataSource _datasource;
        private readonly EthanolConfiguration _configuration;

        public ContextsEndpoint(NpgsqlDataSource datasource, IOptions<EthanolConfiguration> configuration)
        {
            _datasource = datasource;
            _configuration = configuration.Value;
        }

        /// <summary>
        /// Handles incoming requests and responds with a list of host-contexts based on the provided query parameters.
        /// </summary>
        /// <param name="query">The query parameters specifying time window and IP address.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>Asynchronous task signalizing the completion of the operation.</returns>
        public override async Task HandleAsync(ContextsQuery query, CancellationToken ct)
        {
            __logger.LogInformation($"Endpoint '{nameof(ContextsEndpoint)}' received requests '{query}'.");
            try
            {
                var rowList = new List<HostContext>();
                using var connection = _datasource.OpenConnection();
                __logger.LogInformation($"Using connection: `{connection.ConnectionString}` to access database.");

                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = $"SELECT * FROM \"{_configuration.HostContextTable}\" WHERE {query.GetWhereExpression()}";
                    using var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        var row = ReadRow(reader);
                        rowList.Add(row);
                    }
                    reader.Close();
                }
                // collect relevant tags to the context:
                var tagsProcessor = new TagsProcessor(connection, _configuration.TagsTable);
                foreach (var row in rowList)
                {
                    var tags = tagsProcessor.ReadTagObjects(row.Key, row.Start, row.End);
                    row.Tags = tagsProcessor.ComputeCompactTags(tags);
                }

                await SendAsync(rowList, 200, ct);
            }
            catch (Exception ex)
            {
                __logger.LogError(ex, $"Endpoint '{nameof(ContextsEndpoint)}' cannot create a response for the query {0}.", query);
                await SendErrorsAsync(500, ct);
            }
        }

        private HostContext ReadRow(NpgsqlDataReader reader)
        {
            var id = reader.GetInt64("id");
            var key = reader.GetString("key");
            var validity = reader.GetFieldValue<NpgsqlRange<DateTime>>("validity");
            var initiatedConnections = JsonSerializer.Deserialize<IpConnectionInfo[]>(reader.GetString("initiatedconnections"));
            var acceptedConnections = JsonSerializer.Deserialize<IpConnectionInfo[]>(reader.GetString("acceptedconnections"));
            var resolvedDomains = JsonSerializer.Deserialize<ResolvedDomainInfo[]>(reader.GetString("resolveddomains"));
            var webUrls = JsonSerializer.Deserialize<WebRequestInfo[]>(reader.GetString("weburls"));
            var tlsHandshakes = JsonSerializer.Deserialize<TlsHandshakeInfo[]>(reader.GetString("tlshandshakes"));
            return new HostContext(id, key, validity.LowerBound, validity.UpperBound,
                initiatedConnections ?? Array.Empty<IpConnectionInfo>(),
                acceptedConnections ?? Array.Empty<IpConnectionInfo>(),
                resolvedDomains ?? Array.Empty<ResolvedDomainInfo>(),
                webUrls ?? Array.Empty<WebRequestInfo>(),
                tlsHandshakes ?? Array.Empty<TlsHandshakeInfo>());
        }
    }
}