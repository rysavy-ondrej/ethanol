using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;

public class TagJsonFormatManipulator : JsonFormatManipulator
{
    Random _random = new Random();

    byte[] _addressPrefix;

    public TagJsonFormatManipulator(byte[] addressPrefix)
    {
        if (addressPrefix.Length > 4) throw new ArgumentOutOfRangeException(nameof(addressPrefix), "The address prefix length must be less or equal to 4 bytes.");
        _addressPrefix = addressPrefix;
    }

    public override bool UpdateField(string fieldName, JsonElement fieldValue, out JsonValue? newValue)
    {
        switch (fieldName)
        {
            case "key":
                newValue = JsonValue.Create(GetNextHostAddress().ToString());
                return true;
            case "validity":
                newValue = JsonValue.Create(String.Empty);
                return true;
            default:
                newValue = null;
                return false;
        }
    }

    private IPAddress GetNextHostAddress()
    {
        var addressBytes = new byte[4];
        var prefixLen = _addressPrefix.Length;
        _addressPrefix.CopyTo(addressBytes, 0);
        for(int i = prefixLen; i < 4; i++)
        {
            addressBytes[i] = (byte)_random.Next(0, 255);
        }
        return new IPAddress(addressBytes); 
    }
}
