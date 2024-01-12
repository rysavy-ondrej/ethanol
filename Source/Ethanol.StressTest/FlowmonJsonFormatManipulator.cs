using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;

public class FlowmonJsonFormatManipulator : FlowJsonFormatManipulator
{
    // flowmon-json timestamps:
    // "START_NSEC":"2023-12-19 14:00:48.093178043"
    // "START_NSEC_A":"2023-12-19 14:00:48.093178043"
    // "START_NSEC_B":"2023-12-19 14:00:48.133832750"
    // "END_NSEC":"2023-12-19 14:00:48.133832750"
    // "END_NSEC_A":"2023-12-19 14:00:48.093178043"
    // "END_NSEC_B":"2023-12-19 14:00:48.133832750"

    Random random = new Random();
    private bool _randomizeAddresses;

    public FlowmonJsonFormatManipulator(bool randomizeAddresses)
    {
        this._randomizeAddresses = randomizeAddresses;
    }

    public override bool UpdateField(string fieldName, JsonElement fieldValue, out JsonValue? newValue)
    {
        switch (fieldName)
        {
            case "START_NSEC":
            case "START_NSEC_A":
            case "START_NSEC_B":
                newValue = JsonValue.Create(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fffffff"));
                return true;
            case "END_NSEC":
            case "END_NSEC_A":
            case "END_NSEC_B":
                var duration = random.Next(10, 10000);
                newValue = JsonValue.Create(DateTime.Now.AddMilliseconds(duration).ToString("yyyy-MM-dd HH:mm:ss.fffffff"));
                return true;
            case "L3_IPV4_SRC":
                if (_randomizeAddresses)
                {
                    return GetRandomizedAddress(fieldValue.GetString(),out newValue);
                }
                else
                {
                    newValue = null;
                    return false;
                }
            case "L3_IPV4_DST":
                 if (_randomizeAddresses)
                {
                    return GetRandomizedAddress(fieldValue.GetString(),out newValue);
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

    private bool GetRandomizedAddress(string? fieldValue, out JsonValue? value)
    {
        if (fieldValue != null && IPAddress.TryParse(fieldValue, out var adr))
        {
            var bytes = adr.GetAddressBytes();
            random.NextBytes(new Span<byte>(bytes, 2,2));
            value = JsonValue.Create(new IPAddress(bytes).ToString());
            return true;
        }
        value = default;
        return false;
    }
}
