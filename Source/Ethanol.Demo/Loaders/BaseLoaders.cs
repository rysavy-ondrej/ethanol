using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
namespace Ethanol.Demo
{
    public static partial class LoaderFunctions
    {
        /// <summary>
        /// This is loader that implements 7 different relations between flows as defined in
        /// paper "Ding, et al.: Internet traffic classification based on expanding vector of flow".
        /// </summary>
        /// <typeparam name="Target"></typeparam>
        /// <param name="window"></param>
        /// <param name="flowRelation"></param>
        /// <returns></returns>
        public static FactLoaderFunction<Target, Target> RelatedFlow<Target>(TimeSpan window, FlowRelation flowRelation) where Target : IpfixArtifact =>
            (Target target, IQueryable<Target> input) =>
                from other in input
                where target.Id != other.Id
                   && flowRelation.Check(target, other)
                   && target.Window(window, window, other)
                select new Fact(flowRelation.Name, other);


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
            where target.Before(span, dns)
                && target.SrcIp == dns.DstIp
                && target.DstIp == dns.DnsResponseData
            select new Fact("ServiceDomain", dns);
    }
}