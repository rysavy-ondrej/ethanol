using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using Ethanol.ContextBuilder.Helpers;
public class FlowmonJsonFormatManipulator : JsonFormatManipulator
{
    private readonly byte[] _ipPrefixBytes;

    // flowmon-json timestamps:
    // "START_NSEC":"2023-12-19 14:00:48.093178043"
    // "START_NSEC_A":"2023-12-19 14:00:48.093178043"
    // "START_NSEC_B":"2023-12-19 14:00:48.133832750"
    // "END_NSEC":"2023-12-19 14:00:48.133832750"
    // "END_NSEC_A":"2023-12-19 14:00:48.093178043"
    // "END_NSEC_B":"2023-12-19 14:00:48.133832750"

    Random random = new Random();

    public FlowmonJsonFormatManipulator(IPAddressPrefix clientPrefix)
    {
        _ipPrefixBytes = clientPrefix.Address.GetAddressBytes()[..(clientPrefix.PrefixLength/8)];
    }

    public override bool UpdateField(string fieldName, JsonElement fieldValue, out JsonValue? newValue)
    {
        switch (fieldName)
        {
            case "START_NSEC":
            case "START_NSEC_A":
            case "START_NSEC_B":
                newValue = JsonValue.Create(DateTimeOffset.Now.UtcDateTime.ToString("yyyy-MM-dd HH:mm:ss.fffffff"));
                return true;
            case "END_NSEC":
            case "END_NSEC_A":
            case "END_NSEC_B":
                var duration = random.Next(10, 10000);
                newValue = JsonValue.Create(DateTimeOffset.Now.UtcDateTime.AddMilliseconds(duration).ToString("yyyy-MM-dd HH:mm:ss.fffffff"));
                return true;
            case "L3_IPV4_SRC":
            case "L3_IPV4_DST":
                if (random.Next(0, 100) < 50)
                {
                    newValue = JsonValue.Create(GetNextHostAddress(_ipPrefixBytes).ToString());
                    return true;
                }
                else
                {
                    newValue = null;
                    return false;
                }
            default:
                newValue = null;
                return false;
        }
    }
}
