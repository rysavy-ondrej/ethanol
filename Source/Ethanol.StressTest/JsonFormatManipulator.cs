using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using Ethanol.ContextBuilder.Helpers;

public abstract class JsonFormatManipulator
{
    Random _random = new Random();
    protected IPAddress GetNextHostAddress(byte[] addressPrefix)
    {
        var addressBytes = new byte[4];
        var prefixLen = addressPrefix.Length;
        addressPrefix.CopyTo(addressBytes, 0);
        for(int i = prefixLen; i < 4; i++)
        {
            addressBytes[i] = (byte)_random.Next(0, 255);
        }
        return new IPAddress(addressBytes); 
    }
    public abstract bool UpdateField(string fieldName, JsonElement fieldValue, out JsonValue? newValue);
    public string UpdateRecord(string flowJson)
    {
        var fields = new List<string>();
        using (JsonDocument doc = JsonDocument.Parse(flowJson))
        {
            JsonElement root = doc.RootElement;
            foreach (var prop in root.EnumerateObject())
            {
                if (UpdateField(prop.Name, prop.Value, out var newValue))
                {
                    fields.Add($"\"{prop.Name}\":{newValue?.ToJsonString()}");
                }
                else
                {
                    fields.Add($"\"{prop.Name}\":{ToJsonString(prop.Value)}");
                }
            }
        }
        return "{" + String.Join(',', fields) + "}\n";
    }

    string ToJsonString(JsonElement element)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.String:
                return $"\"{element.GetString()}\"";
            case JsonValueKind.Number:
                return element.GetRawText();
            case JsonValueKind.True:
                return "true";
            case JsonValueKind.False:
                return "false";
            case JsonValueKind.Null:
                return "null";
            case JsonValueKind.Undefined:
                return "undefined";
            case JsonValueKind.Object:
                return element.GetRawText();
            case JsonValueKind.Array:
                return element.GetRawText();
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}
