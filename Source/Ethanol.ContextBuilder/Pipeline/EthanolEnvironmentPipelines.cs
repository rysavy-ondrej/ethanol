using Ethanol.Catalogs;
using Ethanol.ContextBuilder.Builders;
using Ethanol.ContextBuilder.Context;
using Ethanol.ContextBuilder.Enrichers;
using Ethanol.ContextBuilder.Observable;
using Ethanol.ContextBuilder.Polishers;
using Ethanol.ContextBuilder.Readers;
using Ethanol.ContextBuilder.Writers;
using System;
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
        /// <param name="reader">The flow reader to source IP flows.</param>
        /// <param name="writer">The writer to persist the processed observable events.</param>
        /// <param name="onInputConsumed">Callback action executed after an input has been consumed by the pipeline.</param>
        /// <param name="onOuputProduced">Callback action executed after an output has been produced by the pipeline.</param>
        /// <returns>A fully constructed Ethanol pipeline instance.</returns>
        public static EthanolPipeline CreateIpHostContextBuilderPipeline(
            this ContextBuilderCatalog catalog,
            PipelineConfiguration configuration,
            IFlowReader<IpFlow> reader,
            ContextWriter<ObservableEvent<IpTargetHostContext>> writer,
            Action<int> onInputConsumed,
            Action<int> onOuputProduced)
        {
            // Initialize the host filter based on the target host prefix specified in the configuration.
            var hostFilter = HostBasedFilter.FromHostPrefix(configuration.TargetHostPrefix);

            // Create instances of the builder, enricher, and polisher components.
            var builder = new IpHostContextBuilder(configuration.WindowSize, configuration.WindowHop);
            var enricher = new IpHostContextEnricher(configuration.TagEnricherConfiguration.GetTagProvider());
            var polisher = new IpHostContextPolisher();

            // Set up the pipeline flow:
            // 1. Consumes IP flows from the reader.
            // 2. Constructs IP host contexts using the builder.
            // 3. Filters contexts based on the host filter.
            // 4. Delays the flow to ensure flow tags are available for the enricher.
            // 5. Enriches the contexts using the enricher.
            // 6. Polishes the enriched contexts.
            // 7. Writes the polished contexts using the writer.
            reader.Do(x => onInputConsumed(1))
                .Subscribe(builder);
            builder
                .Where(hostFilter.Evaluate)
                .Delay(configuration.EnricherDelay)
                .Subscribe(enricher);
            enricher.Do(x => onOuputProduced(1))
                .Subscribe(polisher);
            polisher
                .Subscribe(writer);

            // Return a new instance of the Ethanol pipeline, comprising the various components.
            return new EthanolPipeline(new IPipelineNode[] { reader, builder, enricher, polisher, writer }, (CancellationToken ct) => reader.ReadAllAsync(ct));
        }
    }
}
