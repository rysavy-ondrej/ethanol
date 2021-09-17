using CsvHelper;
using CsvHelper.Configuration;
using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;

namespace Ethanol.Providers
{
    public class ArtifactsLoader<TRawRecord>
    {
        public event EventHandler<TRawRecord> OnReadRecord;
        /// <summary>
        /// Loads CSV lines from the given stream and for each record calls the registered callbacks.
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public Task LoadFromCsvAsync(Stream stream)
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                TrimOptions = TrimOptions.Trim,
                MissingFieldFound = OnMissingField,
            };
            return Task.Run(() =>
            {
                using (var reader = new StreamReader(stream))
                using (var csv = new CsvReader(reader, config))
                {
                    csv.Read();
                    csv.ReadHeader();
                    while (csv.Read())
                    {
                        var record = csv.GetRecord<TRawRecord>();
                        OnReadRecord?.Invoke(this, record); 
                    }
                }
            });
        }

        private void OnMissingField(MissingFieldFoundArgs args)
        {
        }
    }
}