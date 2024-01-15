using System.Text;



internal class JsonSampleFile
{
    List<string> _samples;
    private readonly JsonFormatManipulator _flowJsonFormat;
    private int _currentFlowIndex;

    public JsonSampleFile(List<string> flows, JsonFormatManipulator flowJsonFormat)
    {
        if (flows == null || flows.Count == 0) throw new ArgumentNullException(nameof(flows), "The list of flows cannot be null or empty.");
        if (flowJsonFormat == null) throw new ArgumentNullException(nameof(flowJsonFormat), "The flow json format manipulator cannot be null.");
        
        this._samples = flows;
        this._flowJsonFormat = flowJsonFormat;
    }

    public static JsonSampleFile LoadFromFile(string flowFilePath, JsonFormatManipulator flowJsonFormat, int samplesCount)
    {
        var flows = new List<string>();
        using var reader = new StreamReader(flowFilePath);
        for(int i = 0; i < samplesCount; i++)
        {
            var record = ReadJsonString(reader);
            if (record == null) break;
            flows.Add(record);
        }
        return new JsonSampleFile(flows, flowJsonFormat);
    }
    public string? GetNextSample()
    {
        if (_currentFlowIndex >= _samples.Count)
        {
            _currentFlowIndex = 0;
        }
        var sample = _samples[_currentFlowIndex];
        _currentFlowIndex++;

        return _flowJsonFormat.UpdateRecord(sample);
    }

    public static string? ReadJsonString(TextReader inputStream)
    {
        var buffer = new StringBuilder();

        while (true)
        {
            var line = inputStream.ReadLine()?.Trim();

            // End of file?
            if (line == null) break;

            // Skip empty lines
            if (line == string.Empty) continue;

            // Add without line delimiter to get a single line JSON               
            buffer.Append(line);

            // Check for the end of JSON object (either NDJSON or multiline JSON)
            if ((line.StartsWith("{") && line.EndsWith("}")) || line == "}") break;
        }
        var record = buffer.ToString();
        return string.IsNullOrWhiteSpace(record) ? null : record;
    }
}
