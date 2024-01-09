using System.Net;
using System.Net.Sockets;

namespace Ethanol.DataObjects
{
    /// <summary>
    /// Represents a key used to uniquely identify a flow in the IPFIX dataset.
    /// </summary>
    /// <remarks>
    /// The IPFIX flow key plays a critical role in distinguishing individual flow records from each other.
    /// It incorporates a combination of various fields that together form a unique identifier for each flow.
    /// By using this unique identifier, it becomes feasible to group, correlate, or aggregate flow data.
    /// </remarks>
    public record FlowKey(ProtocolType Protocol, IPAddress? SourceAddress, ushort SourcePort, IPAddress? DestinationAddress, ushort DestinationPort)
    {
        /// <summary>
        /// Provides a string representation of the flow key.
        /// </summary>
        /// <returns>
        /// A string that represents the flow key, detailing its protocol, source, and destination properties.
        /// </returns>
        /// <remarks>
        /// This override allows for a human-readable format of the flow key, which can be helpful for debugging,
        /// logging, or displaying the key in various interfaces or reports.
        /// </remarks>
        public override string ToString() => $"{Protocol}@{SourceAddress}:{SourcePort}-{DestinationAddress}:{DestinationPort}";

        /// <summary>
        /// Generates the reverse version of this flow key.
        /// </summary>
        /// <returns>
        /// A new <see cref="FlowKey"/> where the source and destination addresses and ports are swapped.
        /// </returns>
        /// <remarks>
        /// In many networking contexts, especially with bidirectional flows, it's necessary to quickly identify or 
        /// process the reverse of a given flow. This method provides an efficient way to derive the reverse key from a given flow key.
        /// </remarks>
        public FlowKey GetReverseFlowKey() => new FlowKey(Protocol, DestinationAddress, DestinationPort, SourceAddress, SourcePort);
    }

}
