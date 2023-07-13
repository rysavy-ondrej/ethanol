using System.Text.Json.Serialization;

namespace Ethanol.DataObjects
{
    public class ResolvedDomain
    {
        [JsonPropertyName("DnsServer")]
        public string DnsServer { get; set; }

        [JsonPropertyName("QueryString")]
        public string QueryString { get; set; }

        [JsonPropertyName("ResponseData")]
        public string ResponseData { get; set; }

        [JsonPropertyName("ResponseCode")]
        public int? ResponseCode { get; set; }
    }


}