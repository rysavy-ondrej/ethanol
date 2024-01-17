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
    [HttpGet("/api/v2/host-context/context-stream")]
    public class ContextsStreamEndpoint2 : Endpoint<ContextsQuery, List<HostContext>>
    {
        private readonly ILogger? _logger;
        private readonly NpgsqlDataSource _datasource;
        private readonly string _hostContextTable;
        private readonly string _tagsTableName;
        private readonly int _tagsChunkSize;

        public ContextsStreamEndpoint2(NpgsqlDataSource datasource, EthanolConfiguration configuration, ILogger? logger)
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
            _logger?.LogInformation($"Endpoint '{nameof(ContextsStreamEndpoint)}' received requests '{query}'.");
            try
            {
                using var connection = _datasource.OpenConnection();
                using var tagConnection = _datasource.OpenConnection();
                var tagsProcessor = new TagsProcessor(tagConnection, _tagsTableName, _logger);
                var contextCount = 0;
                var readContextCount = 0;
                var taggedContextCount = 0;

                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = $"SELECT COUNT(*) FROM \"{_hostContextTable}\" WHERE {query.GetWhereExpression()}";
                    _logger?.LogInformation($"Counting host contexts {query.GetWhereExpression()} from {_hostContextTable} table.");
                    contextCount = Convert.ToInt32(await cmd.ExecuteScalarAsync(ct));
                    _logger?.LogInformation($"Counted {contextCount} host contexts.");
                }

                _logger?.LogInformation($"Using connection: `{connection.ConnectionString}` to access database.");
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = $"SELECT * FROM \"{_hostContextTable}\" WHERE {query.GetWhereExpression()} ORDER BY id ASC";
                    _logger?.LogInformation($"Fetching context objects {query.GetWhereExpression()} from {_hostContextTable} table.");

                    using var reader = await cmd.ExecuteReaderAsync(ct);
                    while (ct.IsCancellationRequested == false)
                    {
                        var ctx = ReadNext(reader, out var start, out var end, ct);
                        if (ctx == null) break; // no more data to read
                        readContextCount++;
                        var tags = ReadOrGetTags(tagsProcessor, start!.Value, end!.Value);
                        if (tags.TryGetValue(ctx.Key, out var ctxTags))
                        {
                            ctx.Tags = tagsProcessor.ComputeCompactTags(ctxTags.Where(t => TimeOverlaps(t.StartTime, t.EndTime, ctx.Start, ctx.End)).ToList());
                        }
                        var contextJson = Json.Serialize(ctx);
                        await HttpContext.Response.WriteAsync($"{contextJson}\n", ct);
                        taggedContextCount++;
                        
                        //_logger?.LogInformation($"Processed context chunk: host_count={chunk.Count}, validity_from={start?.ToString("o")}, validity_to={end?.ToString("o")}.");
                        await HttpContext.Response.Body.FlushAsync(ct); // Flush the data to the client
                    }
                    reader.Close();
                }
                _logger?.LogInformation($"Fetched {readContextCount}/{contextCount} host contexts and wrote {taggedContextCount} tagged host contexts.");
                connection.Close();
                tagConnection.Close();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Endpoint '{nameof(ContextsEndpoint)}' cannot create a response for the query '{query}'.");
                await SendErrorsAsync(500, ct);
            }
        }

        
        /// <summary>
        /// Fetches or gets the tags using the specified tags processor, start and end date. If the tags are not available in the cache, they are fetched from the database.
        /// </summary>
        /// <param name="tagsProcessor">The tags processor.</param>
        /// <param name="start">The start date.</param>
        /// <param name="end">The end date.</param>
        /// <returns>A dictionary containing the fetched or retrieved tags.</returns>
        private IDictionary<string, List<TagObject>> ReadOrGetTags(TagsProcessor tagsProcessor, DateTimeOffset start, DateTimeOffset end)
        {
            if (_tagsCache?.CoversTimeRange(start, end) ?? false)
            {
                //_logger?.LogInformation($"Using cached tags for {start} - {end}.");
                return _tagsCache.Cache;
            }
            else
            {
                _logger?.LogInformation($"Fetching tags for {start} - {end}.");
                _tagsCache = null;
                var tags = tagsProcessor.ReadTagObjects(start, end);
                _tagsCache = new TagsCache(tags, start, end);
                _logger?.LogInformation($"Fetched {tags.Count} tag groups for {start} - {end}.");
                return tags;
            }
        }

        TagsCache? _tagsCache;

        class TagsCache
        {
            IDictionary<string, List<TagObject>> _cache;
            DateTimeOffset _start;
            DateTimeOffset _end;
            public TagsCache(IDictionary<string, List<TagObject>> cache, DateTimeOffset start, DateTimeOffset end)
            {
                _cache = cache;
                _start = start;
                _end = end;
            }

            public IDictionary<string, List<TagObject>> Cache => _cache;

            public bool CoversTimeRange(DateTimeOffset start, DateTimeOffset end)
            {
                return start == _start && end == _end;
            }
        }

        /// <summary>
        /// Reads the next chunk of data from the NpgsqlDataReader and returns a list of HostContext objects.
        /// </summary>
        /// <param name="reader">The NpgsqlDataReader to read data from.</param>
        /// <param name="start">The start DateTime of the chunk.</param>
        /// <param name="end">The end DateTime of the chunk.</param>
        /// <returns>A list of HostContext objects representing the chunk of data.</returns>
        HostContext? ReadNext(NpgsqlDataReader reader, out DateTimeOffset? start, out DateTimeOffset? end, CancellationToken ct)
        {
            if (reader.Read() && TryReadRow(reader, out var row))
            {// read rows in to chunk of the specified size
                start = row!.Start ;
                end = row!.End ;
                return row!;
            }
            start = null;
            end = null;
            return null;
        }
        /// <summary>
        /// Fetches tags for a collection of host contexts within a specified time range.
        /// </summary>
        /// <param name="tagsProcessor">The tags processor.</param>
        /// <param name="chunk">The collection of host contexts.</param>
        /// <param name="start">The start time of the time range.</param>
        /// <param name="end">The end time of the time range.</param>
        /// <returns>An enumerable collection of host contexts with their computed tags.</returns>
        IEnumerable<HostContext> FetchTags(TagsProcessor tagsProcessor, IEnumerable<HostContext> chunk, DateTimeOffset start, DateTimeOffset end)
        {
            var tags = ReadOrGetTags(tagsProcessor, start, end);
            _logger?.LogInformation($"Fetched {tags.Count} tag groups for {chunk.Count()} host contexts.");
            foreach (var ctx in chunk)
            {
                if (ctx.Key == null)
                {
                    _logger?.LogError($"Unexpected: Host context with id={ctx.Id} has no key.");
                    continue;
                }
                if (tags.TryGetValue(ctx.Key, out var ctxTags))
                {
                    ctx.Tags = tagsProcessor.ComputeCompactTags(ctxTags.Where(t => TimeOverlaps(t.StartTime, t.EndTime, ctx.Start, ctx.End)).ToList());
                }
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
        private bool TimeOverlaps(DateTimeOffset firstStart, DateTimeOffset firstEnd, DateTimeOffset secondStart, DateTimeOffset secondEnd)
        {
            return firstStart <= secondEnd && firstEnd >= secondStart;
        }

        private bool TryReadRow(NpgsqlDataReader reader, out HostContext? hostContext)
        {
            try
            {
                var id = reader.GetInt64("id");
                var key = reader.GetString("key");
                var validity = reader.GetFieldValue<NpgsqlRange<DateTimeOffset>>("validity");
                var connections = JsonSerializer.Deserialize<IpConnectionInfo[]>(reader.GetString("connections"));
                var resolvedDomains = JsonSerializer.Deserialize<ResolvedDomainInfo[]>(reader.GetString("resolveddomains"));
                var webUrls = JsonSerializer.Deserialize<WebRequestInfo[]>(reader.GetString("weburls"));
                var tlsHandshakes = JsonSerializer.Deserialize<TlsHandshakeInfo[]>(reader.GetString("tlshandshakes"));
                hostContext = new HostContext
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
                return true;
            }
            catch(Exception)
            {
                _logger?.LogWarning($"Failed to read a row from the reader.");
                hostContext = default;
                return false;
            }
        }
    }
}