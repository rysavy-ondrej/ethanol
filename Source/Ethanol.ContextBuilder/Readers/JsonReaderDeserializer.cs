using Ethanol.ContextBuilder.Context;
using Ethanol.ContextBuilder.Serialization;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Ethanol.ContextBuilder.Readers
{
    /// <summary>
    /// Deserializer for JSON and NDJSON (Newline Delimited JSON) formats.
    /// </summary>
    /// <typeparam name="TEntryType">The type of the entry to deserialize.</typeparam>
    public class JsonReaderDeserializer<TEntryType>
    {
        private readonly JsonSerializerOptions _serializerOptions;
        private readonly Func<TEntryType, IpFlow> _mapper;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonReaderDeserializer{TEntryType}"/> class.
        /// </summary>
        /// <param name="mapper">The function used to map the deserialized entry type to an <see cref="IpFlow"/>.</param>
        /// <param name="logger">The logger used for logging.</param>
        public JsonReaderDeserializer(Func<TEntryType,IpFlow> mapper,  ILogger logger)
        {

            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger;
            _serializerOptions = new JsonSerializerOptions();
            _serializerOptions.Converters.Add(new DateTimeJsonConverter());
        }

        /// <summary>
        /// Tries to deserialize the input string into an IpFlow object.
        /// </summary>
        /// <param name="input">The input string to deserialize.</param>
        /// <param name="ipFlow">When this method returns, contains the deserialized IpFlow object if the deserialization was successful, or the default IpFlow object if the deserialization failed.</param>
        /// <returns><c>true</c> if the deserialization was successful; otherwise, <c>false</c>.</returns>
        public bool TryDeserializeFlow(string input, out IpFlow ipFlow)
        {
            try
            {
                var entry = JsonSerializer.Deserialize<TEntryType>(input, _serializerOptions);
                ipFlow = _mapper(entry);
                return true;
            }
            catch (JsonException e)
            {
                _logger?.LogWarning($"Cannot deserialize record: {e.Message}");
                _logger?.LogDebug($"Input fragment: ...{GetErrorSubstring(input, (int)e.BytePositionInLine)}...");
                ipFlow = default;
                return false;
            }
            catch (Exception e)
            {
                _logger?.LogWarning($"Cannot map record to flow: {e.Message}");
                ipFlow = default;
                return false;
            }
        }

        /// <summary>
        /// Retrieves a substring from the given line centered around the specified position.
        /// </summary>
        /// <param name="line">The line of text.</param>
        /// <param name="position">The position of the substring within the line.</param>
        /// <returns>The substring centered around the specified position.</returns>
        string GetErrorSubstring(string line, int position)
        {
            if (string.IsNullOrEmpty(line)) return string.Empty;
            var start = System.Math.Max(0, position - 30);
            var end = System.Math.Min(line.Length, position + 30);
            return line.Substring(start, end - start);
        }   

        /// <summary>
        /// Reads a JSON string from the input stream. This method supports reading both NDJSON (Newline Delimited JSON) 
        /// where each line is a complete JSON object, and multi-line formatted JSON until it reaches the end of an object.
        /// </summary>
        /// <param name="inputStream">The TextReader stream to read the JSON string from.</param>
        /// <exception cref="OperationCanceledException">The operation was cancelled.</exception>
        /// <returns>A string representation of the JSON object, or null if the end of the file is reached or the content is whitespace.</returns>
        public async Task<string> ReadJsonStringAsync(TextReader inputStream, CancellationToken ct)
        {
            var buffer = new StringBuilder();

            while (true)
            {
                var line = (await inputStream.ReadLineAsync(ct))?.Trim();

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
}
