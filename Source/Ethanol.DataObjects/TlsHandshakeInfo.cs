
namespace Ethanol.DataObjects
{
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
    public record TlsHandshakeInfo(string RemoteHostAddress, string RemoteHostName, ushort RemotePort, string ApplicationProcessName, InternetServiceTag[] InternetServices, string ApplicationProtocol, string ServerNameIndication, string JA3Fingerprint, string IssuerName, string SubjectName, string OrganisationName, string CipherSuites, string EllipticCurves);


}
