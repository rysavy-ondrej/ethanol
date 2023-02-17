using System;

namespace Ethanol.ContextBuilder.Context
{
    public class IpFlow : InternetFlow
    {
        public string ApplicationTag { get; set; }

        public DateTime TimeStart { get; set; }
        /// <summary>
        /// The duration of the flow..
        /// </summary>
        public TimeSpan TimeDuration { get; set; }

        /// <summary>
        /// Packet Delta Count field is a data field that represents the number of packets that have been sent between two network devices during a particular flow.
        /// </summary>
        public int PacketDeltaCount { get; set; }

        /// <summary>
        /// Octet Delta Count field is a data field that represents the number of octets (bytes) that have been sent between two network devices during a particular flow. 
        /// </summary>
        public int OctetDeltaCount { get; set; }

        internal object Reverse()
        {
            throw new NotImplementedException();
        }
    }

}
