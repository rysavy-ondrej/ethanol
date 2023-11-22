﻿using Ethanol.ContextBuilder;
using Ethanol.ContextBuilder.Polishers;
using FastEndpoints;
using Microsoft.Extensions.Configuration;
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
        private readonly ILogger _logger;
        private readonly NpgsqlDataSource _datasource;
        private readonly EthanolConfiguration _configuration;

        public ContextsEndpoint(NpgsqlDataSource datasource, EthanolConfiguration configuration, ILogger logger)
        {
            _datasource = datasource;
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Handles incoming requests and responds with a list of host-contexts based on the provided query parameters.
        /// </summary>
        /// <param name="query">The query parameters specifying time window and IP address.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>Asynchronous task signalizing the completion of the operation.</returns>
        public override async Task HandleAsync(ContextsQuery query, CancellationToken ct)
        {
            _logger?.LogInformation($"Endpoint '{nameof(ContextsEndpoint)}' received requests '{query}'.");
            try
            {
                var hostContexts = new List<HostContext>();
                using var connection = _datasource.OpenConnection();
                _logger?.LogInformation($"Using connection: `{connection.ConnectionString}` to access database.");

                using (var cmd = connection.CreateCommand())
                {
                    
                    cmd.CommandText = $"SELECT * FROM \"{_configuration.HostContextTable}\" WHERE {query.GetWhereExpression()}";
                    _logger?.LogTrace($"Execute command: {cmd.CommandText}");

                    using var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        var row = ReadRow(reader);
                        
                        _logger?.LogTrace($"Add context: id={row.Id}, key={row.Key}");

                        hostContexts.Add(row);
                    }
                    reader.Close();
                }

                var tagsProcessor = new TagsProcessor(connection, _configuration.TagsTable, _logger);
                // group context by their windows:
                var windows = hostContexts.GroupBy(r => (Start: r.Start, End: r.End));
                foreach (var window in windows)
                {
                    _logger?.LogTrace($"Processing window: start={window.Key.Start}, end={window.Key.End}");
                    foreach (var chunk in window.Chunk(_configuration.TagsChunkSize))
                    {
                        _logger?.LogTrace($"  Processing chunk: size={chunk.Length}");                        
                        var tags = tagsProcessor.ReadTagObjects(chunk.Select(c => c.Key), window.Key.Start, window.Key.End);
                        foreach (var ctx in chunk)
                        {
                            var ctxTags = tags.Where(t => t.Key == ctx.Key).ToArray();
                            _logger?.LogTrace($"  Compactimg tags: ctx-id: {ctx.Id}, ctx-key={ctx.Key}, tags={ctxTags.Length}");
                            ctx.Tags = tagsProcessor.ComputeCompactTags(ctxTags);
                        }
                    }
                }
                await SendAsync(hostContexts, 200, ct);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Endpoint '{nameof(ContextsEndpoint)}' cannot create a response for the query {0}.", query);
                await SendErrorsAsync(500, ct);
            }
        }

        private HostContext ReadRow(NpgsqlDataReader reader)
        {
            var id = reader.GetInt64("id");
            var key = reader.GetString("key");
            var validity = reader.GetFieldValue<NpgsqlRange<DateTime>>("validity");
            var connections = JsonSerializer.Deserialize<IpConnectionInfo[]>(reader.GetString("connections"));
            var resolvedDomains = JsonSerializer.Deserialize<ResolvedDomainInfo[]>(reader.GetString("resolveddomains"));
            var webUrls = JsonSerializer.Deserialize<WebRequestInfo[]>(reader.GetString("weburls"));
            var tlsHandshakes = JsonSerializer.Deserialize<TlsHandshakeInfo[]>(reader.GetString("tlshandshakes"));
            return new HostContext(id, key, validity.LowerBound, validity.UpperBound,
                connections ?? Array.Empty<IpConnectionInfo>(),
                resolvedDomains ?? Array.Empty<ResolvedDomainInfo>(),
                webUrls ?? Array.Empty<WebRequestInfo>(),
                tlsHandshakes ?? Array.Empty<TlsHandshakeInfo>());
        }
    }
}