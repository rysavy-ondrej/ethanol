using System;
using System.Net;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace Ethanol.ContextBuilder.Serialization
{
    /// <summary>
    /// Provides custom YAML serialization and deserialization for IP addresses.
    /// </summary>
    /// <remarks>
    /// This converter is tailored for handling <see cref="IPAddress"/> objects in YAML. It ensures that the IP addresses are 
    /// correctly serialized to and deserialized from their string representation in YAML format. 
    /// When reading from YAML, the converter attempts to parse the string value into an <see cref="IPAddress"/>. If the string 
    /// is not a valid IP address, a <see cref="YamlException"/> is thrown.
    /// </remarks>
    public class IPAddressYamlTypeConverter : IYamlTypeConverter
    {
        /// <summary>
        /// Determines whether this converter can convert the specified type.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <returns>True if the type is <see cref="IPAddress"/>; otherwise, false.</returns>
        public bool Accepts(Type type)
        {
            return type == typeof(IPAddress);
        }

        /// <summary>
        /// Reads and converts the YAML representation of an IP address.
        /// </summary>
        /// <param name="parser">The YAML parser to read from.</param>
        /// <param name="type">Type of the object to convert. Expected to be <see cref="IPAddress"/>.</param>
        /// <returns>The deserialized IP address.</returns>
        /// <exception cref="YamlException">Thrown when the provided YAML value is not a valid IP address.</exception>
        public object? ReadYaml(IParser parser, Type type)
        {
            var scalar = parser.Consume<Scalar>();
            if (scalar.Value == null)
            {
                return null;
            }

            IPAddress? ipAddress;
            if (!IPAddress.TryParse(scalar.Value, out ipAddress))
            {
                throw new YamlException($"Invalid IP address: {scalar.Value}");
            }

            return ipAddress;
        }

        /// <summary>
        /// Writes an IP address value as a YAML scalar.
        /// </summary>
        /// <param name="emitter">The YAML emitter to write to.</param>
        /// <param name="value">The IP address value to write.</param>
        /// <param name="type">Type of the object to write. Expected to be <see cref="IPAddress"/>.</param>
        public void WriteYaml(IEmitter emitter, object? value, Type type)
        {
            var ipAddressString = value?.ToString() ?? string.Empty;
            emitter.Emit(new Scalar(null, null, ipAddressString, ScalarStyle.Any, true, false));
        }
    }
}
