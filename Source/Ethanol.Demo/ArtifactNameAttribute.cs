using System;

namespace Ethanol.Demo
{
    internal class ArtifactNameAttribute : Attribute
    {
        private readonly string _artifactName;

        public ArtifactNameAttribute(string name)
        {
            this._artifactName = name;
        }

        public string Name => _artifactName;
    }
}