using System;

namespace Ethanol.DataObjects
{
    /// <summary>
    /// Represents a detailed Internet Protocol (IP) flow data structure that extends the basic <see cref="InternetFlow"/>.
    /// </summary>
    /// <remarks>
    /// The <see cref="IpFlow"/> class provides essential information about a specific flow in an IP network. 
    /// It captures flow direction, duration, starting time, and specifics about packets and octets sent and received. 
    /// Additionally, it includes metadata about the recognized application protocol used in the flow.
    /// </remarks>
    public class IpFlow : InternetFlow
    {
        /// <summary>
        /// Gets or sets the type of flow, which can be either a request flow, response flow, or bidirectional flow.
        /// </summary>
        public FlowType FlowType { get; set; }

        /// <summary>
        /// Gets or sets the recognized application protocol tag associated with the flow.
        /// </summary>
        public string? ApplicationTag { get; set; }

        /// <summary>
        /// Gets or sets the starting time of the flow.
        /// </summary>
        public DateTime TimeStart { get; set; }

        /// <summary>
        /// Gets or sets the duration of the flow.
        /// </summary>
        public TimeSpan TimeDuration { get; set; }

        /// <summary>
        /// Gets or sets the Packet Delta Count, representing the number of packets sent between two network devices during this flow.
        /// </summary>
        public int SentPackets { get; set; }

        /// <summary>
        /// Gets or sets the Octet Delta Count, representing the number of octets (bytes) sent between two network devices during this flow.
        /// </summary>
        public long SentOctets { get; set; }

        /// <summary>
        /// Gets or sets the Packet Delta Count, representing the number of packets received between two network devices during this flow.
        /// </summary>
        public int RecvPackets { get; set; }

        /// <summary>
        /// Gets or sets the Octet Delta Count, representing the number of octets (bytes) received between two network devices during this flow.
        /// </summary>
        public long RecvOctets { get; set; }
    }
}
