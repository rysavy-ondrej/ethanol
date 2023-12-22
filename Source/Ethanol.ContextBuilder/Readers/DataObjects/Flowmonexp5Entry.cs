using System;
using System.Text.Json.Serialization;

namespace Ethanol.ContextBuilder.Readers.DataObjects
{
    /// <summary>
    /// Represents an entry from the flowmonexp5 tool's JSON output. This class captures detailed network flow data, 
    /// encompassing general flow characteristics, DNS specifics, IP information, and transport layer details, among others.
    /// </summary>
    /// <remarks>
    /// Each property in the class corresponds to a JSON attribute in the exported data, with attribute names transformed 
    /// according to C# naming conventions. This includes details about:
    /// - Byte and packet flow in both directions (A and B).
    /// - Timestamps indicating the start and end of a flow.
    /// - DNS-specific attributes, including request-response counts and specific DNS record details.
    /// - Layer 3 (IP) details for both IPv4 and IPv6.
    /// - Layer 4 (Transport) details, including protocol and port information.
    /// - TLS-specific attributes, capturing details from the TLS handshake and certificate information.
    /// - HTTP attributes, providing insights into request-response behavior and user-agent characteristics.
    /// The JsonPropertyName annotations map each property to its corresponding JSON attribute name in the flowmonexp5 output.
    /// </remarks>
    public class Flowmonexp5Entry
    {
        [JsonPropertyName("BYTES_B")]
        public long BytesB { get; set; }

        [JsonPropertyName("BYTES_A")]
        public long BytesA { get; set; }

        [JsonPropertyName("END_NSEC")]
        public DateTime EndNsec { get; set; }

        [JsonPropertyName("END_NSEC_A")]
        public DateTime EndNsecA { get; set; }

        [JsonPropertyName("END_NSEC_B")]
        public DateTime EndNsecB { get; set; }

        [JsonPropertyName("INVEA_DNS_ADDTREC_COUNT_REQUEST")]
        public int InveaDnsAddtrecCountRequest { get; set; }

        [JsonPropertyName("INVEA_DNS_ADDTREC_COUNT_RESPONSE")]
        public int InveaDnsAddtrecCountResponse { get; set; }

        [JsonPropertyName("INVEA_DNS_ANSWREC_COUNT_REQUEST")]
        public int InveaDnsAnswrecCountRequest { get; set; }

        [JsonPropertyName("INVEA_DNS_ANSWREC_COUNT_RESPONSE")]
        public int InveaDnsAnswrecCountResponse { get; set; }

        [JsonPropertyName("INVEA_DNS_AUTHREC_COUNT_REQUEST")]
        public int InveaDnsAuthrecCountRequest { get; set; }

        [JsonPropertyName("INVEA_DNS_AUTHREC_COUNT_RESPONSE")]
        public int InveaDnsAuthrecCountResponse { get; set; }

        [JsonPropertyName("INVEA_DNS_CRR_CLASS")]
        public int InveaDnsCrrClass { get; set; }

        [JsonPropertyName("INVEA_DNS_CRR_NAME")]
        public string? InveaDnsCrrName { get; set; }

        [JsonPropertyName("INVEA_DNS_CRR_RDATA")]
        public string? InveaDnsCrrRdata { get; set; }

        [JsonPropertyName("INVEA_DNS_CRR_RDATA_LEN")]
        public int InveaDnsCrrRdataLen { get; set; }

        [JsonPropertyName("INVEA_DNS_CRR_TTL")]
        public int InveaDnsCrrTtl { get; set; }

        [JsonPropertyName("INVEA_DNS_CRR_TYPE")]
        public int InveaDnsCrrType { get; set; }

        [JsonPropertyName("INVEA_DNS_FLAGS_CODES_REQUEST")]
        public int InveaDnsFlagsCodesRequest { get; set; }

        [JsonPropertyName("INVEA_DNS_FLAGS_CODES_RESPONSE")]
        public int InveaDnsFlagsCodesResponse { get; set; }

        [JsonPropertyName("INVEA_DNS_ID")]
        public int InveaDnsId { get; set; }

        [JsonPropertyName("INVEA_DNS_QCLASS")]
        public int InveaDnsQclass { get; set; }

        [JsonPropertyName("INVEA_DNS_QNAME")]
        public string? InveaDnsQname { get; set; }

        [JsonPropertyName("INVEA_DNS_QTYPE")]
        public int InveaDnsQtype { get; set; }

        [JsonPropertyName("INVEA_DNS_QUESTION_COUNT")]
        public int InveaDnsQuestionCount { get; set; }

        [JsonPropertyName("L3_IPV4_DST")]
        public string? L3Ipv4Dst { get; set; }

        [JsonPropertyName("L3_IPV4_SRC")]
        public string? L3Ipv4Src { get; set; }

        [JsonPropertyName("L3_IPV6_DST")]
        public string? L3Ipv6Dst { get; set; }

        [JsonPropertyName("L3_IPV6_SRC")]
        public string? L3Ipv6Src { get; set; }

        [JsonPropertyName("L3_PROTO")]
        public int L3Proto { get; set; }

        [JsonPropertyName("L4_PORT_DST")]
        public int L4PortDst { get; set; }

        [JsonPropertyName("L4_PORT_SRC")]
        public int L4PortSrc { get; set; }

        [JsonPropertyName("L4_PROTO")]
        public int L4Proto { get; set; }

        [JsonPropertyName("ONE")]
        public long One { get; set; }

        [JsonPropertyName("PACKETS_B")]
        public long PacketsB { get; set; }

