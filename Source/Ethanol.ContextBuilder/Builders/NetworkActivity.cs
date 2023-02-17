using Ethanol.ContextBuilder.Context;
using System;
using System.Net;

namespace Ethanol.ContextBuilder.Builders
{

    public class RemoteHostConnections
    {
        public string Hostname { get; set; }
        public IPAddress HostAddress { get; set; }
        public int Flows { get; set; }
        public ushort[] Ports { get; set; }
        public TimeSpan AverageConnectionTime { get; set; }
        public TimeSpan MaxConnectionTime { get; set; }
        public TimeSpan MinConnectionTime { get; set; }
        public int SendBytes { get; set; }
        public int SendPackets { get; set; }
        public int RecvBytes { get; set; }
        public int RecvPackets { get; set; }
    }

    /// <summary>
    /// Represents host-related network activity/flows, which is a part of the host context.
    /// </summary>
    public class NetworkActivity
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
        public RemoteHostConnections[] RemoteHosts { get; set; }
    }
}
