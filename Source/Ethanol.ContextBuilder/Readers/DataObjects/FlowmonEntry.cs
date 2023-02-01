using System;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ethanol.ContextBuilder.Readers.DataObjects
{
    /// <summary>
    /// POCO for JSON record created by flowmonexp5 tool.
    /// </summary>
    internal class FlowmonexpEntry
    {
        [JsonPropertyName("BYTES")]
        public long Bytes { get; set; }

        [JsonPropertyName("BYTES_A")]
        public long BytesA { get; set; }

        [JsonPropertyName("END_NSEC")]
        public DateTime EndNsec { get; set; }

        [JsonPropertyName("END_NSEC_A")]
        public DateTime EndNsecA { get; set; }

        [JsonPropertyName("END_NSEC_B")]
        public DateTime EndNsecB { get; set; }

        [JsonPropertyName("INVEA_DNS_ADDTREC_COUNT_REQUEST")]
        public long InveaDnsAddtrecCountRequest { get; set; }

        [JsonPropertyName("INVEA_DNS_ADDTREC_COUNT_RESPONSE")]
        public long InveaDnsAddtrecCountResponse { get; set; }

        [JsonPropertyName("INVEA_DNS_ANSWREC_COUNT_REQUEST")]
        public long InveaDnsAnswrecCountRequest { get; set; }

        [JsonPropertyName("INVEA_DNS_ANSWREC_COUNT_RESPONSE")]
        public long InveaDnsAnswrecCountResponse { get; set; }

        [JsonPropertyName("INVEA_DNS_AUTHREC_COUNT_REQUEST")]
        public long InveaDnsAuthrecCountRequest { get; set; }

        [JsonPropertyName("INVEA_DNS_AUTHREC_COUNT_RESPONSE")]
        public long InveaDnsAuthrecCountResponse { get; set; }

        [JsonPropertyName("INVEA_DNS_CRR_CLASS")]
        public long InveaDnsCrrClass { get; set; }

        [JsonPropertyName("INVEA_DNS_CRR_NAME")]
        public string InveaDnsCrrName { get; set; }

        [JsonPropertyName("INVEA_DNS_CRR_RDATA")]
        public string InveaDnsCrrRdata { get; set; }

        [JsonPropertyName("INVEA_DNS_CRR_RDATA_LEN")]
        public long InveaDnsCrrRdataLen { get; set; }

        [JsonPropertyName("INVEA_DNS_CRR_TTL")]
        public long InveaDnsCrrTtl { get; set; }

        [JsonPropertyName("INVEA_DNS_CRR_TYPE")]
        public long InveaDnsCrrType { get; set; }

        [JsonPropertyName("INVEA_DNS_FLAGS_CODES_REQUEST")]
        public long InveaDnsFlagsCodesRequest { get; set; }

        [JsonPropertyName("INVEA_DNS_FLAGS_CODES_RESPONSE")]
        public long InveaDnsFlagsCodesResponse { get; set; }

        [JsonPropertyName("INVEA_DNS_ID")]
        public long InveaDnsId { get; set; }

        [JsonPropertyName("INVEA_DNS_QCLASS")]
        public long InveaDnsQclass { get; set; }

        [JsonPropertyName("INVEA_DNS_QNAME")]
        public string InveaDnsQname { get; set; }

        [JsonPropertyName("INVEA_DNS_QTYPE")]
        public long InveaDnsQtype { get; set; }

        [JsonPropertyName("INVEA_DNS_QUESTION_COUNT")]
        public long InveaDnsQuestionCount { get; set; }

        [JsonPropertyName("L3_IPV4_DST")]
        public string L3Ipv4Dst { get; set; }

        [JsonPropertyName("L3_IPV4_SRC")]
        public string L3Ipv4Src { get; set; }

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

        [JsonPropertyName("PACKETS")]
        public long Packets { get; set; }

        [JsonPropertyName("PACKETS_A")]
        public long PacketsA { get; set; }

        [JsonPropertyName("QUEUE_ID")]
        public int QueueId { get; set; }

        [JsonPropertyName("SAMPLING_ALGORITHM")]
        public long SamplingAlgorithm { get; set; }

        [JsonPropertyName("SAMPLING_RATE")]
        public long SamplingRate { get; set; }

        [JsonPropertyName("START_NSEC")]
        public DateTime StartNsec { get; set; }

        [JsonPropertyName("START_NSEC_A")]
        public DateTime StartNsecA { get; set; }

        [JsonPropertyName("START_NSEC_B")]
        public DateTime StartNsecB { get; set; }

        [JsonPropertyName("NBAR_APP_ID")]
        public int NbarAppId { get; set; }

        [JsonPropertyName("NBAR_NAME")]
        public string NbarName { get; set; }

        [JsonPropertyName("TLS_CIPHER_SUITE")]
        public long TlsCipherSuite { get; set; }

        [JsonPropertyName("TLS_CIPHER_SUITES")]
        public string TlsCipherSuites { get; set; }

        [JsonPropertyName("TLS_CLIENT_RANDOM")]
        public string TlsClientRandom { get; set; }

        [JsonPropertyName("TLS_CLIENT_SESSION_ID")]
        public string TlsClientSessionId { get; set; }

        [JsonPropertyName("TLS_CLIENT_VERSION")]
        public long TlsClientVersion { get; set; }

        [JsonPropertyName("TLS_CONTENT_TYPE")]
        public long TlsContentType { get; set; }

        [JsonPropertyName("TLS_EC_POINT_FORMATS")]
        public string TlsEcPointFormats { get; set; }

        [JsonPropertyName("TLS_ELLIPTIC_CURVES")]
        public string TlsEllipticCurves { get; set; }

        [JsonPropertyName("TLS_EXTENSION_LENGTHS")]
        public string TlsExtensionLengths { get; set; }

        [JsonPropertyName("TLS_EXTENSION_TYPES")]
        public string TlsExtensionTypes { get; set; }

        [JsonPropertyName("TLS_HANDSHAKE_TYPE")]
        public long TlsHandshakeType { get; set; }

        [JsonPropertyName("TLS_JA3_FINGERPRINT")]
        public string TlsJa3Fingerprint { get; set; }

        [JsonPropertyName("TLS_SERVER_RANDOM")]
        public string TlsServerRandom { get; set; }

        [JsonPropertyName("TLS_SERVER_SESSION_ID")]
        public string TlsServerSessionId { get; set; }

        [JsonPropertyName("TLS_SERVER_VERSION")]
        public long TlsServerVersion { get; set; }

        [JsonPropertyName("TLS_SETUP_TIME")]
        public long TlsSetupTime { get; set; }

        [JsonPropertyName("TLS_SNI")]
        public string TlsSni { get; set; }

        [JsonPropertyName("TLS_SNI_LENGTH")]
        public long TlsSniLength { get; set; }

        [JsonPropertyName("TLS_CLIENT_KEY_LENGTH")]
        public long TlsClientKeyLength { get; set; }

        [JsonPropertyName("TLS_ISSUER_CN")]
        public string TlsIssuerCn { get; set; }

        [JsonPropertyName("TLS_PUBLIC_KEY_ALG")]
        public long TlsPublicKeyAlg { get; set; }

        [JsonPropertyName("TLS_PUBLIC_KEY_LENGTH")]
        public long TlsPublicKeyLength { get; set; }

        [JsonPropertyName("TLS_SIGNATURE_ALG")]
        public long TlsSignatureAlg { get; set; }

        [JsonPropertyName("TLS_SUBJECT_CN")]
        public string TlsSubjectCn { get; set; }

        [JsonPropertyName("TLS_SUBJECT_ON")]
        public string TlsSubjectOn { get; set; }

        [JsonPropertyName("TLS_VALIDITY_NOT_AFTER")]
        public long TlsValidityNotAfter { get; set; }

        [JsonPropertyName("TLS_VALIDITY_NOT_BEFORE")]
        public long TlsValidityNotBefore { get; set; }

        [JsonPropertyName("HTTP_METHOD_MASK")]
        public long HttpMethodMask { get; set; }

        [JsonPropertyName("HTTP_REQUEST_AGENT")]
        public string HttpRequestAgent { get; set; }

        [JsonPropertyName("HTTP_REQUEST_HOST")]
        public string HttpRequestHost { get; set; }

        [JsonPropertyName("HTTP_REQUEST_REFERER")]
        public Uri HttpRequestReferer { get; set; }

        [JsonPropertyName("HTTP_REQUEST_URL")]
        public string HttpRequestUrl { get; set; }

        [JsonPropertyName("HTTP_REQUEST_URL_SHORT")]
        public string HttpRequestUrlShort { get; set; }

        [JsonPropertyName("HTTP_RESPONSE_STATUS_CODE")]
        public long HttpResponseStatusCode { get; set; }

        [JsonPropertyName("HTTP_UA_APP")]
        public long HttpUaApp { get; set; }

        [JsonPropertyName("HTTP_UA_APP_MAJ")]
        public long HttpUaAppMaj { get; set; }

        [JsonPropertyName("HTTP_UA_APP_MIN")]
        public long HttpUaAppMin { get; set; }

        [JsonPropertyName("HTTP_UA_OS")]
        public long HttpUaOs { get; set; }

        [JsonPropertyName("HTTP_UA_OS_MAJ")]
        public long HttpUaOsMaj { get; set; }

        [JsonPropertyName("HTTP_UA_OS_MIN")]
        public long HttpUaOsMin { get; set; }

        public static bool TryDeserialize(string input, out FlowmonexpEntry entry)
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.Converters.Add(new DateTimeConverter());
            try
            {
                entry = JsonSerializer.Deserialize<FlowmonexpEntry>(input, options);
                return true;
            }
            catch (Exception e)
            {
                entry = default;
                return false;
            }
        }
    }
    public class DateTimeConverter : JsonConverter<DateTime>
    {
        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            Debug.Assert(typeToConvert == typeof(DateTime));
            var datetime = DateTime.Parse(reader.GetString() ?? string.Empty);
            return datetime;
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}



