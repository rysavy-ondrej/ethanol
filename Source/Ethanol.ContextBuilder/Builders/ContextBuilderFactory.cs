using Ethanol.ContextBuilder.Context;
using Ethanol.ContextBuilder.Observable;
using Ethanol.ContextBuilder.Plugins;
using Ethanol.ContextBuilder.Plugins.Attributes;
using System;

namespace Ethanol.ContextBuilder.Builders
{
    /// <summary>
    /// Factory class supporting to instantiating flow readers.
    /// </summary>
    public class ContextBuilderFactory : PluginFactory<IObservableTransformer<IpFlow, object>>
    {
        /// <inheritdoc/>>
        protected override bool FilterPlugins((Type Type, PluginAttribute Plugin) plugin)
        {
            return plugin.Plugin.Category == PluginCategory.Builder;
        }

        /// <summary>
        /// Gets the singleton of the factory.
        /// </summary>
        static public ContextBuilderFactory Instance { get; } = new ContextBuilderFactory();
    }

}
