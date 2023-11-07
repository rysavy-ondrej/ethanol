using Ethanol.Catalogs;
using Ethanol.ContextBuilder.Builders;
using Ethanol.ContextBuilder.Context;
using Ethanol.ContextBuilder.Enrichers;
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
    public static class EthanolEnvironmentPipelines
    {
            /// <summary>
            /// Creates a pipeline for building IP Host Contexts using the specified configuration, reader, writer, and callback actions.
            /// </summary>
            /// <param name="catalog">The context builder catalog used as an extension method receiver.</param>
            /// <param name="configuration">The pipeline configuration settings.</param>
            /// <param name="readers">The flow readers to source IP flows.</param>
            /// <param name="writers">The writers to persist the processed observable events.</param>
            /// <param name="onInputConsumed">Callback action executed after an input has been consumed by the pipeline.</param>
            /// <param name="onOuputProduced">Callback action executed after an output has been produced by the pipeline.</param>
            /// <returns>A fully constructed Ethanol pipeline instance.</returns>
            public static EthanolPipeline CreateIpHostContextBuilderPipeline(
            this ContextBuilderCatalog catalog,
            IFlowReader<IpFlow>[] readers,
            ContextWriter<ObservableEvent<IpTargetHostContext>>[] writers,
            IpHostContextBuilder builder,
            IpHostContextEnricher enricher,
            IpHostContextPolisher polisher,
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
                .Subscribe(polisher);

            foreach (var writer in writers)
            {
                polisher
                    .Subscribe(writer);
            }

            var nodes = readers.Cast<IPipelineNode>().Append(builder).Append(enricher).Append(polisher).Concat(writers);

            // Return a new instance of the Ethanol pipeline, comprising the various components.
            return new EthanolPipeline(nodes.ToArray(), 
                (CancellationToken ct) =>  readers.Select(r=>r.ReadAllAsync(ct)).ToArray());
        }
    }
}
