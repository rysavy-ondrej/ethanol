using System.Net;


namespace Ethanol.ContextBuilder.Simplifier
{
    /// <summary>
    /// Represents a simplified IP host context.
    /// </summary>
    public record IpSimpleHostContext(
        /// <summary>
        /// The IP address of the host.
        /// </summary>
        IPAddress HostAddress,

        /// <summary>
        /// The operating system running on the host.
        /// </summary>
        string OperatingSystem,

        /// <summary>
        /// An array of IP connection information for connections initiated by the host.
        /// </summary>
        IpConnectionInfo[] InitiatedConnections,

        /// <summary>
        /// An array of IP connection information for connections accepted by the host.
        /// </summary>
        IpConnectionInfo[] AcceptedConnections,

        /// <summary>
        /// An array of resolved domain information for domains resolved by the host.
        /// </summary>
        ResolvedDomainInfo[] ResolvedDomains,

        /// <summary>
        /// An array of web request information for URLs requested by the host.
        /// </summary>
        WebRequestInfo[] WebUrls,

        /// <summary>
        /// An array of TLS connection information for secure connections initiated by the host.
        /// </summary>
        TlsConnectionInfo[] Secured
    );
}
