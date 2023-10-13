using System;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace Ethanol.ContextBuilder.Serialization
{
    /// <summary>
    /// Provides custom YAML serialization and deserialization for strings, supporting references to environment variables.
    /// </summary>
    /// <remarks>
    /// This converter is tailored to handle strings in YAML that surrounded with '%". Such strings are treated as
    /// references to environment variables. During the deserialization process, these references are replaced with the corresponding
    /// values from the environment variables. If the referenced environment variable is not found, an empty string is returned.
    /// During the serialization process, the string is written as-is.
    /// </summary>
    public class EnvironmentStringConverter : IYamlTypeConverter
    {
        /// <summary>
        /// Determines whether this converter can convert the specified type.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <returns>True if the type is a string; otherwise, false.</returns>
        public bool Accepts(Type type)
        {
            return type == typeof(string);
        }

        /// <summary>
        /// Reads and converts the YAML representation of a string, potentially replacing it with the value of a referenced environment variable.
        /// </summary>
        /// <param name="parser">The YAML parser to read from.</param>
        /// <param name="type">Type of the object to convert. Expected to be string.</param>
        /// <returns>The deserialized string, or the value of the referenced environment variable if applicable.</returns>
        public object ReadYaml(IParser parser, Type type)
        {
            if (parser.Current is Scalar scalar)
            {
                var textValue = scalar.Value;
                return Environment.ExpandEnvironmentVariables(textValue);
            }
            return String.Empty;
        }

        /// <summary>
        /// Writes a string value as a YAML scalar.
        /// </summary>
        /// <param name="emitter">The YAML emitter to write to.</param>
        /// <param name="value">The string value to write.</param>
        /// <param name="type">Type of the object to write. Expected to be string.</param>
        public void WriteYaml(IEmitter emitter, object value, Type type)
        {
            emitter.Emit(new Scalar(null, null, value.ToString(), ScalarStyle.Any, true, false));
        }
    }
}