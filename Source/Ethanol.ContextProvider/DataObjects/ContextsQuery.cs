/// <summary>
/// Represents a query to retrieve host-context data for a specific time window and IP address.
/// </summary>
public record ContextsQuery
{
    /// <summary>
    /// Gets or sets the start date and time of the query time window. If not set, there's no lower bound to the query time window.
    /// </summary>
    public DateTime? Start { get; set; }

    /// <summary>
    /// Gets or sets the end date and time of the query time window. If not set, there's no upper bound to the query time window.
    /// </summary>
    public DateTime? End { get; set; }

    /// <summary>
    /// Gets or sets the IP prefix used for filtering the context data. If not set, context data for all IP addresses will be retrieved.
    /// </summary>
    public string? HostKey { get; set; }

    internal string GetWhereExpression()
    {
        var start = Start.GetValueOrDefault(DateTime.MinValue);
        var end = End.GetValueOrDefault(DateTime.MaxValue);
        var exprs = new[]{  $"validity && '[{start},{end})'",
                             HostKey != null ? $"key = '{HostKey}'" : "true" };
        return String.Join(" AND ", exprs);
    }
}

