using System.Net;

namespace Ethanol.ContextBuilder.Builders
{
    /// <summary>
    /// Represents host-related network activity/flows, which is a part of the host context.
    /// </summary>
    public record NetworkActivity
    {
        /// <summary>
        /// A collection of all plain HTTP requests made by the host.
        /// </summary>
        public HttpRequest[] PlainWeb { get; set; }
        /// <summary>
        /// A collection of domains requested to resolve by the host.
        /// </summary>
        public DnsResolution[] Domains { get; set; }
        /// <summary>
        /// A collection of encrypted connection of the host.
        /// </summary>
        public TlsConnection[] Encrypted { get; set; }

        /// <summary>
        /// An collection of IP addresses connected by the host.
        /// </summary>
        public IPAddress[] RemoteHosts { get; set; }
    }
}
