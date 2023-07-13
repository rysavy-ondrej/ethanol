using System.Text.Json.Serialization;

namespace Ethanol.DataObjects
{
    public class HostContext
    {
        [JsonPropertyName("HostAddress")]
        public string HostAddress { get; set; }

        [JsonPropertyName("OperatingSystem")]
        public object OperatingSystem { get; set; }

        [JsonPropertyName("InitiatedConnections")]
        public List<InitiatedConnection> InitiatedConnections { get; set; }

        [JsonPropertyName("AcceptedConnections")]
        public List<AcceptedConnection> AcceptedConnections { get; set; }

        [JsonPropertyName("ResolvedDomains")]
        public List<ResolvedDomain> ResolvedDomains { get; set; }

        [JsonPropertyName("WebUrls")]
        public List<WebUrl> WebUrls { get; set; }

        [JsonPropertyName("TlsHandshakes")]
        public List<TlsHandshake> TlsHandshakes { get; set; }
    }


}