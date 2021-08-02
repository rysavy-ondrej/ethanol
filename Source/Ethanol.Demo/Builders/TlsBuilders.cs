using System;

namespace Ethanol.Demo
{
    public static partial class FactLoaders
    {
        public static class Tls
        {
            public static ArtifactBuilder Reverse = new ArtifactBuilder<ArtifactTlsFlow, ArtifactTlsFlow>("HasReverseFlow", (tls, other) => tls.EndPoint(other.DstIp, other.SrcIp) && tls.Window(TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1), other));
        }
        public static class Http
        {
            public static ArtifactBuilder Related = new ArtifactBuilder<Artifact, ArtifactHttpFlow>("HasRelatedHttp", (tls, http) => tls.EndPoint(http) && tls.Window(TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1), http));
        }
    }
}