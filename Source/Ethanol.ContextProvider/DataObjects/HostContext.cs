using Ethanol.ContextBuilder.Polishers;

public class HostContext
{
    public HostContext(long id, string key, DateTime lowerBound, DateTime upperBound, IpConnectionInfo[] initiatedConnections, IpConnectionInfo[] acceptedConnections, ResolvedDomainInfo[] resolvedDomains, WebRequestInfo[] webUrls, TlsHandshakeInfo[] tlsHandshakes)
    {
        Id = id;
        Key = key;
        Start = lowerBound;
        End = upperBound; 
        InitiatedConnections = initiatedConnections;
        AcceptedConnections = acceptedConnections;
        ResolvedDomains = resolvedDomains;
        WebUrls = webUrls;
        TlsHandshakes = tlsHandshakes;
    }

    public long Id { get; set;  }
    public string Key { get; set; }
    public DateTime Start { get;  set; }
    public DateTime End { get;  set; }
    public IpConnectionInfo[] InitiatedConnections { get; set; }
    public IpConnectionInfo[] AcceptedConnections { get; set;  }
    public ResolvedDomainInfo[] ResolvedDomains { get; set; }
    public WebRequestInfo[] WebUrls { get; set; }
    public TlsHandshakeInfo[] TlsHandshakes { get; set; }
    public Dictionary<string, object> Tags { get; internal set; }
}