        [JsonPropertyName("PACKETS_A")]
        public long PacketsA { get; set; }

        [JsonPropertyName("START_NSEC")]
        public DateTime StartNsec { get; set; }

        [JsonPropertyName("START_NSEC_A")]
        public DateTime StartNsecA { get; set; }

        [JsonPropertyName("START_NSEC_B")]
        public DateTime StartNsecB { get; set; }

        [JsonPropertyName("NBAR_APP_ID")]
        public int NbarAppId { get; set; }

        [JsonPropertyName("NBAR_NAME")]
        public string? NbarName { get; set; }

        [JsonPropertyName("TLS_CIPHER_SUITE")]
        public int TlsCipherSuite { get; set; }

        [JsonPropertyName("TLS_CIPHER_SUITES")]
        public string? TlsCipherSuites { get; set; }

        [JsonPropertyName("TLS_CLIENT_RANDOM")]
        public string? TlsClientRandom { get; set; }

        [JsonPropertyName("TLS_CLIENT_SESSION_ID")]
        public string? TlsClientSessionId { get; set; }

        [JsonPropertyName("TLS_CLIENT_VERSION")]
        public int TlsClientVersion { get; set; }

        [JsonPropertyName("TLS_CONTENT_TYPE")]
        public int TlsContentType { get; set; }

        [JsonPropertyName("TLS_EC_POINT_FORMATS")]
        public string? TlsEcPointFormats { get; set; }

        [JsonPropertyName("TLS_ELLIPTIC_CURVES")]
        public string? TlsEllipticCurves { get; set; }

        [JsonPropertyName("TLS_EXTENSION_LENGTHS")]
        public string? TlsExtensionLengths { get; set; }

        [JsonPropertyName("TLS_EXTENSION_TYPES")]
        public string? TlsExtensionTypes { get; set; }

        [JsonPropertyName("TLS_HANDSHAKE_TYPE")]
        public int TlsHandshakeType { get; set; }

        [JsonPropertyName("TLS_JA3_FINGERPRINT")]
        public string? TlsJa3Fingerprint { get; set; }

        [JsonPropertyName("TLS_SERVER_RANDOM")]
        public string? TlsServerRandom { get; set; }

        [JsonPropertyName("TLS_SERVER_SESSION_ID")]
        public string? TlsServerSessionId { get; set; }

        [JsonPropertyName("TLS_SERVER_VERSION")]
        public int TlsServerVersion { get; set; }

        [JsonPropertyName("TLS_SETUP_TIME")]
        public long TlsSetupTime { get; set; }

        [JsonPropertyName("TLS_ALPN")]
        public string? TlsAlpn { get; set; }

        [JsonPropertyName("TLS_SNI")]
        public string? TlsSni { get; set; }

        [JsonPropertyName("TLS_SNI_LENGTH")]
        public int TlsSniLength { get; set; }

        [JsonPropertyName("TLS_CLIENT_KEY_LENGTH")]
        public int TlsClientKeyLength { get; set; }

        [JsonPropertyName("TLS_ISSUER_CN")]
        public string? TlsIssuerCn { get; set; }

        [JsonPropertyName("TLS_PUBLIC_KEY_ALG")]
        public long TlsPublicKeyAlg { get; set; }

        [JsonPropertyName("TLS_PUBLIC_KEY_LENGTH")]
        public long TlsPublicKeyLength { get; set; }

        [JsonPropertyName("TLS_SIGNATURE_ALG")]
        public long TlsSignatureAlg { get; set; }

        [JsonPropertyName("TLS_SUBJECT_CN")]
        public string? TlsSubjectCn { get; set; }

        [JsonPropertyName("TLS_SUBJECT_ON")]
        public string? TlsSubjectOn { get; set; }

        [JsonPropertyName("TLS_VALIDITY_NOT_AFTER")]
        public long TlsValidityNotAfter { get; set; }

        [JsonPropertyName("TLS_VALIDITY_NOT_BEFORE")]
        public long TlsValidityNotBefore { get; set; }

        [JsonPropertyName("HTTP_METHOD_MASK")]
        public int HttpMethodMask { get; set; }

        [JsonPropertyName("HTTP_REQUEST_AGENT")]
        public string? HttpRequestAgent { get; set; }

        [JsonPropertyName("HTTP_REQUEST_HOST")]
        public string? HttpRequestHost { get; set; }

        [JsonPropertyName("HTTP_REQUEST_REFERER")]
        public Uri? HttpRequestReferer { get; set; }

        [JsonPropertyName("HTTP_REQUEST_URL")]
        public string? HttpRequestUrl { get; set; }

        [JsonPropertyName("HTTP_REQUEST_URL_SHORT")]
        public string? HttpRequestUrlShort { get; set; }

        [JsonPropertyName("HTTP_RESPONSE_STATUS_CODE")]
        public int HttpResponseStatusCode { get; set; }

        [JsonPropertyName("HTTP_UA_APP")]
        public int HttpUaApp { get; set; }

        [JsonPropertyName("HTTP_UA_APP_MAJ")]
        public int HttpUaAppMaj { get; set; }

        [JsonPropertyName("HTTP_UA_APP_MIN")]
        public int HttpUaAppMin { get; set; }

        [JsonPropertyName("HTTP_UA_OS")]
        public int HttpUaOs { get; set; }

        [JsonPropertyName("HTTP_UA_OS_MAJ")]
        public int HttpUaOsMaj { get; set; }

        [JsonPropertyName("HTTP_UA_OS_MIN")]
        public int HttpUaOsMin { get; set; }
    }
}



