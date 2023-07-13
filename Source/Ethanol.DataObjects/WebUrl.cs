using System.Text.Json.Serialization;

namespace Ethanol.DataObjects
{

    public class WebUrl
    {
        [JsonPropertyName("RemoteHostAddress")]
        public string RemoteHostAddress { get; set; }

        [JsonPropertyName("RemoteHostName")]
        public string RemoteHostName { get; set; }

        [JsonPropertyName("RemotePort")]
        public int? RemotePort { get; set; }

        [JsonPropertyName("ApplicationProcessName")]
        public object ApplicationProcessName { get; set; }

        [JsonPropertyName("Method")]
        public string Method { get; set; }

        [JsonPropertyName("Url")]
        public string Url { get; set; }
    }


}