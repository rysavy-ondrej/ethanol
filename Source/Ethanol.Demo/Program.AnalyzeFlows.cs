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
        /// Detects TOR communication from context in the collection of <paramref name="sourceFiles"/>.
        /// </summary>
        /// <param name="sourceFiles">The observable collection of source files.</param>
        /// <param name="configuration">The configuration.</param>
        /// <returns>Task that completes when the processing is done.</returns>
        Task AnalyzeFlowsInFiles(IObservable<FileInfo> sourceFiles, FlowProcessor.Configuration configuration)
        {
            var flowProcessor = new FlowProcessor(configuration);
            IStreamable<Empty, ContextFlow<TlsContext>> PrepareTlsContext(ContextBuilder ctxBuilder, IStreamable<Empty, IpfixRecord> flowStream) => ctxBuilder.BuildTlsContext(flowStream);

            var torClassifier = new TorFlowClassifier();

            IStreamable<Empty, ClassifiedContextFlow<TlsContext>> ClassifyTor(IStreamable<Empty, ContextFlow<TlsContext>> contextStream) => contextStream.Select(ContextFlowClassifier<TlsContext>.Classify(torClassifier));
            var flows = 0;
            void PrintEvent(StreamEvent<ClassifiedContextFlow<TlsContext>> obj)
            {
                if (++flows % 1000 == 0) Console.Error.Write('.');
                if (obj.IsData)
                {
                    PrintStreamEvent($"{obj.Payload.Flow.Proto}@{obj.Payload.Flow.SrcIp}:{obj.Payload.Flow.SrcPt}-{obj.Payload.Flow.DstIp}:{obj.Payload.Flow.DstPt}", obj);
                }
            }
            Task ConsumeTorEvents(IStreamable<Empty, ClassifiedContextFlow<TlsContext>> torFlowsStream, CancellationToken cancellationToken) => torFlowsStream.ToStreamEventObservable().Where(f => f.IsEnd).ForEachAsync(PrintEvent, cancellationToken);

            return flowProcessor.LoadFromFiles(sourceFiles, PrepareTlsContext, ClassifyTor, ConsumeTorEvents, _cancellationTokenSource.Token);
        }

    }
}
