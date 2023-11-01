using FastEndpoints;
using System.Threading.Tasks;
using System.Threading;

/// <summary>
/// Provides an endpoint to retrieve information about the last available time window.
/// </summary>
/// <remarks>
/// The endpoint responds to GET requests on "/api/v1/host-context/windows/last" and returns the details of the last available time window.
/// </remarks>
[HttpGet("/api/v1/host-context/windows/last")]
public class LastWindowEndpoint : EndpointWithoutRequest<WindowObject>
{
    /// <summary>
    /// Handles incoming requests and responds with the details of the last available time window.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Asynchronous task signalizing the status of the operation.</returns>
    public override async Task HandleAsync(CancellationToken ct)
    {
       await SendAsync(new WindowObject { WindowId = 1, Start = DateTime.Now, End= DateTime.Now });
    }
}
