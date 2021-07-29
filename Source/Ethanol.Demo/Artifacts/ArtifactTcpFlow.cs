using System.Collections.Generic;

namespace Ethanol.Demo
{
    [ArtifactName("Tcp")]
    public class ArtifactTcpFlow : Artifact
    {
        public override string Operation => throw new System.NotImplementedException();

        public override IEnumerable<ArtifactBuilder> Builders => throw new System.NotImplementedException();
    }
}