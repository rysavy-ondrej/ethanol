using System.Net;
using System.Net.Sockets;

namespace Ethanol.ContextBuilder.Context
{
    public enum IPVersion
    {
        /// <summary>Internet protocol version 4.</summary>
        IPv4 = 4,

        /// <summary>Internet protocol version 6.</summary>
        IPv6 = 6
    }

    /// <summary>
    /// An abstract representation of internet flow.
    /// </summary>
    public abstract class InternetFlow
    {
        /// <summary>
        /// The IP version
        /// </summary>
        public IPVersion Version { get; set; }

        /// <summary>
        /// The destination address
        /// </summary>
        public IPAddress DestinationAddress { get; set; }

        public ushort DestinationPort { get; set; }

        /// <summary>
        /// The protocol of the ip packet's payload
        /// Named 'Protocol' in IPv4
        /// Named 'NextHeader' in IPv6'
        /// </summary>
        public ProtocolType Protocol { get; set; }

        /// <summary>
        /// The source address
        /// </summary>
        public IPAddress SourceAddress { get; set; }


        public ushort SourcePort { get; set; }

        public FlowKey FlowKey => new FlowKey(Protocol, SourceAddress, SourcePort, DestinationAddress, DestinationPort);
    }
}
