using FastEndpoints;
using Npgsql;
using NpgsqlTypes;
using System.Data;
using System.Text.Json;
using Ethanol.DataObjects;
using Ethanol.ContextBuilder.Serialization;
using System.Diagnostics.CodeAnalysis;
using Ethanol.ContextBuilder.Enrichers;

namespace Ethanol.ContextProvider.Endpoints
{
    [HttpGet("/api/v3/host-context/context-stream")]
    public class ContextsStreamEndpoint3 : Endpoint<ContextsQuery, List<HostContext>>
    {
        private readonly ILogger? _logger;
        private readonly NpgsqlDataSource _datasource;
        private readonly string _hostContextTable;
        private readonly string _tagsTableName;
        private readonly int _tagsChunkSize;

        public ContextsStreamEndpoint3(NpgsqlDataSource datasource, EthanolConfiguration configuration, ILogger? logger)
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
                    cmd.CommandText = $"SELECT * FROM \"{_hostContextTable}\" WHERE {query.GetWhereExpression()} ORDER BY validity, key ASC";
                    _logger?.LogInformation($"Fetching context objects {query.GetWhereExpression()} from {_hostContextTable} table.");

                    using var reader = await cmd.ExecuteReaderAsync(ct);
                    TagsIntervalReader? tagReader = null; 
                    while (ct.IsCancellationRequested == false)
                    {
                        var ctx = await ReadNextAsync(reader);
                        if (ctx == null) break; // no more data to read
                        if (ctx.Key == null) continue; // skip rows with null key
                        readContextCount++;

                        var start = ctx.Start;
                        var end = ctx.End;
                        // If tagReader has not been initialized yet or it doesn't cover the time range from start to end
                        // then we need to create a new tag reader for the time range from start to end.
                        if (tagReader == null || tagReader.CoversTimeRange(start, end) == false)
                        {
                            _logger?.LogInformation($"Creating a new tag reader for the time range from {start} to {end}...");
                            tagReader?.Close();
                            tagReader?.Dispose();
                            tagReader = await TagsIntervalReader.Create(tagsProcessor, start, end);
                            _logger?.LogInformation($"Created a new tag reader for the time range from {start} to {end}.");
                        }

                        // fetch tags for the context from the reader:
                        var tags = await tagReader.ReadTagsAsync(ctx.Key);
                        ctx.Tags = tagsProcessor.ComputeCompactTags(tags);
                        
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

        class TagsIntervalReader : IDisposable
        {
            NpgsqlDataReader _reader;
            DateTimeOffset _start;
            DateTimeOffset _end;
            TagsIntervalReader(NpgsqlDataReader reader, DateTimeOffset start, DateTimeOffset end)
            {
                _reader = reader;
                _start = start;
                _end = end;
            }

            public NpgsqlDataReader Reader => _reader;

            public bool CoversTimeRange(DateTimeOffset start, DateTimeOffset end)
            {
                return start == _start && end == _end;
            }

            public void Close()
            {
                _reader.Close();
            }
            public void Dispose()
            {
                _reader.Close();
                _reader.Dispose();
            }
            public static async Task<TagsIntervalReader> Create(TagsProcessor tagsProcessor, DateTimeOffset start, DateTimeOffset end)
            {
                    var reader = await tagsProcessor.GetReaderForIntervalAsync(start, end, "key");
                    var tagsReader = new TagsIntervalReader(reader, start, end);
                    return tagsReader;
            }

            bool _readStarted = false;
            bool _eof = false;
            internal async Task<IList<TagObject>> ReadTagsAsync(string key)
            {
                var tags = new List<TagObject>();
                if (_eof) return tags;

                // Read the first row if it hasn't been read yet:
                if (_readStarted == false)
                {
                    _readStarted = true;
                    if (await _reader.ReadAsync() == false)
                    {
                        _eof = true;
                        return tags;
                    } 
                }
            
                // skip rows with keys that are less than the key we are looking for
                while(String.CompareOrdinal(_reader.GetString("key"), key) < 0)
                {
                    if (await _reader.ReadAsync() == false)
                    {
                        _eof = true;
                        return tags;
                    } 
                }  
                // read rows with the matching key:
                while(PostgresTagDataSource.TryReadRow(_reader, key, out var tag))
                {
                    tags.Add(tag);
                    if (await _reader.ReadAsync() == false)
                    {
                        _eof = true;
                        return tags;
                    } 
                }
                return tags;
            }
        }

        /// <summary>
        /// Reads the next chunk of data from the NpgsqlDataReader and returns a list of HostContext objects.
        /// </summary>
        /// <param name="reader">The NpgsqlDataReader to read data from.</param>
        /// <param name="start">The start DateTime of the chunk.</param>
        /// <param name="end">The end DateTime of the chunk.</param>
        /// <returns>A list of HostContext objects representing the chunk of data.</returns>
        async Task<HostContext?> ReadNextAsync(NpgsqlDataReader reader)
        {
            
            if (await reader.ReadAsync() && TryReadRow(reader, out var row))
            {
                return row;
            }
            return null;
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

        private bool TryReadRow(NpgsqlDataReader reader,  [NotNullWhen(true)] out HostContext? hostContext)
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