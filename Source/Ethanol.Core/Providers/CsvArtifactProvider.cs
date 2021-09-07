using CsvHelper;
using CsvHelper.Configuration;
using Ethanol.Artifacts;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reactive.Linq;

namespace Ethanol.Providers
{

    public static class CsvArtifactProvider
    {
        public static IArtifactProvider CreateArtifactSource(Type artifactType, string filename)
        {
            var genericType = typeof(CsvArtifactProvider<>);
            Type constructed = genericType.MakeGenericType(new[] { artifactType });
            object o = Activator.CreateInstance(type: constructed, args: filename);
            return o as IArtifactProvider;
        }
    }
    /// <summary>
    /// Represents a data source based on CSV file that provides artifacts of type <typeparamref name="TArtifact"/>.
    /// </summary>
    public class CsvArtifactProvider<TArtifact> : IArtifactProvider<TArtifact> where TArtifact : Artifact
    {
        readonly string _filename;
        private List<TArtifact> _artifacts;

        public CsvArtifactProvider(string filename)
        {
            _filename = filename ?? throw new ArgumentNullException(nameof(filename));
        }

        void Load()
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
                    _artifacts = csv.GetRecords<TArtifact>().ToList();                    
                }
            }
            for(int i = 0; i < _artifacts.Count; i++)
            {
                _artifacts[i].Id = $"{i+1}";
            }
        }


        public IObservable<TTarget> GetObservable<TTarget>() where TTarget : Artifact
        {
            return GetOrLoadArtifacts().Cast<TTarget>().ToObservable();
        }


        IList<TArtifact> GetOrLoadArtifacts()
        {
            if (_artifacts == null) Load();
            return _artifacts; 
        }

        public IObservable<TArtifact> GetObservable()
        {
            return GetOrLoadArtifacts().ToObservable();
        }

        public Type ArtifactType => typeof(TArtifact);

        public string Source => $"file://{Path.GetFullPath(this._filename)}";
    }
}