using CsvHelper;
using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;

namespace Ethanol.Providers
{
    /// <summary>
    /// The class can be used to load individual records from CSV files.
    /// </summary>
    /// <typeparam name="TRecord">The type of record to load. Input CSV files should have corresponding columns to the fields of this type.</typeparam>
    public class CsvLoader<TRecord>
    {
        /// <summary>
        /// Number of flows read by this current instance.
        /// </summary>
        public int FlowCount { get; set; }

        /// <summary>
        /// Called before a file is started to read by the current loader.
        /// </summary>
        public event EventHandler<string> OnStartLoading;
        /// <summary>
        /// Called after a CSV file has been read by the current loader.
        /// </summary>
        public event EventHandler<string> OnFinish;
        /// <summary>
        /// Called for each record read by the current loader.
        /// </summary>
        public event EventHandler<TRecord> OnReadRecord;
        /// <summary>
        /// Loads CSV lines from the given stream and for each record calls the registered callbacks.
        /// </summary>
        /// <param name="stream">An input stream to load CSV records from. The first line of this stream must be a CSV header.</param>
        /// <returns>A completion task as this method performs async loading with a callback called for every loaded record.</returns>
        public Task Load(string filename, Stream stream)
        {
            return Task.Run(() =>
            {
                OnStartLoading?.Invoke(this, filename);
                foreach (var record in LoadAll(stream))
                {
                    FlowCount++;
                    OnReadRecord?.Invoke(this, record);
                }
                OnFinish?.Invoke(this, filename);
            });
        }

        /// <summary>
        /// Loads all records from the provided stream.
        /// </summary>
        /// <param name="stream">The source stream to read record from.</param>
        /// <returns>An enumerable of CSV record.</returns>
        public static IEnumerable<TRecord> LoadAll(Stream stream)
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                TrimOptions = TrimOptions.Trim,
                MissingFieldFound = _ => { },
            };
            using (var reader = new StreamReader(stream, leaveOpen: true))
            using (var csv = new CsvReader(reader, config))
            {
                csv.Read();
                csv.ReadHeader();
                while (csv.Read())
                {
                    var record = csv.GetRecord<TRecord>();
                    yield return record;
                }
            }
        }

        /// <summary>
        /// Reads all records from the given CSV file.
        /// </summary>
        /// <param name="path">A path to CSV file to read CSV records from.</param>
        /// <returns>An enumerable collection of CSV records of type <typeparamref name="TRecord"/>.</returns>
        public static IEnumerable<TRecord> LoadAll(string path)
        {
            return LoadAll(File.OpenRead(path));
        }
    }
}