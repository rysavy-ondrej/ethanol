using Ethanol.Providers;
using Microsoft.StreamProcessing;
using System;
using System.IO;
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
                Console.WriteLine("---");
                Console.Write(yamlSerializer.Serialize(evt));
                Console.WriteLine("...");
            }
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
