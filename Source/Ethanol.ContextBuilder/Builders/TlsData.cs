using Ethanol.ContextBuilder.Context;

namespace Ethanol.ContextBuilder.Builders
{
    /// <summary>
    ///  Represents selected information from TLS connection.
    /// </summary>
    public record TlsData
    {
        public IpfixKey Flow { get; set; }
        public string RequestHost { get; set; }
        public string TlsVersion { get; set; }
        public string JA3 { get; set; }
        public string SNI { get; set; }
        public string CommonName { get; set; }
    }
}
