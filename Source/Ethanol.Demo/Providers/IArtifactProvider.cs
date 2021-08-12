using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ethanol.Demo
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
        IQueryable<TArtifact> Queryable { get; }
    }

    public interface IArtifactProvider
    {
        string Source { get; }
        Type ArtifactType { get; }
        IQueryable<TArtifact> GetQueryable<TArtifact>() where TArtifact : Artifact;
    }
}
