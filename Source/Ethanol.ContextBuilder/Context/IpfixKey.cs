using System;
using System.Net;
using System.Net.Sockets;

namespace Ethanol.ContextBuilder.Context
{
    /// <summary>
    /// The IPFIX key is a set of one or more fields that are used to uniquely identify a flow.
    /// <para/>
    /// This includes fields such as the source IP address, destination IP address, source port, destination port, and protocol. 
    /// </summary>
    public record FlowKey(ProtocolType Protocol, IPAddress SourceAddress, ushort SourcePort, IPAddress DestinationAddress, ushort DestinationPort)
    {
        /// <summary>
        /// A string representation of the flow key.
        /// </summary>
        /// <returns></returns>
        public override string ToString() => $"{Protocol}@{SourceAddress}:{SourcePort}-{DestinationAddress}:{DestinationPort}";

        public FlowKey GetReverseFlowKey() => new FlowKey(Protocol, DestinationAddress, DestinationPort, SourceAddress, SourcePort);

    }
}
