using System;

namespace Ethanol.Demo
{

    public static partial class FactLoaders
    {
        public static class Common
        {
            /// <summary>
            /// Provides DNS flows that may be related to the given flow.
            /// </summary>
            public static FactBuilder ServiceDomain =
                new FactBuilder<Artifact, ArtifactDns>("ServiceDomain", (flow, dns) => flow.EndPoint(dns.DstIp, dns.DnsResponseData) && flow.Before(TimeSpan.FromMinutes(30), dns));
            /// <summary>
            /// Provides flow of the given <typeparamref name="Target"/> type 
            /// that shares the conversation end point addresses and fits in the given window.
            /// </summary>
            /// <typeparam name="Target">The type of flows.</typeparam>
            /// <param name="span">The window size to consider.</param>
            public static FactBuilder ÄdjacentFlow<Target>(TimeSpan span) where Target : Artifact =>
                new FactBuilder<Artifact, Target>("AdjacentFlow", (flow, other) => flow.Id != other.Id && flow.EndPointConv(other) && flow.Window(span, span, other));
        }
    }
}