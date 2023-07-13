using System.Text.Json.Serialization;

namespace Ethanol.DataObjects
{
    public class TlsHandshake
    {
        [JsonPropertyName("RemoteHostAddress")]
        public string RemoteHostAddress { get; set; }

        [JsonPropertyName("RemoteHostName")]
        public string RemoteHostName { get; set; }

        [JsonPropertyName("RemotePort")]
        public int? RemotePort { get; set; }

        [JsonPropertyName("ApplicationProcessName")]
        public string ApplicationProcessName { get; set; }

        [JsonPropertyName("ApplicationProtocol")]
        public string ApplicationProtocol { get; set; }

        [JsonPropertyName("ServerNameIndication")]
        public string ServerNameIndication { get; set; }

        [JsonPropertyName("JA3Fingerprint")]
        public string JA3Fingerprint { get; set; }

        [JsonPropertyName("IssuerName")]
        public string IssuerName { get; set; }

        [JsonPropertyName("SubjectName")]
        public string SubjectName { get; set; }

        [JsonPropertyName("OrganisationName")]
        public string OrganisationName { get; set; }

        [JsonPropertyName("CipherSuites")]
        public string CipherSuites { get; set; }

        [JsonPropertyName("EllipticCurves")]
        public string EllipticCurves { get; set; }
    }


}