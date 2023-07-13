using System.Text.Json.Serialization;

namespace Ethanol.DataObjects
{
    public class AcceptedConnection
    {
        [JsonPropertyName("RemoteHostAddress")]
        public string RemoteHostAddress { get; set; }

        [JsonPropertyName("RemoteHostName")]
        public object RemoteHostName { get; set; }

        [JsonPropertyName("RemotePort")]
        public int? RemotePort { get; set; }

        [JsonPropertyName("Service")]
        public string Service { get; set; }

        [JsonPropertyName("Flows")]
        public int? Flows { get; set; }

        [JsonPropertyName("PacketsSent")]
        public int? PacketsSent { get; set; }

        [JsonPropertyName("OctetsSent")]
        public int? OctetsSent { get; set; }

        [JsonPropertyName("PacketsRecv")]
        public int? PacketsRecv { get; set; }

        [JsonPropertyName("OctetsRecv")]
        public int? OctetsRecv { get; set; }
    }


}