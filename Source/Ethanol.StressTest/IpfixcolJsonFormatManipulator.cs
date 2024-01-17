using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using Ethanol.ContextBuilder.Helpers;

public class IpfixcolJsonFormatManipulator : JsonFormatManipulator
{
    // ipfixcol-json timestamps:
    // "iana:flowStartMilliseconds":"2023-12-29T12:49:10.270Z",
    // "iana:flowEndMilliseconds":"2023-12-29T12:49:11.999Z",

    Random random = new Random();
    private IPAddressPrefix _clientPrefix;
    private readonly byte[] _ipPrefixBytes;

    public IpfixcolJsonFormatManipulator(IPAddressPrefix clientPrefix)
    {
        _clientPrefix = clientPrefix;
        _ipPrefixBytes = clientPrefix.Address.GetAddressBytes()[..(clientPrefix.PrefixLength/8)];
    }

    public override bool UpdateField(string fieldName, JsonElement fieldValue, out JsonValue? newValue)
    {
        switch (fieldName)
        {
            case "iana:flowStartMilliseconds":
                newValue = JsonValue.Create(DateTimeOffset.Now.UtcDateTime.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"));
                return true;
            case "iana:flowEndMilliseconds":
                var duration = random.Next(10, 10000);
                newValue = JsonValue.Create(DateTimeOffset.Now.UtcDateTime.AddMilliseconds(duration).ToString("yyyy-MM-ddTHH:mm:ss.fffZ"));
                return true;
            case "iana:sourceIPv4Address": 
            case "iana:destinationIPv4Address":
                if (random.Next(0, 100) < 50)
                {
                    newValue = newValue = JsonValue.Create(GetNextHostAddress(_ipPrefixBytes).ToString());
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