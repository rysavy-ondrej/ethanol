using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
namespace Ethanol.Demo
{

    public static partial class FactLoaders
    {
        public static class Common
        {

            public static FactBuilder DomainName =
                new FactBuilder<Artifact, ArtifactDns>("HasDomain", (tls, dns) => tls.EndPoint(dns.DstIp, dns.DnsResponseData) && tls.Before(TimeSpan.FromMinutes(30), dns));
            public static FactBuilder Adjacent<Target>(TimeSpan span) where Target : Artifact =>
                new FactBuilder<Artifact, Target>("IsNearTo", (tls, other) => tls.Id != other.Id && tls.EndPointConv(other) && tls.Window(span, span, other));


            public static IEnumerable<(string, ArtifactDns)> DomainNameLinq(ArtifactTls target, IEnumerable<ArtifactDns> dnsFlows) => 
                from dns in dnsFlows 
                where   target.SrcIp == dns.DstIp 
                     && target.DstIp == dns.DnsResponseData 
                     && target.Before(TimeSpan.FromMinutes(5), dns) 
                select ("HasDomain", dns);  
       }
    }
}