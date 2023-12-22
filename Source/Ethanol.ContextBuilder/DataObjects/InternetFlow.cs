using System.Net;
using System.Net.Sockets;

namespace Ethanol.ContextBuilder.Context
{
    /// <summary>
    /// Represents the Internet protocol version.
    /// </summary>
    public enum IPVersion
    {
        /// <summary>Internet protocol version 4.</summary>
        IPv4 = 4,

        /// <summary>Internet protocol version 6.</summary>
        IPv6 = 6
    }

    /// <summary>
    /// Provides an abstract base for representing internet data flows.
    /// </summary>
    public abstract class InternetFlow
    {
        /// <summary>
        /// Gets or sets the version of the Internet Protocol (IP) used.
        /// </summary>
        public IPVersion Version { get; set; }

        /// <summary>
        /// Gets or sets the destination IP address for the flow.
        /// </summary>
        public IPAddress? DestinationAddress { get; set; }

        /// <summary>
        /// Gets or sets the destination port number.
        /// </summary>
        public ushort DestinationPort { get; set; }

        /// <summary>
        /// Gets or sets the protocol type of the packet's payload. 
        /// Corresponds to the 'Protocol' field in IPv4 headers and 'NextHeader' field in IPv6 headers.
        /// </summary>
        public ProtocolType Protocol { get; set; }

        /// <summary>
        /// Gets or sets the source IP address for the flow.
        /// </summary>
        public IPAddress? SourceAddress { get; set; }

        /// <summary>
        /// Gets or sets the source port number.
        /// </summary>
        public ushort SourcePort { get; set; }

        /// <summary>
        /// Gets the unique key representing this flow based on protocol, source and destination addresses and ports.
        /// </summary>
        public FlowKey FlowKey => new FlowKey(Protocol, SourceAddress, SourcePort, DestinationAddress, DestinationPort);
    }

}
