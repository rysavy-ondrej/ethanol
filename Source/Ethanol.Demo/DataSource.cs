using System;
using System.Collections.Generic;
using System.Linq;

namespace Ethanol.Demo
{
    /// <summary>
    /// Data source is a collection of artifact sources. 
    /// </summary>
    public class DataSource
    {
        private readonly List<ArtifactSource> _artifactSources;

        public DataSource(IEnumerable<ArtifactSource> artifactSources)
        {
            if (artifactSources is null)
            {
                throw new ArgumentNullException(nameof(artifactSources));
            }
            _artifactSources = artifactSources.ToList();
        }
        public ArtifactSource GetArtifactSource(Type artifactType)
        {
            return _artifactSources.First(x => x.ArtifactType == artifactType);
        }
    }
}