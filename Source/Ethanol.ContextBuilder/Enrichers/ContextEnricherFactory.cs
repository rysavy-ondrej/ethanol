using Ethanol.ContextBuilder.Observable;
using Ethanol.ContextBuilder.Plugins;
using Ethanol.ContextBuilder.Plugins.Attributes;
using System;

namespace Ethanol.ContextBuilder.Enrichers
{
    /// <summary>
    /// Represents a factory for creating instances of context enrichers. This factory filters and produces plugins that are categorized as enrichers.
    /// </summary>
    public class ContextEnricherFactory : PluginFactory<IObservableTransformer>
    {
        /// <summary>
        /// Filters plugins to only include those that belong to the 'Enricher' category.
        /// </summary>
        /// <param name="plugin">The tuple containing the type of the plugin and its associated attributes.</param>
        /// <returns>Returns <c>true</c> if the plugin belongs to the 'Enricher' category; otherwise, <c>false</c>.</returns>
        protected override bool FilterPlugins((Type Type, PluginAttribute Plugin) plugin)
        {
            return plugin.Plugin.Category == PluginCategory.Enricher;
        }

        /// <summary>
        /// Gets the singleton instance of the <see cref="ContextEnricherFactory"/> class.
        /// </summary>
        static public ContextEnricherFactory Instance { get; } = new ContextEnricherFactory();
    }
}
