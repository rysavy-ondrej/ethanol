using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;

public class IpfixcolJsonFormatManipulator : FlowJsonFormatManipulator
{
    // ipfixcol-json timestamps:
    // "iana:flowStartMilliseconds":"2023-12-29T12:49:10.270Z",
    // "iana:flowEndMilliseconds":"2023-12-29T12:49:11.999Z",

    Random random = new Random();
    private bool _randomizeAddresses;

    public IpfixcolJsonFormatManipulator(bool randomizeAddresses)
    {
        this._randomizeAddresses = randomizeAddresses;
    }

    public override bool UpdateField(string fieldName, JsonElement fieldValue, out JsonValue? newValue)
    {
        switch (fieldName)
        {
            case "iana:flowStartMilliseconds":
                newValue = JsonValue.Create(DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"));
                return true;
            case "iana:flowEndMilliseconds":
                var duration = random.Next(10, 10000);
                newValue = JsonValue.Create(DateTime.Now.AddMilliseconds(duration).ToString("yyyy-MM-ddTHH:mm:ss.fffZ"));
                return true;
            case "iana:sourceIPv4Address": 
                if (_randomizeAddresses)
                {
                    var bytes = new byte[4];
                    random.NextBytes(bytes);
                    newValue = JsonValue.Create(new IPAddress(bytes).ToString());
                    return true;
                }
                else
                {
                    newValue = null;
                    return false;
                }
            case "iana:destinationIPv4Address":
                if (_randomizeAddresses)
                {
                    var bytes = new byte[4];
                    random.NextBytes(bytes);
                    newValue = JsonValue.Create(new IPAddress(bytes).ToString());
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