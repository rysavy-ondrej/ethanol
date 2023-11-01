/// <summary>
/// Represents a query for retrieving window information.
/// </summary>
public record WindowsQuery
{
    /// <summary>
    /// Gets or sets the starting timestamp of the window.
    /// </summary>
    public DateTime Start { get; set; }

    /// <summary>
    /// Gets or sets the ending timestamp of the window.
    /// </summary>
    public DateTime End { get; set; }
}


