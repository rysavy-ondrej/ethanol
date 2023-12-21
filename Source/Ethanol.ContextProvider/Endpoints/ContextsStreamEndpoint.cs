using FastEndpoints;
using Npgsql;
using NpgsqlTypes;
using System.Data;
using System.Text.Json;
using Ethanol.DataObjects;
using Ethanol.ContextBuilder.Serialization;

namespace Ethanol.ContextProvider.Endpoints
{
    /// <summary>
    /// Provides an endpoint to retrieve a list of host-contexts based on the specified time window and IP address criteria.
    /// <remarks>
    /// This enpoint should provide the similar response "/api/v1/host-context/contexts" but it is more suitable for
    /// a large data as it processes and returns data in chunks.
    /// </summary>
    /// <remarks>
    /// The endpoint listens to GET requests on "/api/v1/host-context/context-stream" and returns a list of matching host-contexts.
    /// </remarks>
    [HttpGet("/api/v1/host-context/context-stream")]
    public class ContextsStreamEndpoint : Endpoint<ContextsQuery, List<HostContext>>
    {
        private readonly ILogger? _logger;
        private readonly NpgsqlDataSource _datasource;
        private readonly string _hostContextTable;
        private readonly string _tagsTableName;
        private readonly int _tagsChunkSize;

        public ContextsStreamEndpoint(NpgsqlDataSource datasource, EthanolConfiguration configuration, ILogger? logger)
        {
            _datasource = datasource ?? throw new ArgumentNullException(nameof(datasource));
            _hostContextTable = configuration?.HostContextTable ?? throw new ArgumentNullException(nameof(configuration));
            _tagsTableName = configuration?.TagsTable ?? throw new ArgumentNullException(nameof(configuration));
            _tagsChunkSize = configuration?.TagsChunkSize ?? throw new ArgumentNullException(nameof(configuration));
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
                using var connection = _datasource.OpenConnection();
                var tagsProcessor = new TagsProcessor(_datasource.OpenConnection(), _tagsTableName, _logger);

                _logger?.LogInformation($"Using connection: `{connection.ConnectionString}` to access database.");
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = $"SELECT * FROM \"{_hostContextTable}\" WHERE {query.GetWhereExpression()} ORDER BY validity ASC";
                    _logger?.LogInformation($"Fetching context objects {query.GetWhereExpression()} from {_hostContextTable} table.");

                    using var reader = cmd.ExecuteReader();
                    while (true)
                    {
                        var chunk = ReadNextChunk(reader, out var start, out var end);
                        if (chunk.Count == 0) break; // no more data to read

                        _logger?.LogInformation($"Processing context chunk: host_count={chunk.Count}, validity_from={start}, validity_to={end}");
                        var contexts = FetchTags(tagsProcessor, chunk, start, end);
                        foreach (var ctx in contexts)
                        {
                            var contextJson = Json.Serialize(ctx);
                            await HttpContext.Response.WriteAsync($"{contextJson}\n", ct);
                        }
                        await HttpContext.Response.Body.FlushAsync(ct); // Flush the data to the client
                    }
                    reader.Close();
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Endpoint '{nameof(ContextsEndpoint)}' cannot create a response for the query '{query}'.");
                await SendErrorsAsync(500, ct);
            }
        }
        /// <summary>
        /// Reads the next chunk of data from the NpgsqlDataReader and returns a list of HostContext objects.
        /// </summary>
        /// <param name="reader">The NpgsqlDataReader to read data from.</param>
        /// <param name="start">The start DateTime of the chunk.</param>
        /// <param name="end">The end DateTime of the chunk.</param>
        /// <returns>A list of HostContext objects representing the chunk of data.</returns>
        List<HostContext> ReadNextChunk(NpgsqlDataReader reader, out DateTime start, out DateTime end)
        {
            var chunk = new List<HostContext>();
            while (reader.Read() && chunk.Count < _tagsChunkSize)
            {
                // read rows in to chunk of the specified size
                var row = ReadRow(reader);
                chunk.Add(row);
            }
            start = chunk.FirstOrDefault()?.Start ?? DateTime.MinValue;
            end = chunk.LastOrDefault()?.End ?? DateTime.MinValue;
            return chunk;
        }
        /// <summary>
        /// Fetches tags for a collection of host contexts within a specified time range.
        /// </summary>
        /// <param name="tagsProcessor">The tags processor.</param>
        /// <param name="chunk">The collection of host contexts.</param>
        /// <param name="start">The start time of the time range.</param>
        /// <param name="end">The end time of the time range.</param>
        /// <returns>An enumerable collection of host contexts with their computed tags.</returns>
        IEnumerable<HostContext> FetchTags(TagsProcessor tagsProcessor, IEnumerable<HostContext> chunk, DateTime start, DateTime end)
        {
            var tags = tagsProcessor.ReadTagObjects(chunk.Select(c => c.Key ?? string.Empty), start, end);
            foreach (var ctx in chunk)
            {
                var ctxTags = tags.Where(t => t.Key == ctx.Key && TimeOverlaps(t.StartTime, t.EndTime, ctx.Start, ctx.End)).ToArray();
                ctx.Tags = tagsProcessor.ComputeCompactTags(ctxTags);
                yield return ctx; 
            }
        }

        /// <summary>
        /// Checks if the time range defined by <paramref name="firstStart"/> and <paramref name="firstEnd"/> overlaps with the time range defined by <paramref name="secondStart"/> and <paramref name="secondEnd"/>.
        /// </summary>
        /// <param name="firstStart">The start time of the first time range.</param>
        /// <param name="firstEnd">The end time of the first time range.</param>
        /// <param name="secondStart">The start time of the second time range.</param>
        /// <param name="secondEnd">The end time of the second time range.</param>
        /// <returns><c>true</c> if the time ranges overlap; otherwise, <c>false</c>.</returns>
        private bool TimeOverlaps(DateTime firstStart, DateTime firstEnd, DateTime secondStart, DateTime secondEnd)
        {
            return firstStart <= secondEnd && firstEnd >= secondStart;
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
            return new HostContext
            {
                Id = id,
                Key = key,
                Start = validity.LowerBound,
                End = validity.UpperBound,
                Connections = connections ?? Array.Empty<IpConnectionInfo>(),
                ResolvedDomains = resolvedDomains ?? Array.Empty<ResolvedDomainInfo>(),
                WebUrls = webUrls ?? Array.Empty<WebRequestInfo>(),
                TlsHandshakes = tlsHandshakes ?? Array.Empty<TlsHandshakeInfo>()
            };
        }
    }
}