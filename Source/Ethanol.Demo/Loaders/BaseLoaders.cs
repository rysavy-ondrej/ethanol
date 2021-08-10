using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
namespace Ethanol.Demo
{
    public static partial class LoaderFunctions
    {
            public static FactLoaderFunction<Target, Target> AdjacentFlow<Target>(TimeSpan span) where Target : IpfixArtifact =>
                (Target target, IQueryable<Target> input) =>
                from other in input
                where target.Id != other.Id
                    && target.EndPointConv(other)
                    && target.Window(span, span, other)
                select new Fact("AdjacentFlow", other);

            public static FactLoaderFunction<Target, ArtifactDns> ServiceDomain<Target>(TimeSpan span) where Target : IpfixArtifact =>
                (Target target, IQueryable<ArtifactDns> input) =>
                from dns in input 
                where target.SrcIp == dns.DstIp 
                   && target.DstIp == dns.DnsResponseData 
                   && target.Before(span, dns) 
                select new Fact("ServiceDomain", dns);  
    }
}