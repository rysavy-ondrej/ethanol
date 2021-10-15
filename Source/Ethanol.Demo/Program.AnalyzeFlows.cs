using Ethanol.Streaming;
using Microsoft.StreamProcessing;
using System;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Ethanol.Demo
{

    public class EthanolEnvironment
    {
        private readonly DataLoaderCatalog _dataLoaderCatalog;
        private readonly ContextBuilderCatalog _contextBuilderCatalog;

        public EthanolEnvironment()
        {
            _dataLoaderCatalog = new DataLoaderCatalog(this);
            _contextBuilderCatalog = new ContextBuilderCatalog(this);
        }

        public DataLoaderCatalog DataLoader => _dataLoaderCatalog;
        public ContextBuilderCatalog ContextBuilder => _contextBuilderCatalog;
    }

    partial class Program
    {
        /// <summary>
        /// Takes the input flow files and input tcp connection dump files and creates the context 
        /// to be used in the further analysis. 
        /// </summary>
        /// <param name="sourceFlowFiles"></param>
        /// <param name="sourceDumpFiles"></param>
        /// <returns></returns>
        //Task CollectFlowsInFiles(IObservable<FileInfo> sourceFlowFiles, IObservable<FileInfo> sourceDumpFiles)

        /// <summary>
        /// Detects TOR communication from context in the collection of <paramref name="sourceFiles"/>.
        /// </summary>
        /// <param name="sourceFiles">The observable collection of source files.</param>
        /// <param name="configuration">The configuration.</param>
        /// <returns>Task that completes when the processing is done.</returns>
        Task AnalyzeFlowsInFiles(IObservable<FileInfo> sourceFiles, DataFileFormat inputFormat, DataFileFormat outputFormat)
        {
            var ethanol = new EthanolEnvironment();
            var torClassifier = new TorFlowClassifier();

            var entry = new IpfixObservableStream(TimeSpan.FromMinutes(15), TimeSpan.FromMinutes(5));
            var exitStream = ethanol.ContextBuilder.BuildTlsContext(entry).Select(ContextFlowClassifier<TlsContext>.Classify(torClassifier));
            var exit = new ObservableEgressStream<ClassifiedContextFlow<TlsContext>>(exitStream);
            
            var transformer = new ObservableTransformStream<IpfixRecord, ClassifiedContextFlow<TlsContext>>(entry, exit);

            var flows = 0;
            
            var consumerTask = exit.ForEachAsync(obj =>
            {
                if (++flows % 1000 == 0) Console.Error.Write('.');
                if (outputFormat == DataFileFormat.Yaml)
                {
                    PrintStreamEventYaml(obj.Payload.Flow.ToString(), obj);
                }
                else
                {
                    PrintStreamEventJson(obj.Payload.Flow.ToString(), obj);
                }
            });

            var producerTask = inputFormat == DataFileFormat.Csv
                ? ethanol.DataLoader.LoadFromCsvFiles(sourceFiles, entry, _cancellationTokenSource.Token)
                : ethanol.DataLoader.LoadFromNfdFiles(sourceFiles, entry, _cancellationTokenSource.Token);
            return Task.WhenAll(consumerTask, producerTask);
        }

    }
}
