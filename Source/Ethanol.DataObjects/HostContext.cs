

using System.Diagnostics;

namespace Ethanol.DataObjects;

/// <summary>
/// Represents the context of a host including its time of activity, 
/// IP connections, resolved domains, web requests, and TLS handshakes.
/// </summary>
[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
public record HostContext
{
    /// <summary>
    /// Gets or sets the ID of the host context.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Gets or sets the key of the host context.
    /// </summary>
    public string? Key { get; set; }

    /// <summary>
    /// Gets or sets the start time of the host context.
    /// </summary>
    public DateTime Start { get; set; }

    /// <summary>
    /// Gets or sets the end time of the host context.
    /// </summary>
    public DateTime End { get; set; }

    /// <summary>
    /// Gets or sets the array of IP connection information associated with the host context.
    /// </summary>
    public IpConnectionInfo[]? Connections { get; set; }

    /// <summary>
    /// Gets or sets the array of resolved domain information associated with the host context.
    /// </summary>
    public ResolvedDomainInfo[]? ResolvedDomains { get; set; }

    /// <summary>
    /// Gets or sets the array of web request information associated with the host context.
    /// </summary>
    public WebRequestInfo[]? WebUrls { get; set; }

    /// <summary>
    /// Gets or sets the array of TLS handshake information associated with the host context.
    /// </summary>
    public TlsHandshakeInfo[]? TlsHandshakes { get; set; }

    /// <summary>
    /// Gets or sets the dictionary of tags associated with the host context.
    /// </summary>
    public Dictionary<string, object>? Tags { get; set; }

    private string GetDebuggerDisplay()
    {
        return ToString();
    }
}