using CsvHelper;
using Ethanol.Providers;
using Ethanol.Streaming;
using Microsoft.StreamProcessing;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;

namespace Ethanol.Demo
{
    partial class Program
    {

        public record DetectConfiguration(OutputFormat OutputFormat, bool ReadFromCsvInput, bool WriteIntermediateFiles, string IntermediateFilesPath);
        record DetectTorConfiguration(double DomainNameEntropy);

        /// <summary>
        /// Detects TOR communication from context in the collection of <paramref name="sourceFiles"/>.
        /// </summary>
        /// <param name="sourceFiles">The observable collection of source files.</param>
        /// <param name="configuration">The configuration.</param>
        /// <returns>Task that completes when the processing is done.</returns>
        Task DetectTor(IObservable<FileInfo> sourceFiles, DetectConfiguration configuration, double domainNameEntropy)
        {
            IStreamable<Empty, ContextFlow<TlsContext>> PrepareTlsContext(ContextBuilder ctxBuilder, IStreamable<Empty, RawIpfixRecord> flowStream) => ctxBuilder.BuildTlsContext(flowStream);

            IStreamable<Empty, ClassifiedContextFlow<TlsContext>> ClassifyTor(IStreamable<Empty, ContextFlow<TlsContext>> contextStream) => contextStream.Select(f => ClassifyTorFlow(f));
            var flows = 0;
            void PrintEvent(StreamEvent<ClassifiedContextFlow<TlsContext>> obj)
            {
                if (++flows % 1000 == 0) Console.Error.Write('.');
                if (obj.IsData)
                {
                    PrintStreamEvent($"{obj.Payload.Flow.Proto}@{obj.Payload.Flow.SrcIp}:{obj.Payload.Flow.SrcPt}-{obj.Payload.Flow.DstIp}:{obj.Payload.Flow.DstPt} {String.Join(',', obj.Payload.Tags)}", obj);
                }
            }
            Task ConsumeTorEvents(IStreamable<Empty, ClassifiedContextFlow<TlsContext>> torFlowsStream, CancellationToken cancellationToken) => torFlowsStream.ToStreamEventObservable().ForEachAsync(PrintEvent, cancellationToken);

            return Detect(sourceFiles, configuration, PrepareTlsContext, ClassifyTor, ConsumeTorEvents);
        }

        

        static ClassifiedContextFlow<TlsContext> ClassifyTorFlow(ContextFlow<TlsContext> flow)
        {
            var tags = new List<string>();
            if (IsTorFlow(flow)) tags.Add("tor");
            if (IsRemoteDesktop(flow)) tags.Add("rdp");
            if (IsWebBrowser(flow)) tags.Add("web");
            return new ClassifiedContextFlow<TlsContext>(flow.Flow, tags.ToArray(), flow.Context);

        }
        static bool IsTorFlow(ContextFlow<TlsContext> flow)
        {
            return flow.Context.TlsClientFlows?
                .Any(fact =>   string.IsNullOrWhiteSpace(fact.DomainName)
                            && fact.TlsServerCommonName == "N/A"
                            && fact.ServerNameEntropy > 3
                            && fact.Flow.DstPt > 443) 
                ?? false;
        }
        static bool IsRemoteDesktop(ContextFlow<TlsContext> flow)
        {
            return flow.Flow.DstPt == 3389;
        }
        static bool IsWebBrowser(ContextFlow<TlsContext> flow)
        {
            return flow.Context.TlsClientFlows?
                .Select(f => f.DomainName).Distinct().Count() > 2;
        }
        /// <summary>
        /// Detect the custom provided case in the source files. 
        /// </summary>
        /// <param name="sourceFiles">A collection of source files.</param>
        /// <param name="configuration">The configuration object.</param>
        /// <returns>Task that completes when all source items have been processed.</returns>
        public async Task Detect<TContext>(IObservable<FileInfo> sourceFiles, DetectConfiguration configuration, 
            Func<ContextBuilder, IStreamable<Empty, RawIpfixRecord>, IStreamable<Empty, ContextFlow<TContext>>> contextPrepareFunc, 
            Func<IStreamable<Empty, ContextFlow<TContext>>, IStreamable<Empty, ClassifiedContextFlow<TContext>>> contextClassifierFunc, 
            Func<IStreamable<Empty, ClassifiedContextFlow<TContext>>, CancellationToken, Task> contextConsumeFunc) 
        { 
            var cancellationToken = _cancellationTokenSource.Token;
            Config.ForceRowBasedExecution = true;
            var ctxBuilder = new ContextBuilder();
            var windowSize = TimeSpan.FromMinutes(15);
            var windowHop = TimeSpan.FromMinutes(5);

            // used only if reading from nfdump source files
            var nfdump = new NfdumpExecutor();
            var loader = new CsvLoader<RawIpfixRecord>();
            var subject = new Subject<RawIpfixRecord>();

            var loadedFlows = 0;
            void LoadedFlowsStatus()
            {
                if (++loadedFlows % 1000 == 0) Console.Error.Write('!');
            }

            loader.OnReadRecord += (object _, RawIpfixRecord value) => { LoadedFlowsStatus(); subject.OnNext(value); };
            if (configuration.WriteIntermediateFiles)
            {
                var records = new List<RawIpfixRecord>();
                loader.OnStartLoading += (_, filename) => { records.Clear(); };
                loader.OnReadRecord += (_, record) => { records.Add(record); };
                loader.OnFinish += (_, filename) => { WriteAllRecords(Path.Combine(configuration.IntermediateFilesPath, $"{filename}.csv"), records); };
            }
            var flowStream = subject.GetWindowedEventStream(x => DateTime.Parse(x.TimeStart).Ticks, windowSize, windowHop);

            // USER PROVIDED METHODS ------>
            var contextStream = contextPrepareFunc(ctxBuilder, flowStream);
            var eventStream = contextClassifierFunc(contextStream);
            var consumer = contextConsumeFunc(eventStream, cancellationToken);
            // <----------------------------

            // producer:
            var producer = sourceFiles
                .ForEachAsync(f => LoadRecordsFromFile(loader, nfdump, f, configuration.ReadFromCsvInput).Wait(), cancellationToken)
                .ContinueWith(_ => subject.OnCompleted());
            await Task.WhenAll(producer, consumer);
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
    }
}
