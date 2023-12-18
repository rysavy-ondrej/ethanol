
namespace Ethanol.DataObjects
{
    /// <summary>
    /// Represents information about a resolved domain, including details about the DNS server, query string, and response data.
    /// </summary>
    /// <param name="DnsServer">The IP address of the DNS server that was used to resolve the domain.</param>
    /// <param name="QueryString">The query string used to look up the domain.</param>
    /// <param name="ResponseData">The response data returned by the DNS server.</param>
    /// <param name="ResponseCode">The response code returned by the DNS server (e.g. NoError, NXDomain, etc.).</param>
    public record ResolvedDomainInfo(string? DnsServer, string QueryString, string? ResponseData, DnsResponseCode ResponseCode);
}
