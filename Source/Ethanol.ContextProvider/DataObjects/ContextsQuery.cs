using System.Globalization;

/// <summary>
/// Represents a query to retrieve host-context data for a specific time window and IP address.
/// </summary>
public record ContextsQuery
{
    /// <summary>
    /// Gets or sets the start date and time of the query time window. If not set, there's no lower bound to the query time window.
    /// </summary>
    public DateTimeOffset? Start { get; set; }

    /// <summary>
    /// Gets or sets the end date and time of the query time window. If not set, there's no upper bound to the query time window.
    /// </summary>
    public DateTimeOffset? End { get; set; }

    /// <summary>
    /// Gets or sets the flag to modify ordering of the contexts in the result. Default is ascending order by the time of the context.
    /// </summary>
    public bool? OrderDescending { get; set; }
    /// <summary>
    /// Gets or sets the IP prefix used for filtering the context data. If not set, context data for all IP addresses will be retrieved.
    /// </summary>
    public string? HostKey { get; set; }

    /// <summary>
    /// Generates WHERE expression from the properties of the current object.
    /// </summary>
    /// <returns>Expression usable in WHERE clauses of the SQL expression.</returns>
    internal string GetWhereExpression()
    {
        var start = Start.GetValueOrDefault(DateTimeOffset.MinValue).ToString("o", CultureInfo.InvariantCulture);
        var end = End.GetValueOrDefault(DateTimeOffset.MaxValue).ToString("o", CultureInfo.InvariantCulture);
        var exprs = new[]{  $"validity && '[{start},{end})'",
                             HostKey != null ? $"key = '{HostKey}'" : "key <> '0.0.0.0'" };
        return String.Join(" AND ", exprs);
    }
}

