using CsvHelper;
using Ethanol.Providers;
using Ethanol.Streaming;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Ethanol.Demo
{

    public class IpfixObservableStream : ObservableIngressStream<IpfixRecord>
    {
        public IpfixObservableStream(TimeSpan windowSize, TimeSpan windowHop) : base(x => DateTime.Parse(x.TimeStart).Ticks, windowSize, windowHop)
        {
        }
    }

    public class TcpconObservableStream : ObservableIngressStream<TcpconRecord>
    {
        public TcpconObservableStream(TimeSpan windowSize, TimeSpan windowHop) : base(x => x.CurrentTime.Ticks, windowSize, windowHop)
        {
        }
    }

    /// <summary>
    /// Represents the source of IpfixRecords. Use provided methods for load or fetch the available records.
    /// </summary>
    public static class SourceFileLoader
    {
        /// <summary>
        /// Asynchronously loads <see cref="IpfixRecord"/> objects from nfdump files.
        /// </summary>
        /// <param name="_"></param>
        /// <param name="sourceFiles">The obesrvable collection of source files.</param>
        /// <param name="ingressStream">The stream to push loaded records to.</param>
        /// <param name="intermediateFilesPath">Filepath to folder where to store CSV files for the loaded Nfdump files. Can be null, in which case no CSV files are written.</param>
        /// <param name="cancellationToken">The token to cancel the loading operation.</param>
        /// <returns>The completion task.</returns>
        public static async Task LoadFromNfdFiles(this DataLoaderCatalog _, IObservable<FileInfo> sourceFiles, CsvLoader<IpfixRecord> csvLoader, CancellationToken cancellationToken)
        {
            var nfdump = new NfdumpExecutor();
            await sourceFiles
                .ForEachAsync(f => LoadRecordsFromFile(csvLoader, null, f, true).Wait(), cancellationToken);
        }
        /// <summary>
        /// Detect the custom provided case in the source files. 
        /// </summary>
        /// <param name="sourceFiles">A collection of source files.</param>
        /// <param name="ingressStream">An observer to be populated with loaded records.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to stop the loading of the records.</param>
        /// <returns>Task that completes when all source items have been processed. In case of inifinite input observable it ends only by setting the cancellation token.</returns>
        public static Task LoadFromNfdFiles(this DataLoaderCatalog catalog, IObservable<FileInfo> sourceFiles, ObservableIngressStream<IpfixRecord> ingressStream, CancellationToken cancellationToken)
        {
            var loader = new CsvLoader<IpfixRecord>();
            var loadedFlows = 0;
            loader.OnReadRecord += (object _, IpfixRecord value) =>
            {
                if (++loadedFlows % 1000 == 0) Console.Error.Write('!');
                ingressStream.OnNext(value);

            };
            return catalog.LoadFromNfdFiles(sourceFiles, loader, cancellationToken).ContinueWith(_ => ingressStream.OnCompleted());
        }


        /// <summary>
        /// Detect the custom provided case in the source files. 
        /// </summary>
        /// <param name="sourceFiles">A collection of source files.</param>
        /// <param name="ingressStream">An observer to be populated with loaded records.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to stop the loading of the records.</param>
        /// <returns>Task that completes when all source items have been processed. In case of inifinite input observable it ends only by setting the cancellation token.</returns>
        public static async Task LoadFromCsvFiles<TRecord>(this DataLoaderCatalog _, IObservable<FileInfo> sourceFiles, ObservableIngressStream<TRecord> ingressStream, CancellationToken cancellationToken)
        {
            var loader = new CsvLoader<TRecord>();
            var loadedFlows = 0;
            loader.OnReadRecord += (object _, TRecord value) =>
            {
                if (++loadedFlows % 1000 == 0) Console.Error.Write('!');
                ingressStream.OnNext(value);

            };

            // producer:
            await sourceFiles
                .ForEachAsync(f => LoadRecordsFromFile(loader, null, f, true).Wait(), cancellationToken)
                .ContinueWith(_ => ingressStream.OnCompleted());
        }

        /// <summary>
        /// Writes all records to CSV file.
        /// </summary>
        /// <param name="filename">Target filename of the CSV file to be produced.</param>
        /// <param name="records">A list of records to be written to the output file</param>
        static void WriteAllRecords<T>(string filename, IEnumerable<T> records)
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
        static async Task LoadRecordsFromFile<T>(CsvLoader<T> csvLoader, NfdumpExecutor nfdumpExecutor, FileInfo fileInfo, bool readFromCsvInput)
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
