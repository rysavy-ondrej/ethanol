using FastEndpoints;
using Ethanol.DataObjects;

namespace Ethanol.ContextProvider.Endpoints
{

    /// <summary>
    /// Provides an endpoint to retrieve an enumeration of aggregated host-contexts for a specified range of time windows.
    /// </summary>
    /// <remarks>
    /// The endpoint listens to GET requests on "/api/v1/host-context/contexts/aggregated" and returns a list of aggregated host-contexts that correspond to the specified range of time windows.
    /// </remarks>
    [HttpGet("/api/v1/host-context/contexts/aggregated")]
    public class AggregatedContextsEndpoint : Endpoint<AggregatedContextsQuery, List<HostContext>>
    {
        /// <summary>
        /// Handles incoming requests and responds with an enumeration of aggregated host-contexts based on the provided range of time windows.
        /// </summary>
        /// <param name="req">The query parameters specifying the range of time windows for which aggregated host-contexts are to be retrieved.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>Asynchronous task with a list of aggregated host-contexts for the specified range of time windows.</returns>
        public override Task HandleAsync(AggregatedContextsQuery req, CancellationToken ct)
        {
            return base.HandleAsync(req, ct);
        }
    }
}