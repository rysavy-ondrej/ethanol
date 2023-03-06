using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace Ethanol.ContextBuilder.Context
{
    public enum FlowType
    {
        RequestFlow,
        ResponseFlow, 
        BidirectionFlow,
    }
    /// <summary>
    /// Represents a (possibly) bidirectional internet flow. 
    /// </summary>
    public class IpFlow : InternetFlow
    {
        /// <summary>
        /// The type of flow: request flow, response flow, or bidirectional flow.
        /// </summary>
        public FlowType FlowType { get; set; }
        /// <summary>
        /// Stands for the recognized application protocol. 
        /// </summary>
        public string ApplicationTag { get; set; }

        public DateTime TimeStart { get; set; }
        /// <summary>
        /// The duration of the flow..
        /// </summary>
        public TimeSpan TimeDuration { get; set; }

        /// <summary>
        /// Packet Delta Count field is a data field that represents the number of packets that have been sent between two network devices during a particular flow.
        /// </summary>
        public int SentPackets { get; set; }

        /// <summary>
        /// Octet Delta Count field is a data field that represents the number of octets (bytes) that have been sent between two network devices during a particular flow. 
        /// </summary>
        public int SentOctets { get; set; }
        /// <summary>
        /// Packet Delta Count field is a data field that represents the number of packets that have been sent between two network devices during a particular flow.
        /// </summary>
        public int RecvPackets { get; set; }

        /// <summary>
        /// Octet Delta Count field is a data field that represents the number of octets (bytes) that have been sent between two network devices during a particular flow. 
        /// </summary>
        public int RecvOctets { get; set; }
    }

    public static class IpFlowExtensions
    {
        public static IEnumerable<R> SelectFlows<T,R>(this IEnumerable<IpFlow> flows, Func<T,R> func) where T : IpFlow
        {
            return flows.Where(x => x is T).Cast<T>().Select(func);
        }
        public static IEnumerable<T> SelectFlows<T>(this IEnumerable<IpFlow> flows) where T : IpFlow
        {
            return flows.Where(x => x is T).Cast<T>();
        }
    }

}
