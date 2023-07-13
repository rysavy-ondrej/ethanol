using System.Text.Json.Serialization;

namespace Ethanol.DataObjects
{
    public class HostContextEvent
    {
        [JsonPropertyName("StartTime")]
        public DateTime? StartTime { get; set; }

        [JsonPropertyName("EndTime")]
        public DateTime? EndTime { get; set; }

        [JsonPropertyName("Payload")]
        public HostContext Payload { get; set; }
    }
}