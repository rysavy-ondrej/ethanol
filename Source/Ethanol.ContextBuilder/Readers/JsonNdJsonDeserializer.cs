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
    public class JsonNdJsonDeserializer<TEntryType>
    {
        private readonly JsonSerializerOptions _serializerOptions;
        private readonly Func<TEntryType, IpFlow> _mapper;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonNdJsonDeserializer{TEntryType}"/> class.
        /// </summary>
        /// <param name="mapper">The function used to map the deserialized entry type to an <see cref="IpFlow"/>.</param>
        /// <param name="logger">The logger used for logging.</param>
        public JsonNdJsonDeserializer(Func<TEntryType,IpFlow> mapper,  ILogger logger)
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
            catch (Exception e)
            {
                _logger?.LogWarning($"Cannot deserialize flow: {e.Message}");
                ipFlow = default;
                return false;
            }
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

                buffer.AppendLine(line);

                // Check for the end of JSON object (either NDJSON or multiline JSON)
                if ((line.StartsWith("{") && line.EndsWith("}")) || line == "}") break;
            }
            var record = buffer.ToString().Trim();
            return string.IsNullOrWhiteSpace(record) ? null : record;
        }
    }
}
