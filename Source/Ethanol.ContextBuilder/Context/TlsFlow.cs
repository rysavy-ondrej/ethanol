using System;

namespace Ethanol.ContextBuilder.Context
{
    public class TlsFlow : IpFlow
    {
        public string IssuerCommonName { get; set; }
        public string SubjectCommonName { get; set; }
        public string SubjectOrganisationName { get; set; }
        public DateTime CertificateValidityFrom { get; set; }
        public DateTime CertificateValidityTo { get; set; }
        public string SignatureAlgorithm { get; set; }
        public string PublicKeyAlgorithm { get; set; }
        public int PublicKeyLength { get; set; }

        public string ClientVersion { get; set; }
        public string CipherSuites { get; set; }
        public string ClientRandomID { get; set; }
        public string ClientSessionID { get; set; }
        public string ExtensionTypes { get; set; }
        public int ExtensionLengths { get; set; }
        public string EllipticCurves { get; set; }
        public string EcPointFormats { get; set; }
        public int ClientKeyLength { get; set; }
        public string JA3Fingerprint { get; set; }

        public string ContentType { get; set; }
        public string HandshakeType { get; set; }
        public TimeSpan SetupTime { get; set; }
        public string ServerVersion { get; set; }
        public string ServerRandomID { get; set; }
        public string ServerSessionID { get; set; }
        public string ServerCipherSuite { get; set; }
        public string ApplicationLayerProtocolNegotiation { get; set; }
        public string ServerNameIndication { get; set; }
        public int ServerNameLength { get; set; }
    }

}
