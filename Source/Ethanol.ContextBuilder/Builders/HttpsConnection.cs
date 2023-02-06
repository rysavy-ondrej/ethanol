using Ethanol.ContextBuilder.Context;

namespace Ethanol.ContextBuilder.Builders
{
    /// <summary>
    /// Represents selected information from HTTPS connection.
    /// </summary>
    public record HttpsConnection
    {
        public IpfixKey Flow { get; set; }
        public string DomainName { get; set; }

    }
}
