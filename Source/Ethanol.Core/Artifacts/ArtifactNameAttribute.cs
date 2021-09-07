using System;

namespace Ethanol.Artifacts
{
    public class ArtifactNameAttribute : Attribute
    {
        private readonly string _artifactName;

        public ArtifactNameAttribute(string name)
        {
            this._artifactName = name;
        }

        public string Name => _artifactName;
    }
}