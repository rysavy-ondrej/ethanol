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

    partial class Program
    {
        /// <summary>
        /// Detects TOR communication from context in the collection of <paramref name="sourceFlowFiles"/>.
        /// </summary>
        /// <param name="sourceFlowFiles">The observable collection of source files.</param>
        /// <param name="configuration">The configuration.</param>
        /// <returns>Task that completes when the processing is done.</returns>
        Task AnalyzeFlowsInFiles(IObservable<FileInfo> sourceFlowFiles, IObservable<FileInfo> sourceDumpFiles,DataFileFormat inputFormat, DataFileFormat outputFormat)
        {
            var ethanol = new EthanolEnvironment();
            var torClassifier = new TorFlowClassifier();

            var entryObserver = new IpfixObservableStream(TimeSpan.FromMinutes(15), TimeSpan.FromMinutes(5));
            var socketObserver = new SocketObservableStream(TimeSpan.FromMinutes(15), TimeSpan.FromMinutes(5));
            entryObserver.EnrichFrom(socketObserver,
                left => left.FlowKey,
                right => right.FlowKey,
                (rec, grouping) => UpdateProcessName(rec, grouping) 
                );
            
            var exitStream = ethanol.ContextBuilder.BuildTlsContext(entryObserver).Select(ContextFlowClassifier<TlsContext>.Classify(torClassifier));
            var exitObservable = new ObservableEgressStream<ClassifiedContextFlow<TlsContext>>(exitStream);
            
            var transformer = new ObservableTransformStream<IpfixRecord, ClassifiedContextFlow<TlsContext>>(entryObserver, exitObservable);
            var consumerTask = exitObservable.ForEachAsync(obj =>
            {
                if (outputFormat == DataFileFormat.Yaml)
                {
                    PrintStreamEventYaml(obj.Payload.Flow.ToString(), obj);
                }
                else
                {
                    PrintStreamEventJson(obj.Payload.Flow.ToString(), obj);
                }
            });

            var flowProducerTask = inputFormat == DataFileFormat.Csv
                ? ethanol.DataLoader.LoadFromCsvFiles(sourceFlowFiles, entryObserver, _cancellationTokenSource.Token)
                : ethanol.DataLoader.LoadFromNfdFiles(sourceFlowFiles, entryObserver, _cancellationTokenSource.Token);

            var dumpProducerTask = sourceDumpFiles != null
                ? ethanol.DataLoader.LoadFromCsvFiles(sourceDumpFiles, socketObserver, _cancellationTokenSource.Token)
                : CompleteEmptyObservableTask(socketObserver);
 
            return Task.WhenAll(consumerTask, flowProducerTask, dumpProducerTask);
        }

        private IpfixRecord UpdateProcessName(IpfixRecord ipfixRecord, IGrouping<FlowKey ,SocketRecord> socketGrouping)
        {
            ipfixRecord.ProcessName = socketGrouping?.FirstOrDefault()?.ProcessName;
            return ipfixRecord;
        }

        private static Task CompleteEmptyObservableTask<T>(IObserver<T> socketObserver)
        {
            socketObserver.OnCompleted();
            return Task.CompletedTask;
        }
    }
}
