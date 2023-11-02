using Ethanol.ContextBuilder;
using Ethanol.ContextBuilder.Context;
using Ethanol.ContextBuilder.Enrichers.TagProviders;
using Npgsql;
using System.Data;

class TagsProcessor
{
    static protected readonly ILogger __logger = LogManager.GetCurrentClassLogger();
    private readonly PostgresTagProvider _provider;

    public TagsProcessor(NpgsqlConnection connection, string tablename)
    {
        _provider = new PostgresTagProvider(connection, tablename);
    }

    private record ActivityAll(int flows, long bytes);
    private record DependencyMapping(string src, string port, int num_packets);

    public Dictionary<string, object> ComputeCompactTags(TagObject[] tags)
    {
        string[] CleanAndSplitString(string s)
        {
            return (s ?? String.Empty).Split(',').Select(p => p.Trim('[', ']', '{', '}', ' ', '"')).ToArray();
        }
        string CleanAndRejoinString(string s, char sep)
        {
            return String.Join(sep, CleanAndSplitString(s));
        }
        long SafeIntFromFloatString(string s)
        {
            return Convert.ToInt64(double.TryParse(s.Trim('"') ?? string.Empty, out var p) ? p : 0f);
        }
        Dictionary<string, Dictionary<string, long>> GetDependencyMapping(IEnumerable<TagObject> enumerable)
        {
            var mapping = enumerable
                .Select(d => (key: d.Value, val: d.GetDetailsAs<DependencyMapping>()))
                .Select(m => (ip: m.key, port: m.val.port ?? string.Empty, packets: (long)m.val.num_packets))
                .GroupBy(g => g.ip, (k, e) => (ip: k, ports: e.GroupBy(t => t.port, (k, e) => (port: k, packets: e.Sum(x => x.packets))).ToDictionary(x => x.port, x => x.packets)))
                .ToDictionary(x => x.ip, x => x.ports);
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
            __logger.LogError(e, "Cannot compact tags {0}.", tags);
        }
        return result;
    }

    public TagObject[] ReadTagObjects(string key, DateTime start, DateTime end)
    {
        var tags = _provider.Get(key, start, end);
        return tags.ToArray();
    }
}
