/// <summary>
/// Represents a query to retrieve aggregated context data spanning multiple time windows.
/// </summary>
public class AggregatedContextsQuery
{
    /// <summary>
    /// Gets or sets the starting window's unique identifier for the aggregation range.
    /// </summary>
    /// <remarks>
    /// The StartWindowId indicates the beginning of the range for which aggregated context data is to be retrieved.
    /// </remarks>
    public long StartWindowId { get; set; }

    /// <summary>
    /// Gets or sets the ending window's unique identifier for the aggregation range.
    /// </summary>
    /// <remarks>
    /// The EndWindowId indicates the end of the range for which aggregated context data is to be retrieved.
    /// </remarks>
    public long EndWindowId { get; set; }
}
