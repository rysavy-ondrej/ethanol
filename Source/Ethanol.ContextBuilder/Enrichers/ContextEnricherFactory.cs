using Ethanol.ContextBuilder.Observable;
using Ethanol.ContextBuilder.Plugins;
using Ethanol.ContextBuilder.Plugins.Attributes;
using System;

namespace Ethanol.ContextBuilder.Enrichers
{
    public class ContextEnricherFactory : PluginFactory<IObservableTransformer>
    {
        /// <inheritdoc/>>
        protected override bool FilterPlugins((Type Type, PluginAttribute Plugin) plugin)
        {
            return plugin.Plugin.Category == PluginCategory.Enricher;
        }

        /// <summary>
        /// Gets the singleton of the factory.
        /// </summary>
        static public ContextEnricherFactory Instance { get; } = new ContextEnricherFactory();
    }
}
