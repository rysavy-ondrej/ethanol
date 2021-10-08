using CsvHelper;
using Ethanol.Providers;
using Ethanol.Streaming;
using Microsoft.StreamProcessing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;

namespace Ethanol.Demo
{
    public class FlowProcessor
    {
        public record Configuration(bool ReadFromCsvInput, bool WriteIntermediateFiles, string IntermediateFilesPath);

        private readonly Configuration configuration;

        public FlowProcessor(Configuration configuration)
        {
            this.configuration = configuration;
        }

        /// <summary>
        /// Detect the custom provided case in the source files. 
        /// </summary>
        /// <param name="sourceFiles">A collection of source files.</param>
        /// <param name="configuration">The configuration object.</param>
        /// <returns>Task that completes when all source items have been processed.</returns>
        public async Task LoadFromFiles<TContext>(IObservable<FileInfo> sourceFiles, 
            Func<ContextBuilder, IStreamable<Empty, IpfixRecord>, IStreamable<Empty, ContextFlow<TContext>>> contextPrepareFunc, 
            Func<IStreamable<Empty, ContextFlow<TContext>>, IStreamable<Empty, ClassifiedContextFlow<TContext>>> contextClassifierFunc, 
            Func<IStreamable<Empty, ClassifiedContextFlow<TContext>>, CancellationToken, Task> contextConsumeFunc, CancellationToken cancellationToken) 
        { 
            
#if DEBUG
            Config.ForceRowBasedExecution = true;
            var ts = new Stopwatch();
            ts.Start();
#endif
            var ctxBuilder = new ContextBuilder();
            var windowSize = TimeSpan.FromMinutes(15);
            var windowHop = TimeSpan.FromMinutes(5);

            // used only if reading from nfdump source files
            var nfdump = new NfdumpExecutor();
            var loader = new CsvLoader<IpfixRecord>();
            var subject = new Subject<IpfixRecord>();

            var loadedFlows = 0;
            loader.OnReadRecord += (object _, IpfixRecord value) => {
                if (++loadedFlows % 1000 == 0) Console.Error.Write('!');
                subject.OnNext(value); 
            
            };
            if (configuration.WriteIntermediateFiles)
            {
                var records = new List<IpfixRecord>();
                loader.OnStartLoading += (_, filename) => { records.Clear(); };
                loader.OnReadRecord += (_, record) => { records.Add(record); };
                loader.OnFinish += (_, filename) => { WriteAllRecords(Path.Combine(configuration.IntermediateFilesPath, $"{filename}.csv"), records); };
            }
            var flowStream = subject.GetWindowedEventStream(x => DateTime.Parse(x.TimeStart).Ticks, windowSize, windowHop);

            // USER PROVIDED METHODS ------>
            var contextStream = contextPrepareFunc(ctxBuilder, flowStream);
            var classifiedStream = contextClassifierFunc(contextStream);
            var consumer = contextConsumeFunc(classifiedStream, cancellationToken);
            // <----------------------------

            // producer:
            var producer = sourceFiles
                .ForEachAsync(f => LoadRecordsFromFile(loader, nfdump, f, configuration.ReadFromCsvInput).Wait(), cancellationToken)
                .ContinueWith(_ => subject.OnCompleted());
            await Task.WhenAll(producer, consumer);
#if DEBUG
            ts.Stop();
            Console.Error.WriteLine($"Processed {loadedFlows} flows in {ts.Elapsed}.");
#endif
        }

        /// <summary>
        /// Writes all records to CSV file.
        /// </summary>
        /// <param name="filename">Target filename of the CSV file to be produced.</param>
        /// <param name="records">A list of records to be written to the output file</param>
        private void WriteAllRecords<T>(string filename, IEnumerable<T> records)
        {
            using (var writer = new StreamWriter(filename))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(records);
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
