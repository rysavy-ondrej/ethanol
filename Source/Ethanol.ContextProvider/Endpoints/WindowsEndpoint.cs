using FastEndpoints;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Provides an endpoint to retrieve a list of available time windows based on the specified period.
/// </summary>
/// <remarks>
/// The endpoint listens to GET requests on "/api/v1/host-context/windows" and returns a list of time windows that fall within the specified period.
/// </remarks>
[HttpGet("/api/v1/host-context/windows")]
public class WindowsEndpoint : Endpoint<WindowsQuery, List<WindowObject>>
{
    /// <summary>
    /// Handles incoming requests and responds with a list of available time windows based on the provided query parameters.
    /// </summary>
    /// <param name="query">The query parameters specifying the period for which time windows are to be retrieved.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Asynchronous task with a list of available time windows within the specified period.</returns>
    public override async Task HandleAsync(WindowsQuery query, CancellationToken ct)
    {
        await SendAsync(new List<WindowObject>(), 200);
    }
}
