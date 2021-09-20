using CsvHelper;
using CsvHelper.Configuration;
using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;

namespace Ethanol.Providers
{
    /// <summary>
    /// The class can be used to load individual records from CSV files.
    /// </summary>
    /// <typeparam name="TRecord">The type of record to load. Input CSV files should have corresponding columns to the fields of this type.</typeparam>
    public class ArtifactsLoader<TRecord>
    {
        public int FlowCount { get; set; }

        public event EventHandler<TRecord> OnReadRecord;
        /// <summary>
        /// Loads CSV lines from the given stream and for each record calls the registered callbacks.
        /// </summary>
        /// <param name="stream">An input stream to load CSV records from. The first line of this stream must be a CSV header.</param>
        /// <returns>A completion task as this method performs async loading with a callback called for every loaded record.</returns>
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
                        var record = csv.GetRecord<TRecord>();
                        FlowCount++;
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