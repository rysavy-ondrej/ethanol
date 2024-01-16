using Npgsql;
using System.Data;
using Ethanol.DataObjects;

/// <summary>
/// Processor for handling operations related to tags. The class is designed to read tags relevant 
/// for host context from the database. Its primary purpose is to compute aggregated tag information 
/// for efficient data representation and further analysis. It leverages tag data stored in a database 
/// table, ensuring accurate and up-to-date tag processing.
/// </summary>
class TagsProcessor
{
    /// <summary>
    /// Logger instance for logging messages.
    /// </summary>
    private readonly ILogger? __logger;

    /// <summary>
    /// The provider for fetching tag data from PostgreSQL.
    /// </summary>
    private readonly PostgresTagDataSource _provider;

    /// <summary>
    /// Initializes a new instance of the <see cref="TagsProcessor"/> class.
    /// </summary>
    /// <param name="connection">The Npgsql connection instance.</param>
    /// <param name="tablename">The name of the table to work with.</param>

    public TagsProcessor(NpgsqlConnection connection, string tablename, ILogger? logger)
    {
        _provider = new PostgresTagDataSource(connection, tablename);
        __logger = logger;
    }
    /// <summary>
    /// Represents aggregated activity data.
    /// </summary>
    private record ActivityAll(int flows, long bytes);

    /// <summary>
    /// Represents a mapping of a dependency.
    /// </summary>
    private record DependencyMapping(string src, string port, int num_packets);

    /// <summary>
    /// Computes compact tag representations for the given set of tags.
    /// </summary>
    /// <param name="tags">Array of <see cref="TagObject"/> to be processed.</param>
    /// <returns>A dictionary with compacted tag representations.</returns>
    public Dictionary<string, object> ComputeCompactTags(IList<TagObject> tags)
    {
        string[] CleanAndSplitString(string? s)
        {
            return (s ?? String.Empty).Split(',').Select(p => p.Trim('[', ']', '{', '}', ' ', '"')).ToArray();
        }
        string CleanAndRejoinString(string? s, char sep)
        {
            return String.Join(sep, CleanAndSplitString(s));
        }
        long SafeIntFromFloatString(string? s)
        {
            if (s == null) return 0;
            return System.Convert.ToInt64(double.TryParse(s.Trim('"') ?? string.Empty, out var p) ? p : 0f);
        }
        Dictionary<string, Dictionary<string, long>> GetDependencyMapping(IEnumerable<TagObject> enumerable)
        {
            var mapping = enumerable
                .Select(d => (key: d.Value, val: d.GetDetailsAs<DependencyMapping>()))
                .Select(m => (ip: m.key.ToSafeString(), port: (m.val?.port).ToSafeString(), packets: (long)(m.val?.num_packets ?? 0)))
                .GroupBy(g => g.ip, (k, e) => (ip: k, ports: e.GroupBy(t => t.port, (k, e) => (port: k, packets: e.Sum(x => x.packets))).ToDictionary(x => x.port.ToSafeString(), x => x.packets)))
                .ToDictionary(x => x.ip.ToSafeString(), x => x.ports);
            return mapping;
        }
        ActivityAll GetActivityInformation(string flows, string bytes)
        {
            var activityFlows = tags.WhereType(flows).Select(t => (int)SafeIntFromFloatString(t.Value));
            var activityBytes = tags.WhereType(bytes).Select(t => SafeIntFromFloatString(t.Value));
            return new ActivityAll(activityFlows.Sum(), activityBytes.Sum());
        }

        var result = new Dictionary<string, object>();
        try
        {
            result["ip_dependency_client"] = GetDependencyMapping(tags.WhereType("ip_dependency_client"));
            result["ip_dependency_server"] = GetDependencyMapping(tags.WhereType("ip_dependency_server"));
            result["open_ports"] = tags.WhereType("open_ports").SelectMany(t => CleanAndSplitString(t.Value)).Distinct().ToList();
            result["tags_by_services"] = tags.WhereType("tags_by_services").SelectMany(t => CleanAndSplitString(t.Value)).Distinct().ToList();
            result["hostml_label"] = tags.WhereType("hostml_label").SelectMany(t => CleanAndSplitString(t.Value)).Distinct().ToList();
            result["in_flow_tags"] = tags.WhereType("in_flow_tags").SelectMany(t => CleanAndSplitString(t.Value)).Distinct().ToList();
            result["tls_os_version"] = tags.WhereType("tls_os_version").Select(t => CleanAndRejoinString(t.Value, '/')).Distinct().ToList();
            result["activity_all"] = GetActivityInformation("activity_flows", "activity_bytes");
            result["activity_global"] = GetActivityInformation("activity_flows_global", "activity_bytes_global");
        }
        catch (Exception e)
        {
            __logger?.LogError(e, "Cannot compact tags {0}.", tags);
        }
        return result;
    }

    /// <summary>
    /// Reads tag objects for the specified key within a given time range.
    /// </summary>
    /// <param name="key">The key to filter tags.</param>
    /// <param name="start">The start of the date-time range.</param>
    /// <param name="end">The end of the date-time range.</param>
    /// <returns>An array of <see cref="TagObject"/> matching the criteria.</returns>
    public IDictionary<string, List<TagObject>> ReadTagObjects(IEnumerable<string> key, DateTimeOffset start, DateTimeOffset end)
    {
        var dictionary = new Dictionary<string, List<TagObject>>();
        var tags = _provider.GetMany(key, start, end);
        foreach (var tag in tags)
        { 
            if (tag.Key == null) continue;
            if (dictionary.TryGetValue(tag.Key, out var list))
            {
                list.Add(tag);
            }
            else
            {
                dictionary[tag.Key] = new List<TagObject> { tag };
            }
        }
        return dictionary;
    }
}
public static class ToStringExtensions
{
    public static string ToSafeString<T>(this T? s)
    {
        return s?.ToString() ?? string.Empty;
    }
}
