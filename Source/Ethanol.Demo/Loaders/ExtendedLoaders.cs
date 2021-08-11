using System;
using System.Linq;

namespace Ethanol.Demo
{
    public static partial class LoaderFunctions
    {
        public static FactLoaderFunction<Target, Target> ReverseFlow<Target>(TimeSpan span) where Target : IpfixArtifact =>
             (Target target, IQueryable<Target> input) =>
                from other in input
                where target.Id != other.Id
                    && target.EndPoint(other.DstIp, other.SrcIp)
                    && target.Window(span, span, other)
                select new Fact(nameof(ReverseFlow), other);
        public static FactLoaderFunction<Target, Target> SiblingFlow<Target>(TimeSpan span) where Target : IpfixArtifact =>
             (Target target, IQueryable<Target> input) =>
                from other in input
                where target.Id != other.Id
                    && FlowRelation.SrcHost.Check(target,other)
                    && target.SrcPt - 10 <= other.SrcPt && other.SrcPt <= target.SrcPt + 10
                    && target.Window(span, span, other)
                select new Fact(nameof(SiblingFlow), other);

        public static FactLoaderFunction<Target, ArtifactHttp> RelatedHttp<Target>(TimeSpan span) where Target : IpfixArtifact =>
            (Target target, IQueryable<ArtifactHttp> input) =>
            from http in input
            where
                    target.EndPoint(http) 
                &&  target.Window(TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1), http)
            select new Fact(nameof(RelatedHttp), http);
    }
}