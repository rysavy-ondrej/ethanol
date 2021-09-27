using Ethanol.Providers;
using Microsoft.StreamProcessing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace Ethanol.Demo
{
    partial class Program
    {
        /// <summary>
        /// Produces an output for the single stream output.
        /// </summary>
        /// <typeparam name="T">The type of context.</typeparam>
        /// <param name="obj">Flow and context.</param>
        private void PrintStreamEvent<T>(string eventType, StreamEvent<T> obj)
        {
            if (obj.IsInterval || obj.IsEnd)
            {
                var evt = new { Event = eventType, ValidTime = new { Start = new DateTime(obj.StartTime), End = new DateTime(obj.EndTime) }, Payload = obj.Payload };
                Console.WriteLine(yamlSerializer.Serialize(evt));
            }
        }
        /// <summary>
        /// Computes an entropy of the given string.
        /// </summary>
        /// <param name="message">A string to compute entropy for.</param>
        /// <returns>A flow value representing Shannon's entropy for the given string.</returns>
        private double ComputeEntropy(string message)
        {
            if (message == null) return 0;
            Dictionary<char, int> K = message.GroupBy(c => c).ToDictionary(g => g.Key, g => g.Count());
            double entropyValue = 0;
            foreach (var character in K)
            {
                double PR = character.Value / (double)message.Length;
                entropyValue -= PR * Math.Log(PR, 2);
            }
            return entropyValue;
        }
        /// <summary>
        /// Computes entropy for individual parts of the domain name.
        /// </summary>
        /// <param name="domain">The domain name.</param>
        /// <returns>An array of entropy values for each domain name.</returns>
        private double[] ComputeDnsEntropy(string domain)
        {
            if (string.IsNullOrWhiteSpace(domain)) return new double[] { 0.0 };
            var parts = domain.Split('.');
            return parts.Select(ComputeEntropy).ToArray();
        }
        /// <summary>
        /// Loads records from either nfdump or csv file.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="csvLoader">The CSV loader.</param>
        /// <param name="nfdumpExecutor">Nfdump wrapper.</param>
        /// <param name="fileInfo">File info to load.</param>
        /// <param name="readFromCsvInput">true for loading from CSV.</param>
        /// <returns>Task that signalizes the completion of loading operation.</returns>
        public static async Task LoadRecordsFromFile<T>(CsvLoader<T> csvLoader, NfdumpExecutor nfdumpExecutor, FileInfo fileInfo, bool readFromCsvInput)
        {
            csvLoader.FlowCount = 0;
            if (readFromCsvInput)
            {
                await csvLoader.Load(fileInfo.Name, fileInfo.OpenRead());
            }
            else
            {
                await nfdumpExecutor.ProcessInputAsync(fileInfo.FullName, "ipv4", async reader => await csvLoader.Load(fileInfo.Name, reader));
            }
        }
    }
}
