using System;

namespace Ethanol.Demo
{
    public static partial class FactLoaders
    {
        

        public static class Tls
        {
            public static FactBuilder ReverseFlow = new FactBuilder<ArtifactTls, ArtifactTls>("ReverseFlow", (tls, other) => tls.EndPoint(other.DstIp, other.SrcIp) && tls.Window(TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1), other));
            public static FactBuilder SiblingFlow = new FactBuilder<ArtifactTls, ArtifactTls>("SiblingFlow", (tls, other) => tls.Id != other.Id
             && tls.SameSource(other) 
             && tls.SrcPt - 10 <= other.SrcPt && other.SrcPt <= tls.SrcPt + 10  
             && tls.Window(TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1), other));
        }
        public static class Http
        {
            public static FactBuilder Related = new FactBuilder<Artifact, ArtifactHttp>("RelatedHttp", (tls, http) => tls.EndPoint(http) && tls.Window(TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1), http));
        }
    }
}