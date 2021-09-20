using System;
using System.IO;
using System.Threading.Tasks;

namespace Ethanol.Providers
{
    /// <summary>
    /// Loads different artifacts from a single source file and push them to provided observables.
    /// It test filter provided by each artifact type on every record. 
    /// If the record passes the filter it is loaded using the specified mapper.
    /// </summary>
    public class ArtifactsMultiloader<TRawRecord>
    {
        private readonly CsvLoader<TRawRecord> _loader;
        private readonly IArtifactObservableProvider<TRawRecord>[] observables;

        public ArtifactsMultiloader(params IArtifactObservableProvider<TRawRecord>[] observables)
        {
            _loader = new CsvLoader<TRawRecord>();
            _loader.OnReadRecord += _loader_OnReadRecord;
            this.observables = observables;
        }

        private void _loader_OnReadRecord(object sender, TRawRecord record)
        {
            for(int i= 0; i < observables.Length; i++)
            {
                if (observables[i].Match(record))
                {
                    observables[i].Push(record);
                }
            }
        }

        public Task LoadFromCsvAsync(Stream stream)
        {
            return _loader.Load(stream);
        }
    }
}