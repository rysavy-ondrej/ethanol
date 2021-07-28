using CsvHelper;
using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Ethanol.Demo
{
    /// <summary>
    /// Data source is a collection of artifact sources. 
    /// </summary>
    public class DataSource
    {
        private readonly IEnumerable<ArtifactSource> _artifactSources;

        public DataSource(IEnumerable<ArtifactSource> artifactSources)
        {
            if (artifactSources is null)
            {
                throw new ArgumentNullException(nameof(artifactSources));
            }
            _artifactSources = artifactSources;
        }
        public ArtifactSource GetArtifactSource(Type artifactType)
        {
            return _artifactSources.First(x => x.ArtifactType == artifactType);
        }
    }


    public abstract class ArtifactSource
    {
        public abstract Type ArtifactType { get; }

        public abstract IEnumerable<Artifact> Artifacts { get; }

    }

    /// <summary>
    /// Represents a data source based on CSV file that provides artifacts of type <typeparamref name="TArtifactType"/>.
    /// </summary>
    public class CsvArtifactSource<TArtifactType> : ArtifactSource where TArtifactType : Artifact
    {
        readonly string _filename;
        private List<TArtifactType> _artifacts;

        public CsvArtifactSource(string filename)
        {
            _filename = filename ?? throw new ArgumentNullException(nameof(filename));
        }

        public void Load()
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = false
            };
            using (var reader = new StreamReader(_filename))
            using (var csv = new CsvReader(reader, config))
            {
                _artifacts = csv.GetRecords<TArtifactType>().ToList();
            }
        }

        public override Type ArtifactType => typeof(TArtifactType);

        public override IEnumerable<Artifact> Artifacts => _artifacts.Cast<Artifact>();
    }
}