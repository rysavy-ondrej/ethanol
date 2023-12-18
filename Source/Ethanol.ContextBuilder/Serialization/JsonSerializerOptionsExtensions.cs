using System.IO;
using System.Text.Json;

namespace Ethanol.ContextBuilder.Serialization
{
    /// <summary>
    /// Provides extension methods for <see cref="JsonSerializerOptions"/> to enhance serialization capabilities.
    /// </summary>
    /// <remarks>
    /// This class aims to provide additional customization and flexibility to the JSON serialization process by introducing
    /// custom converters, specifically tailored for IP address conversion.
    /// </remarks>
    public static class JsonSerializerOptionsExtensions
    {
        /// <summary>
        /// Adds the <see cref="IPAddressJsonConverter"/> to the list of converters for the provided serialization options.
        /// </summary>
        /// <param name="options">The JSON serializer options to which the converter should be added.</param>
        public static void AddIPAddressConverter(this JsonSerializerOptions options)
        {
            options.Converters.Add(new IPAddressJsonConverter());
        }
    }

    public static class Json
    {
        static JsonSerializerOptions options = new JsonSerializerOptions{ PropertyNameCaseInsensitive = true };
        static Json()
        {
            options.AddIPAddressConverter();
        }
        public static string Serialize<T>(T value)
        {
            return JsonSerializer.Serialize<T>(value, options);
        }
        public static T Deserialize<T>(string value)
        {
            return JsonSerializer.Deserialize<T>(value, options);
        }
        public static T Deserialize<T>(Stream stream)
        {
            return JsonSerializer.Deserialize<T>(stream, options);
        }
    }
}
