using System;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace Ethanol.ContextBuilder.Serialization
{
    /// <summary>
    /// Provides custom YAML serialization and deserialization for IP address prefixes.
    /// </summary>
    /// <remarks>
    /// This converter is tailored for handling <see cref="IPAddressPrefix"/> objects in YAML. It ensures that the IP address 
    /// prefixes are correctly serialized to and deserialized from their string representation in YAML format. 
    /// When reading from YAML, the converter attempts to parse the string value into an <see cref="IPAddressPrefix"/>. 
    /// If the string is not a valid IP address prefix, a <see cref="YamlException"/> is thrown.
    /// </remarks>
    public class IPAddressPrefixYamlTypeConverter : IYamlTypeConverter
    {
        /// <summary>
        /// Determines whether this converter can convert the specified type.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <returns>True if the type is <see cref="IPAddressPrefix"/>; otherwise, false.</returns>
        public bool Accepts(Type type)
        {
            return type == typeof(IPAddressPrefix);
        }

        /// <summary>
        /// Reads and converts the YAML representation of an IP address prefix.
        /// </summary>
        /// <param name="parser">The YAML parser to read from.</param>
        /// <param name="type">Type of the object to convert. Expected to be <see cref="IPAddressPrefix"/>.</param>
        /// <returns>The deserialized IP address prefix.</returns>
        /// <exception cref="YamlException">Thrown when the provided YAML value is not a valid IP address prefix.</exception>
        public object ReadYaml(IParser parser, Type type)
        {
            var scalar = parser.Consume<Scalar>();
            if (scalar.Value == null)
            {
                return null;
            }

            IPAddressPrefix ipAddress;
            if (!IPAddressPrefix.TryParse(scalar.Value, out ipAddress))
            {
                throw new YamlException($"Invalid IP address prefix: {scalar.Value}");
            }

            return ipAddress;
        }

        /// <summary>
        /// Writes an IP address prefix value as a YAML scalar.
        /// </summary>
        /// <param name="emitter">The YAML emitter to write to.</param>
        /// <param name="value">The IP address prefix value to write.</param>
        /// <param name="type">Type of the object to write. Expected to be <see cref="IPAddressPrefix"/>.</param>
        public void WriteYaml(IEmitter emitter, object value, Type type)
        {
            var ipAddress = (IPAddressPrefix)value;
            emitter.Emit(new Scalar(null, null, ipAddress.ToString(), ScalarStyle.Any, true, false));
        }
    }
}
