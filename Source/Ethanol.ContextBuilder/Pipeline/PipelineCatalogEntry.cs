using Ethanol.Catalogs;
using Ethanol.ContextBuilder.Context;
using Ethanol.ContextBuilder.Observable;
using Ethanol.ContextBuilder.Polishers;
using Ethanol.ContextBuilder.Readers;
using Ethanol.ContextBuilder.Writers;
using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;

namespace Ethanol.ContextBuilder.Pipeline
{

    /// <summary>
    /// Provides methods for building and initializing pipelines in the Ethanol environment.
    /// </summary>
    public static class PipelineCatalogEntry
    {

        // TODO: Do it more generic>
        // IFlowReader -> ITransformer -> ... -> ITransformer -> IContextWriter 
        //
        // TODO: Replace I/O progress actions with monitor transformers... 

        /// <summary>
        /// Creates a pipeline for building IP Host Contexts using the specified configuration, reader, writer, and callback actions.
        /// </summary>
        public static EthanolPipeline CreateIpHostContextBuilderPipeline(
        this ContextBuilderCatalog catalog,
        IDataReader<IpFlow>[] readers,
        IDataWriter<ObservableEvent<IpTargetHostContext>>[] writers,
        IObservableTransformer<IpFlow, ObservableEvent<IpHostContext>> builder,
        IObservableTransformer<ObservableEvent<IpHostContext>, ObservableEvent<IpHostContextWithTags>> enricher,
        IObservableTransformer<ObservableEvent<IpHostContextWithTags>, ObservableEvent<IpTargetHostContext>> refiner,
        Action<int> onInputConsumed,
        Action<int> onOuputProduced)
        {
            // Set up the pipeline flow:
            // 1. Consumes IP flows from the reader.
            // 2. Constructs IP host contexts using the builder.
            // 5. Enriches the contexts using the enricher.
            // 6. Polishes the enriched contexts.
            // 7. Writes the polished contexts using the writer.
            readers.Merge().Do(x => onInputConsumed(1))
                .Subscribe(builder);

            builder
                .Subscribe(enricher);

            enricher.Do(x => onOuputProduced(1))
                .Subscribe(refiner);

            foreach (var writer in writers)
            {
                refiner
                    .Subscribe(writer);
            }

            var nodes = readers.Cast<IPipelineNode>().Append(builder).Append(enricher).Append(refiner).Concat(writers);

            // Return a new instance of the Ethanol pipeline, comprising the various components.
            return new EthanolPipeline(nodes.ToArray(),
                (CancellationToken ct) => readers.Select(r => r.ReadAllAsync(ct)).ToArray());
        }
    }
}
