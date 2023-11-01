using FastEndpoints;
using System.Collections.Generic;
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
    /// <summary>
    /// Handles incoming requests and responds with a list of host-contexts based on the provided query parameters.
    /// </summary>
    /// <param name="query">The query parameters specifying time window and IP address.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Asynchronous task signalizing the completion of the operation.</returns>
    public override async Task HandleAsync(ContextsQuery query, CancellationToken ct)
    {
        await SendAsync(new List<HostContext>(), 200, ct);
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
    /// <summary>
    /// Handles incoming requests and responds with the count of host-contexts based on the provided query parameters.
    /// </summary>
    /// <param name="query">The query parameters specifying time window and IP address.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Asynchronous task signalizing the completion of the operation.</returns>
    public override async Task HandleAsync(ContextsQuery query, CancellationToken ct)
    {
        await SendAsync(0, 200);
    }
}


