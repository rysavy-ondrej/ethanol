using Ethanol.Catalogs;
using Ethanol.ContextBuilder.Builders;
using Ethanol.ContextBuilder.Context;
using Ethanol.ContextBuilder.Enrichers;
using Ethanol.ContextBuilder.Observable;
using Ethanol.ContextBuilder.Readers;
using Ethanol.ContextBuilder.Polishers;
using Ethanol.ContextBuilder.Writers;
using NRules.Diagnostics;
using System;
using System.Reactive.Linq;

namespace Ethanol.ContextBuilder.Pipeline
{

    public static class EthanolEnvironmentPipelines
    {
        public static EthanolPipeline CreateIpHostContextBuilderPipeline(this ContextBuilderCatalog catalog, PipelineConfiguration configuration, IFlowReader<IpFlow> reader, ContextWriter<ObservableEvent<IpTargetHostContext>> writer, Action<int> onInputConsumed, Action<int> onOuputProduced)
        {
            var hostFilter = HostBasedFilter.FromHostPrefix(configuration.TargetHostPrefix);
            var builder = new IpHostContextBuilder(configuration.WindowSize, configuration.WindowHop);
            var enricher = new IpHostContextEnricher(configuration.TagEnricherConfiguration.GetTagProvider());
            var polisher = new IpHostContextPolisher();
            
            reader.Do(x => onInputConsumed(1))
                .Subscribe(builder);
            builder
                .Where(hostFilter.Evaluate)                // performs filtering on the generated context, not all context are further processed
                .Delay(configuration.EnricherDelay)        // this is to delay outputs from builder to enricher in order to wait to flow tags
                .Subscribe(enricher);
            enricher.Do(x => onOuputProduced(1))
                .Subscribe(polisher);
            polisher
                .Subscribe(writer);

            return new EthanolPipeline(reader, builder, enricher, polisher, writer);
        }
    }
}
