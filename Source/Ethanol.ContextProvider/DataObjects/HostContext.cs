using Ethanol.ContextBuilder.Polishers;

public class HostContext
{
    public HostContext(long id, string key, DateTime lowerBound, DateTime upperBound, IpConnectionInfo[] connections, ResolvedDomainInfo[] resolvedDomains, WebRequestInfo[] webUrls, TlsHandshakeInfo[] tlsHandshakes)
    {
        Id = id;
        Key = key;
        Start = lowerBound;
        End = upperBound; 
        Connections = connections;
        ResolvedDomains = resolvedDomains;
        WebUrls = webUrls;
        TlsHandshakes = tlsHandshakes;
    }

    public long Id { get; set;  }
    public string Key { get; set; }
    public DateTime Start { get;  set; }
    public DateTime End { get;  set; }
    public IpConnectionInfo[] Connections { get; set; }
    public ResolvedDomainInfo[] ResolvedDomains { get; set; }
    public WebRequestInfo[] WebUrls { get; set; }
    public TlsHandshakeInfo[] TlsHandshakes { get; set; }
    public Dictionary<string, object> Tags { get; internal set; }
}