namespace Ethanol.ContextBuilder.Builders
{
    /// <summary>
    /// Represents host-related network activity/flows, which is a part of the host context.
    /// </summary>
    public record NetworkActivity
    {
        public HttpRequest[] Http { get; set; }
        public HttpsConnection[] Https { get; set; }
        public DnsResolution[] Dns { get; set; }
        public TlsData[] Tls { get; set; }
    }
}
