using Ethanol.ContextBuilder.Context;
using Ethanol.ContextBuilder.Plugins;
using Ethanol.ContextBuilder.Plugins.Attributes;
using System;

namespace Ethanol.ContextBuilder.Readers
{
    /// <summary>
    /// Represents a factory responsible for instantiating flow readers that implement the <see cref="IFlowReader{IpFlow}"/> interface. 
    /// This factory aids in managing plugins categorized as 'Reader' and ensures only the appropriate plugins are instantiated.
    /// </summary>
    public class ReaderFactory : PluginFactory<IFlowReader<IpFlow>>
    {
        /// <summary>
        /// Filters the available plugins based on their category.
        /// Only plugins that belong to the 'Reader' category are considered.
        /// </summary>
        /// <param name="plugin">A tuple containing the Type and associated PluginAttribute of a potential plugin.</param>
        /// <returns>True if the plugin belongs to the 'Reader' category; otherwise, false.</returns>
        protected override bool FilterPlugins((Type Type, PluginAttribute Plugin) plugin)
        {
            return plugin.Plugin.Category == PluginCategory.Reader;
        }

        /// <summary>
        /// Provides a singleton instance of the ReaderFactory. 
        /// This ensures that there's only one ReaderFactory instance throughout the application's lifecycle.
        /// </summary>
        static public ReaderFactory Instance { get; } = new ReaderFactory();
    }
}
