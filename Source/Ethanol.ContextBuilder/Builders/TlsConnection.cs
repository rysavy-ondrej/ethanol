using Ethanol.ContextBuilder.Context;

namespace Ethanol.ContextBuilder.Builders
{
    /// <summary>
    /// The record describes information on a single TLS (Transport Layer Security) connection, which 
    /// is a secure, encrypted communication channel established between two parties over a network.
    /// </summary>
    public record TlsConnection
    {
        /// <summary>
        /// The IPFIX key to identify the current TLS connection.
        /// </summary>
        public FlowKey Flow { get; set; }

        /// <summary>
        /// JA3 is a type of fingerprinting method that is used to identify the client-side software that is initiating a TLS (Transport Layer Security) connection.
        /// <para/>
        /// The JA3 fingerprint is created by calculating a hash of the TLS client hello message, which is the first message exchanged during the TLS handshake process.
        /// </summary>
        public string JA3 { get; set; }
        /// <summary>
        /// The negotiated version of the connection.
        /// </summary>
        public string Version { get; set; }
        /// <summary>
        /// The cipher suite negotiated for the connection.
        /// </summary>
        public string CipherSuite { get; set; }
        /// <summary>
        /// TLS (Transport Layer Security) Server Name Indication (SNI) is an extension to the TLS protocol that allows the client to indicate 
        /// which hostname it is attempting to connect to during the TLS handshake. 
        /// <para/>
        /// The SNI extension is used to support multiple hostnames
        /// on a single server, which is particularly useful for hosting multiple websites on a single IP address.
        /// </summary>
        public string ServerNameIndication { get; set; }
        /// <summary>
        /// The ALPN extension is used during the TLS handshake process to negotiate the application protocol that will be used over the secure connection.
        /// <para/>
        /// Some common options include HTTP/1.1, HTTP/2, SPDY/3.1, QUIC/1, and FTP.
        /// </summary>
        public string ApplicationLayerProtocolNegotiation { get; set; }
        /// <summary>
        /// The name of the certificate authority that issued the certificate.
        /// </summary>
        public string IssuerCommonName { get; set; }
        /// <summary>
        /// The name of the entity that the certificate represents (e.g., the website domain name).
        /// </summary>
        public string SubjectCommonName { get; set; }
        /// <summary>
        /// The subject organization name field can be used to identify the organization that owns or operates the website or service that is associated with the TLS certificate (e.g., Microsoft).
        /// </summary>
        public string SubjectOrganizationName { get; set; }
    }
}
