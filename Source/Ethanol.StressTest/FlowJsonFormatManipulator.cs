using System.Text.Json;
using System.Text.Json.Nodes;

public abstract class FlowJsonFormatManipulator
{
    public abstract bool UpdateField(string fieldName, JsonElement fieldValue, out JsonValue? newValue);
    public string UpdateFlow(string flowJson)
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
        return "{" + String.Join(',', fields) + "}";
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
