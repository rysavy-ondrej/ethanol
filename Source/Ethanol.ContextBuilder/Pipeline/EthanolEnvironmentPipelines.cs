using Ethanol.Catalogs;
using Ethanol.ContextBuilder.Builders;
using Ethanol.ContextBuilder.Context;
using Ethanol.ContextBuilder.Enrichers;
using Ethanol.ContextBuilder.Readers;
using Ethanol.ContextBuilder.Simplifier;
using Ethanol.ContextBuilder.Writers;
using NRules.Diagnostics;
using System;
using System.Reactive.Linq;

namespace Ethanol.ContextBuilder.Pipeline
{
    /// <summary>
    /// Represents a pipeline for processing ethanol data, which consists of a series of nodes that transform the data.
    /// </summary>
    public class EthanolPipeline
    {
        private readonly IPipelineNode[] nodes;

        /// <summary>
        /// Initializes a new instance of the <see cref="EthanolPipeline"/> class with the specified nodes.
        /// </summary>
        /// <param name="nodes">An array of <see cref="IPipelineNode"/> objects representing the nodes in the pipeline.</param>
        public EthanolPipeline(params IPipelineNode[] nodes)
        {
            this.nodes = nodes;
        }
    }

    /// <summary>
    /// Specifies the type of a pipeline node.
    /// </summary>
    public enum PipelineNodeType
    {
        Producer, Transformer, Filter, Sink
    }

    /// <summary>
    /// Represents a node in an ethanol processing pipeline, which can either produce, transform, filter, or sink data.
    /// </summary>
    public interface IPipelineNode
    {
        /// <summary>
        /// Gets the type of the pipeline node.
        /// </summary>
        PipelineNodeType NodeType { get; }
    }

    public static class EthanolEnvironmentPipelines
    {
        public static EthanolPipeline CreateIpHostContextBuilderPipeline(this ContextBuilderCatalog catalog, PipelineConfiguration configuration, IFlowReader<IpFlow> reader, ContextWriter<object> writer, Action<int> onInputConsumed, Action<int> onOuputProduced)
        {
            var builder = new IpHostContextBuilder(configuration.WindowSize, configuration.WindowHop);
            var enricher = new IpHostContextEnricher(configuration.HostTagEnricherConfiguration.GetHostTagProvider(), configuration.FlowTagEnricherConfiguration.GetFlowTagProvider());
            var simplifier = new IpHostContextSimplifier();

            reader.Do(x => onInputConsumed(1))
                .Subscribe(builder);
            builder.Delay(configuration.EnricherDelay)        // this is to delay outputs from builder to enricher in order to wait to flow tags...
                .Subscribe(enricher);
            enricher.Do(x => onOuputProduced(1))
                .Subscribe(simplifier);
            simplifier
                .Subscribe(writer);

            return new EthanolPipeline(reader, builder, enricher, simplifier, writer);
        }
    }

}
