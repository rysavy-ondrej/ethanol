using Ethanol.ContextBuilder.Builders;
using Ethanol.ContextBuilder.Context;
using Ethanol.ContextBuilder.Plugins;
using Ethanol.ContextBuilder.Plugins.Attributes;
using System;

namespace Ethanol.ContextBuilder.Readers
{

    /// <summary>
    /// Factory class supporting to instantiating flow readers.
    /// </summary>
    public class ReaderFactory : PluginFactory<FlowReader<IpfixObject>>
    {
        protected override bool FilterPlugins((Type Type, PluginAttribute Plugin) plugin)
        {
            return plugin.Plugin.PluginType == PluginType.Reader;
        }
        /// <summary>
        /// Gets the singleton of the factory.
        /// </summary>
        static public ReaderFactory Instance { get; } = new ReaderFactory();
    }
}
