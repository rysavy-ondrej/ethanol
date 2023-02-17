using System;
using System.Net;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

public class IPAddressTypeConverter : IYamlTypeConverter
{
    public bool Accepts(Type type)
    {
        return type == typeof(IPAddress);
    }

    public object ReadYaml(IParser parser, Type type)
    {
        var scalar = parser.Consume<Scalar>();
        if (scalar.Value == null)
        {
            return null;
        }

        IPAddress ipAddress;
        if (!IPAddress.TryParse(scalar.Value, out ipAddress))
        {
            throw new YamlException($"Invalid IP address: {scalar.Value}");
        }

        return ipAddress;
    }

    public void WriteYaml(IEmitter emitter, object value, Type type)
    {
        var ipAddress = (IPAddress)value;
        emitter.Emit(new Scalar(null, null, ipAddress.ToString(), ScalarStyle.Any, true, false));
    }
}
