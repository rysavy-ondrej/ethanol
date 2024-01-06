using System.Text;

internal class FlowFile
{
    List<string> _flows;
    private readonly FlowJsonFormatManipulator _flowJsonFormat;
    private int _currentFlowIndex;

    public FlowFile(List<string> flows, FlowJsonFormatManipulator flowJsonFormat)
    {
        if (flows == null || flows.Count == 0) throw new ArgumentNullException(nameof(flows), "The list of flows cannot be null or empty.");
        if (flowJsonFormat == null) throw new ArgumentNullException(nameof(flowJsonFormat), "The flow json format manipulator cannot be null.");
        
        this._flows = flows;
        this._flowJsonFormat = flowJsonFormat;
    }

    public static FlowFile LoadFromFile(string flowFilePath, FlowJsonFormatManipulator flowJsonFormat)
    {
        var flows = new List<string>();
        using var reader = new StreamReader(flowFilePath);
        while (true)
        {
            var record = ReadJsonString(reader);
            if (record == null) break;
            flows.Add(record);
        }
        return new FlowFile(flows, flowJsonFormat);
    }
    public string? GetNextFlow()
    {
        if (_currentFlowIndex >= _flows.Count)
        {
            _currentFlowIndex = 0;
        }
        var flow = _flows[_currentFlowIndex];
        _currentFlowIndex++;

        return _flowJsonFormat.UpdateFlow(flow);
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
