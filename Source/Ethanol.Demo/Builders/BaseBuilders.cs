using System;

namespace Ethanol.Demo
{

    public static partial class FactLoaders
    {
        public static class Common
        {
            public static FactBuilder DomainName =
                new FactBuilder<Artifact, ArtifactDns>("HasDomain", (tls, dns) => tls.EndPoint(dns.DstIp, dns.DnsResponseData) && tls.Before(TimeSpan.FromMinutes(30), dns));
            public static FactBuilder Surrounding<Target>(TimeSpan span) where Target : Artifact =>
                new FactBuilder<Artifact, Target>("IsNearTo", (tls, other) => tls.Id != other.Id && tls.EndPointConv(other) && tls.Window(span, span, other));
        }
    }
}