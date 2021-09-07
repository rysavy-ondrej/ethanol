using Ethanol.Artifacts;
using System;
using System.Linq;

namespace Ethanol.Providers
{
    /// <summary>
    /// This interface provides a collection of artifacts of the given type.
    /// </summary>
    /// <typeparam name="TArtifact"></typeparam>
    public interface IArtifactProvider<TArtifact> : IArtifactProvider where TArtifact : Artifact
    {
        /// <summary>
        /// Provides a collection of artifacts. It may be possible to build LINQ query on this collection.
        /// </summary>
        IObservable<TArtifact> GetObservable();
    }

    public interface IArtifactProvider
    {
        string Source { get; }
        Type ArtifactType { get; }
        IObservable<TArtifact> GetObservable<TArtifact>() where TArtifact : Artifact;
    }
}
