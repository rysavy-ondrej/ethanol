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

namespace Ethanol.ContextBuilder
{
    public class EthanolPipeline
    {
        private readonly IPipelineNode[] nodes;

        public EthanolPipeline(params IPipelineNode[] nodes)
        {
            this.nodes = nodes;
        }
    }
    public enum PipelineNodeType
    {
        Producer, Transformer, Filter, Sink
    }
    public interface IPipelineNode
    {
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
            builder
                .Subscribe(enricher);
            enricher.Do(x => onOuputProduced(1))
                .Subscribe(simplifier);
            simplifier
                .Subscribe(writer);

            return new EthanolPipeline(reader,builder, enricher, simplifier, writer);
        }
    }

}
