/// <summary>
/// Represents a query to retrieve host-context data for a specific time window and IP address.
/// </summary>
public record ContextsQuery
{
    /// <summary>
    /// Gets or sets the unique identifier for the time window.
    /// </summary>
    /// <remarks>
    /// The WindowId defines the specific window of time for which the host-context data is to be retrieved.
    /// </remarks>
    public string? WindowId { get; set; }

    /// <summary>
    /// Gets or sets the IP address to filter specific host-context data.
    /// </summary>
    /// <remarks>
    /// The IP property allows for the retrieval of host-context data specific to the provided IP address.
    /// <para/>
    /// If not IP address is provided than all host-context within the given window will be retrieved.
    /// </remarks>
    public string? IP { get; set; }
}
