/// <summary>
/// Represents a distinct time window with a unique identifier and a start and end timestamp.
/// </summary>
public record WindowObject
{
    /// <summary>
    /// Gets or sets the starting timestamp of the time window.
    /// </summary>
    /// <remarks>
    /// The Start property defines the beginning of the time window for which data can be aggregated or segmented.
    /// </remarks>
    public DateTime Start { get; set; }

    /// <summary>
    /// Gets or sets the ending timestamp of the time window.
    /// </summary>
    /// <remarks>
    /// The End property defines the conclusion of the time window, marking the limit of data inclusion for this particular window.
    /// </remarks>
    public DateTime End { get; set; }
}
