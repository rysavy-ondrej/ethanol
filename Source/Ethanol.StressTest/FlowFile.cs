using System.Text;
using Microsoft.Extensions.Logging;



internal class JsonSampleFile
{
    List<string> _samples;
    private readonly JsonFormatManipulator _sampleJsonFormat;
    private readonly ILogger? _logger;
    private int _currentFlowIndex;

    public JsonSampleFile(List<string> flows, JsonFormatManipulator flowJsonFormat, ILogger ? logger = null)
    {
        if (flows == null || flows.Count == 0) throw new ArgumentNullException(nameof(flows), "The list of flows cannot be null or empty.");
        if (flowJsonFormat == null) throw new ArgumentNullException(nameof(flowJsonFormat), "The flow json format manipulator cannot be null.");
        
        this._samples = flows;
        this._sampleJsonFormat = flowJsonFormat;
        this._logger = logger;
    }

    public static JsonSampleFile LoadFromFile(string flowFilePath, JsonFormatManipulator flowJsonFormat, int samplesCount, ILogger? logger = null)
    {
        var flows = new List<string>();
        using var reader = new StreamReader(flowFilePath);
        for(int i = 0; i < samplesCount; i++)
        {
            var record = ReadJsonString(reader);
            if (record == null) break;
            flows.Add(record);
        }
        logger?.LogInformation($"Loaded {flows.Count} samples from {flowFilePath}");
        return new JsonSampleFile(flows, flowJsonFormat,logger);
    }
    public string? GetNextSample()
    {
        if (_currentFlowIndex >= _samples.Count)
        {
            _currentFlowIndex = 0;
        }
        var sample = _samples[_currentFlowIndex];
        _currentFlowIndex++;
        return _sampleJsonFormat.UpdateRecord(sample);
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
            if (line == string.Empty || line=="[" || line=="]") continue;

            // Add without line delimiter to get a single line JSON               
            buffer.Append(line);

            // Check for the end of JSON object (either NDJSON or multiline JSON)
            if ((line.StartsWith("{") && line.EndsWith("}")) || line == "}") break;
        }
        var record = buffer.ToString();
        return string.IsNullOrWhiteSpace(record) ? null : record;
    }
}
