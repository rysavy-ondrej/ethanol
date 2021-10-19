using Ethanol.Catalogs;
using Ethanol.Providers;
using Ethanol.Streaming;
using System;
using System.IO;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Ethanol.Console
{

    public class IpfixObservableStream : ObservableIngressStream<IpfixRecord>
    {
        static long GetTimestamp(IpfixRecord ipfixRecord)
        {
            if(DateTime.TryParse(ipfixRecord?.TimeStart, out var dateTime))
            {
                return dateTime.Ticks;
            }
            else
            {
                return DateTime.MinValue.Ticks;
            }
        }
        public IpfixObservableStream(TimeSpan windowSize, TimeSpan windowHop) : base(GetTimestamp, windowSize, windowHop)
        {
        }
    }

    public class SocketObservableStream : ObservableIngressStream<SocketRecord>
    {
        static long GetTimestamp(SocketRecord socketRecord)
        {
            var dateTime = socketRecord?.CurrentTime ?? DateTime.MinValue;
            return dateTime.Ticks;
        }
        public SocketObservableStream(TimeSpan windowSize, TimeSpan windowHop) : base(GetTimestamp, windowSize, windowHop)
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
            await sourceFiles.ForEachAsync(f => LoadRecordsFromFile(csvLoader, nfdump, f).Wait(), cancellationToken);
        }
        /// <summary>
        /// Detect the custom provided case in the source files. 
        /// </summary>
        /// <param name="sourceFiles">A collection of source files.</param>
        /// <param name="ingressStream">An observer to be populated with loaded records.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to stop the loading of the records.</param>
        /// <returns>Task that completes when all source items have been processed. In case of inifinite input observable it ends only by setting the cancellation token.</returns>
        public static Task LoadFromNfdFiles(this DataLoaderCatalog catalog, IObservable<FileInfo> sourceFiles, IObserver<IpfixRecord> ingressStream, CancellationToken cancellationToken)
        {
            var loader = new CsvLoader<IpfixRecord>();
            loader.OnReadRecord += (object _, IpfixRecord value) => ingressStream.OnNext(value);
            return catalog.LoadFromNfdFiles(sourceFiles, loader, cancellationToken);
        }


        /// <summary>
        /// Detect the custom provided case in the source files. 
        /// </summary>
        /// <param name="sourceFiles">A collection of source files.</param>
        /// <param name="ingressStream">An observer to be populated with loaded records.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to stop the loading of the records.</param>
        /// <returns>Task that completes when all source items have been processed. In case of inifinite input observable it ends only by setting the cancellation token.</returns>
        public static async Task LoadFromCsvFiles<TRecord>(this DataLoaderCatalog _, IObservable<FileInfo> sourceFiles, IObserver<TRecord> ingressStream, CancellationToken cancellationToken)
        {
            var loader = new CsvLoader<TRecord>();
            loader.OnReadRecord += (object _, TRecord value) => ingressStream.OnNext(value);
            await sourceFiles
                .ForEachAsync(f => LoadRecordsFromFile(loader, null, f).Wait(), cancellationToken);
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
        static async Task LoadRecordsFromFile<T>(CsvLoader<T> csvLoader, NfdumpExecutor nfdumpExecutor, FileInfo fileInfo)
        {
            if (nfdumpExecutor==null)
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
