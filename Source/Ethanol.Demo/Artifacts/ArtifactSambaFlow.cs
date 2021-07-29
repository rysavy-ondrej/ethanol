using System.Collections.Generic;

namespace Ethanol.Demo
{
    [ArtifactName("Samba")]
    public class ArtifactSambaFlow : Artifact
    {
        public override string Operation => throw new System.NotImplementedException();

        public override IEnumerable<ArtifactBuilder> Builders => throw new System.NotImplementedException();
    }
}