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
    public record FlowKey(ProtocolType Pt, IPAddress SrcIp, ushort SrcPt, IPAddress DstIp, ushort DstPt)
    {
        /// <summary>
        /// A string representation of the flow key.
        /// </summary>
        /// <returns></returns>
        public override string ToString() => $"{Pt}@{SrcIp}:{SrcPt}-{DstIp}:{DstPt}";
        /// <summary>
        /// Parses the input string to flow key.
        /// </summary>
        /// <param name="input">The input string.</param>
        /// <returns>true if parsed; false on parsing error.</returns>
        /// <exception cref="NotImplementedException"></exception>
        public static bool TryParse(string input)
        {
            throw new NotImplementedException();
        }
    }
}
