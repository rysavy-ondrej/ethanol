using System.Text.Json;

namespace Ethanol.ContextBuilder.Writers
{
    public static class JsonSerializerOptionsExtensions
    {
        public static void AddIPAddressConverter(this JsonSerializerOptions options)
        {
            options.Converters.Add(new IPAddressConverter());
        }
    }
}
