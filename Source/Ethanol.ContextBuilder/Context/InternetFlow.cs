using System;
using System.Net;
using System.Net.Sockets;

namespace Ethanol.ContextBuilder.Context
{
    public enum IPVersion
    {
        /// <summary>Internet protocol version 4.</summary>
        IPv4 = 4,

        /// <summary>Internet protocol version 6.</summary>
        IPv6 = 6
    }

    /// <summary>
    /// An abstract representation of internet flow.
    /// </summary>
    public abstract class InternetFlow
    {
        /// <summary>
        /// The IP version
        /// </summary>
        public IPVersion Version { get; set; }

        /// <summary>
        /// The destination address
        /// </summary>
        public IPAddress DestinationAddress { get; set; }

        public ushort DestinationPort { get; set; }

        /// <summary>
        /// The protocol of the ip packet's payload
        /// Named 'Protocol' in IPv4
        /// Named 'NextHeader' in IPv6'
        /// </summary>
        public ProtocolType Protocol { get; set; }

        /// <summary>
        /// The source address
        /// </summary>
        public IPAddress SourceAddress { get; set; }


        public ushort SourcePort { get; set; }

        public FlowKey FlowKey => new FlowKey(Protocol, SourceAddress, SourcePort, DestinationAddress, DestinationPort);
    }

    public class IpFlow : InternetFlow
    {
        public string ApplicationTag { get; set; }

        public DateTime TimeStart { get; set; }
        /// <summary>
        /// The duration of the flow..
        /// </summary>
        public TimeSpan TimeDuration { get; set; }

        /// <summary>
        /// Packet Delta Count field is a data field that represents the number of packets that have been sent between two network devices during a particular flow.
        /// </summary>
        public int PacketDeltaCount { get; set; }

        /// <summary>
        /// Octet Delta Count field is a data field that represents the number of octets (bytes) that have been sent between two network devices during a particular flow. 
        /// </summary>
        public int OctetDeltaCount { get; set; }
    }

    public class HttpFlow : IpFlow
    {
        public string URL { get; set; }
        public string Hostname { get; set; }
        public string ResultCode { get; set; }
        public string Method { get; set; }
        public string OperatingSystem { get; set; }
        public string ApplicationInformation { get; set; }
    }

    public class DnsFlow : IpFlow
    {
        public string Identifier { get; set; }
        public int QuestionCount { get; set; }
        public int AnswerCount { get; set; }
        public int AuthorityCount { get; set; }
        public int AdditionalCount { get; set; }
        public string ResponseType { get; set; }
        public string ResponseClass { get; set; }
        public int ResponseTTL { get; set; }
        public string ResponseName { get; set; }
        public string ResponseCode { get; set; }
        public string ResponseData { get; set; }
        public string QuestionType { get; set; }
        public string QuestionClass { get; set; }
        public string QuestionName { get; set; }
        public string Flags { get; set; }
        public string Opcode { get; set; }
        public string QueryResponseFlag { get; set; }
    }

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

    public class SambaFlow : IpFlow
    {
        public string SMB1Command { get; set; }
        public string SMB2Command { get; set; }
        public string SMB2TreePath { get; set; }
        public string SMB2FilePath { get; set; }
        public string SMB2FileType { get; set; }
        public string SMB2Operation { get; set; }
        public string SMB2Delete { get; set; }
        public string SMB2Error { get; set; }
    }

}
