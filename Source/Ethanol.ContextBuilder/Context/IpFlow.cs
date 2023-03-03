using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

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
