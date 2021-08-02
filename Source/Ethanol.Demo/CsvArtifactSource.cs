using CsvHelper;
using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Ethanol.Demo
{

    public static class CsvArtifactSource
    {
        public static ArtifactSource CreateArtifactSource(Type artifactType, string filename)
        {
            var genericType = typeof(CsvArtifactSource<>);
            Type constructed = genericType.MakeGenericType(new[] { artifactType });
            object o = Activator.CreateInstance(type: constructed, args: filename);
            return o as ArtifactSource;
        }
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
                HasHeaderRecord = false,
                TrimOptions = TrimOptions.Trim,
            };
            using (var reader = new StreamReader(_filename))
            {
                // skip the first row as it has not valid CSV format!
                reader.ReadLine();
                using (var csv = new CsvReader(reader, config))
                {
                    _artifacts = csv.GetRecords<TArtifactType>().ToList();                    
                }
            }
            for(int i = 0; i < _artifacts.Count; i++)
            {
                _artifacts[i].Id = $"{i+1}";
            }
        }

        public override void Validate()
        {
            Load();
        }

        public override Type ArtifactType => typeof(TArtifactType);

        public override IEnumerable<Artifact> Artifacts
        {
            get 
            {
                if (_artifacts == null) Load();
                return _artifacts.Cast<Artifact>(); 
            }
        }
    }
}