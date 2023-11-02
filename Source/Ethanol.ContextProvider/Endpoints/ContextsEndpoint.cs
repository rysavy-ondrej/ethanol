using Ethanol.ContextBuilder.Polishers;
using FastEndpoints;
using Npgsql;
using NpgsqlTypes;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Provides an endpoint to retrieve a list of host-contexts based on the specified time window and IP address criteria.
/// </summary>
/// <remarks>
/// The endpoint listens to GET requests on "/api/v1/host-context/contexts" and returns a list of matching host-contexts.
/// </remarks>
[HttpGet("/api/v1/host-context/contexts")]
public class ContextsEndpoint : Endpoint<ContextsQuery, List<HostContext>>
{
    private readonly NpgsqlConnection _connection;
    private readonly TagsProcessor _tagsProcessor;

    public ContextsEndpoint(Npgsql.NpgsqlConnection connection)
    {
        this._connection = connection;
        connection.Open();
        _tagsProcessor = new TagsProcessor(connection, "enrichment_data");
    }

    /// <summary>
    /// Handles incoming requests and responds with a list of host-contexts based on the provided query parameters.
    /// </summary>
    /// <param name="query">The query parameters specifying time window and IP address.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Asynchronous task signalizing the completion of the operation.</returns>
    public override async Task HandleAsync(ContextsQuery query, CancellationToken ct)
    {
        using var cmd = _connection.CreateCommand();
        cmd.CommandText = $"SELECT * FROM \"public\".\"host_context\" WHERE {query.GetWhereExpression()}";
        using var reader = cmd.ExecuteReader();
        var rowList = new List<HostContext>();
        while (reader.Read())
        {
            var row = ReadRow(reader);
            rowList.Add(row);
        }
        reader.Close();
        // we have host context, now we need tags:
        foreach(var row in rowList)
        {
            var tags = _tagsProcessor.ReadTagObjects(row.Key, row.Start, row.End);
            row.Tags = _tagsProcessor.ComputeCompactTags(tags);
        }

        await SendAsync(rowList, 200, ct);
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

/// <summary>
/// Provides an endpoint to retrieve the count of host-contexts based on the specified time window and IP address criteria.
/// </summary>
/// <remarks>
/// The endpoint listens to GET requests on "/api/v1/host-context/contexts/count" and returns the count of matching host-contexts.
/// </remarks>
[HttpGet("/api/v1/host-context/contexts/count")]
public class ContextsCountEndpoint : Endpoint<ContextsQuery, int>
{
    private NpgsqlConnection _connection;

    public ContextsCountEndpoint(Npgsql.NpgsqlConnection connection)
    {
        _connection = connection;
        _connection.Open();
    }

    /// <summary>
    /// Handles incoming requests and responds with the count of host-contexts based on the provided query parameters.
    /// </summary>
    /// <param name="query">The query parameters specifying time window and IP address.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Asynchronous task signalizing the completion of the operation.</returns>
    public override async Task HandleAsync(ContextsQuery query, CancellationToken ct)
    {
        using var cmd = _connection.CreateCommand();
        cmd.CommandText = $"SELECT COUNT(*) FROM \"public\".\"host_context\"";
        var result = await cmd.ExecuteScalarAsync(ct);
        var count = Int32.TryParse(result?.ToString(), out var val) ? val : 0;
        await SendAsync(count, 200);
    }
}
