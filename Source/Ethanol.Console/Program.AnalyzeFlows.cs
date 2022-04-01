using Ethanol.Streaming;
using Microsoft.StreamProcessing;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;

namespace Ethanol.Console
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
            var sw = new Stopwatch();
            sw.Start();
            var ethanol = new EthanolEnvironment();
            var torClassifier = new TorFlowClassifier();
            var totalOutputFlows = 0;
            var entryObserver = new IpfixObservableStream(TimeSpan.FromMinutes(15), TimeSpan.FromMinutes(5));
            var socketObserver = new SocketObservableStream(TimeSpan.FromMinutes(15), TimeSpan.FromMinutes(5));
            /*
            var ipfixStream = entryObserver.Join(socketObserver,
                left => left.FlowKey,
                right => right.FlowKey,
                (left, right) => UpdateWith(left, right.ProcessName)); 
            */
            var ipfixStream = entryObserver.EnrichFrom(socketObserver,
                left => left.FlowKey,
                right => right.FlowKey,
                (left, right) => UpdateWith(left, right));

            var exitStream = ethanol.ContextBuilder.BuildTlsContext(ipfixStream);//.Select(ContextFlowClassifier<TlsContext>.Classify(torClassifier));
            var exitObservable = new ObservableEgressStream<ContextFlow<TlsContext>>(exitStream);
            var consumerTask = exitObservable.ForEachAsync(obj =>
            {
                totalOutputFlows++;
                if (outputFormat == DataFileFormat.Yaml)
                {
                    PrintStreamEventYaml(obj.Payload.FlowKey.ToString(), obj);
                }
                else
                {
                    PrintStreamEventJson(obj.Payload.FlowKey.ToString(), obj);
                }
            });

            var flowProducerTask = inputFormat == DataFileFormat.Csv
                ? ethanol.DataLoader.LoadFromCsvFiles(sourceFlowFiles, entryObserver, _cancellationTokenSource.Token).ContinueWith(_=> entryObserver.OnCompleted())
                : ethanol.DataLoader.LoadFromNfdFiles(sourceFlowFiles, entryObserver, _cancellationTokenSource.Token);

            var proxyObserver = new Subject<SocketRecord>();
            proxyObserver.Do(DumpsItems).Subscribe(socketObserver);

            var dumpProducerTask = sourceDumpFiles != null
                ? ethanol.DataLoader.LoadFromCsvFiles(sourceDumpFiles, proxyObserver, _cancellationTokenSource.Token).ContinueWith(_ => proxyObserver.OnCompleted())
                : CompleteEmptyObservableTask(socketObserver);
 
            return Task.WhenAll(consumerTask, flowProducerTask, dumpProducerTask).ContinueWith(t => System.Console.Error.WriteLine($"Flows:{totalOutputFlows} [{sw.Elapsed}]"));         
        }

        private void DumpsItems<T>(T obj)
        {
            File.AppendAllText($"{typeof(T)}.dump", yamlSerializer.Serialize(obj));
        }

        private IpfixRecord UpdateWith(IpfixRecord left, IGrouping<FlowKey, SocketRecord> sockRecords)
        {
            if (sockRecords == null) return left;
            var newIpfix = (IpfixRecord)left.Clone();
            newIpfix.ProcessName = sockRecords.FirstOrDefault()?.ProcessName;
            return newIpfix;
        }
        private static Task CompleteEmptyObservableTask<T>(IObserver<T> observer)
        {
            observer.OnCompleted();
            return Task.CompletedTask;
        }
    }
}
