using System;

namespace Ethanol.Demo
{
    public static partial class FactLoaders
    {
        public static class Tls
        {
            public static FactBuilder Reverse = new FactBuilder<ArtifactTls, ArtifactTls>("HasReverseFlow", (tls, other) => tls.EndPoint(other.DstIp, other.SrcIp) && tls.Window(TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1), other));
        }
        public static class Http
        {
            public static FactBuilder Related = new FactBuilder<Artifact, ArtifactHttp>("HasRelatedHttp", (tls, http) => tls.EndPoint(http) && tls.Window(TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1), http));
        }
    }
}