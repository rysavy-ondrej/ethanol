using Ethanol.ContextBuilder.Context;
using Ethanol.ContextBuilder.Enrichers;
using System.Net;


namespace Ethanol.ContextBuilder.Polishers
{

    public record CustomHostAttribute(string Name, object Value, float Reliability);


    /// <summary>
    /// Represents information about an IP connection, including details on packet and flow counts.
    /// </summary>
    /// <param name="RemoteHostAddress">The remote host IP address.</param>
    /// <param name="RemoteHostName">The remote host name.</param>
    /// <param name="RemotePort">The remote port number.</param>
    /// <param name="Flows">The number of flows associated with the connection.</param>
    /// <param name="ApplicationProcessName">The name of the application or process that made the request or accepted the connection.</param>
    /// <param name="PacketsSent">The number of packets sent over the connection.</param>
    /// <param name="OctetsSent">The number of octets sent over the connection.</param>
    /// <param name="PacketsRecv">The number of packets received over the connection.</param>
    /// <param name="OctetsRecv">The number of octets received over the connection.</param>
    public record IpConnectionInfo(IPAddress RemoteHostAddress, string RemoteHostName, ushort RemotePort, string ApplicationProcessName, int Flows, int PacketsSent, int OctetsSent, int PacketsRecv, int OctetsRecv);


    /// <summary>
    /// Represents information about a web request, including details about the remote host, port, and URL.
    /// </summary>
    /// <param name="RemoteHostAddress">The remote host IP address.</param>
    /// <param name="RemoteHostName">The remote host name.</param>
    /// <param name="RemotePort">The remote port number.</param>
    /// <param name="ApplicationProcessName">The name of the application or process that made the request.</param>
    /// <param name="Method">The HTTP method used in the request (e.g. GET, POST, etc.).</param>
    /// <param name="Url">The URL of the requested resource.</param>
    public record WebRequestInfo(IPAddress RemoteHostAddress, string RemoteHostName, ushort RemotePort, string ApplicationProcessName, string Method, string Url);

    /// <summary>
    /// Represents information about a resolved domain, including details about the DNS server, query string, and response data.
    /// </summary>
    /// <param name="DnsServer">The IP address of the DNS server that was used to resolve the domain.</param>
    /// <param name="QueryString">The query string used to look up the domain.</param>
    /// <param name="ResponseData">The response data returned by the DNS server.</param>
    /// <param name="ResponseCode">The response code returned by the DNS server (e.g. NoError, NXDomain, etc.).</param>
    public record ResolvedDomainInfo(IPAddress DnsServer, string QueryString, string ResponseData, DnsResponseCode ResponseCode);

    /// <summary>
    /// Represents information about a TLS handshake, including details about the remote host, port, and certificate.
    /// </summary>
    /// <param name="RemoteHostAddress">The remote host IP address.</param>
    /// <param name="RemoteHostName">The remote host name.</param>
    /// <param name="RemotePort">The remote port number.</param>
    /// <param name="ApplicationProcessName">The name of the application or process that initiated the handshake.</param>
    /// <param name="ApplicationProtocol">The name of the application protocol negotiated during the handshake (e.g. HTTP/2).</param>
    /// <param name="ServerNameIndication">The Server Name Indication (SNI) provided during the handshake.</param>
    /// <param name="JA3Fingerprint">The JA3 fingerprint of the client and server hello messages exchanged during the handshake.</param>
    /// <param name="IssuerName">The name of the certificate issuer.</param>
    /// <param name="SubjectName">The name of the certificate subject.</param>
    /// <param name="OrganisationName">The name of the certificate organisation.</param>
    public record TlsHandshakeInfo(IPAddress RemoteHostAddress, string RemoteHostName, ushort RemotePort, string ApplicationProcessName, string ApplicationProtocol, string ServerNameIndication, string JA3Fingerprint, string IssuerName, string SubjectName, string OrganisationName, string CipherSuites, string EllipticCurves);

    /// <summary>
    /// Represents a simplified IP host context.
    /// </summary>
    public record IpTargetHostContext(
        /// <summary>
        /// The IP address of the host.
        /// </summary>
        IPAddress HostAddress,

        /// <summary>
        /// An array of other attributes associated with the context.
        /// </summary>
        TagRecord[] CustomAttributes,

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
        /// An array of TLS handshake information for secure connections initiated by the host.
        /// </summary>
        TlsHandshakeInfo[] TlsHandshakes
    );
}
