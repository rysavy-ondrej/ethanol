using System.Text.Json;
using System.Text.Json.Nodes;
using Ethanol.ContextBuilder.Helpers;

public class TagJsonFormatManipulator : JsonFormatManipulator
{
    byte[] _addressPrefix;

    public TagJsonFormatManipulator(IPAddressPrefix clientPrefix)
    {
        _addressPrefix = clientPrefix.Address.GetAddressBytes()[..(clientPrefix.PrefixLength/8)];
    }

    public override bool UpdateField(string fieldName, JsonElement fieldValue, out JsonValue? newValue)
    {
        switch (fieldName)
        {
            case "key":
                newValue = JsonValue.Create(GetNextHostAddress(_addressPrefix).ToString());
                return true;
            case "validity":
                newValue = JsonValue.Create(String.Empty);
                return true;
            default:
                newValue = null;
                return false;
        }
    }
}
