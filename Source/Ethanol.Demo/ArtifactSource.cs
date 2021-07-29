using System;
using System.Collections.Generic;

namespace Ethanol.Demo
{
    public abstract class ArtifactSource
    {
        public abstract Type ArtifactType { get; }

        public abstract IEnumerable<Artifact> Artifacts { get; }

        public abstract void Validate();
    }
}