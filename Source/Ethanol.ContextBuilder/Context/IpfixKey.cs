using Ethanol.ContextBuilder.Math;
using System;

namespace Ethanol.ContextBuilder.Context
{
    /// <summary>
    /// Represents a flow key.
    /// </summary>
    public record IpfixKey
    {
        /// <summary>
        /// Flow protocol.
        /// </summary>
        public string Proto { get; set; }
        /// <summary>
        /// Source IP address.
        /// </summary>
        public string SrcIp { get; set; }
        /// <summary>
        /// Source L4 port.
        /// </summary>
        public int SrcPt { get; set; }
        /// <summary>
        /// Destination IP address.
        /// </summary>
        public string DstIp { get; set; }
        /// <summary>
        /// Destionation L4 port.
        /// </summary>
        public int DstPt { get; set; }

        /// <summary>
        /// A string representation of the flow key.
        /// </summary>
        /// <returns></returns>
        public override string ToString() => $"{Proto}@{SrcIp}:{SrcPt}-{DstIp}:{DstPt}";
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
