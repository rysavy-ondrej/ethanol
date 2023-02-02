using Ethanol.ContextBuilder.Plugins;
using Ethanol.ContextBuilder.Plugins.Attributes;
using Ethanol.ContextBuilder.Readers;
using System;

namespace Ethanol.ContextBuilder.Writers
{
    /// <summary>
    /// Factory class supporting to instantiating flow readers.
    /// </summary>
    public class WriterFactory : PluginFactory<ContextWriter<object>>
    {
        protected override bool FilterPlugins((Type Type, PluginAttribute Plugin) plugin)
        {
            return plugin.Plugin.PluginType == PluginType.Writer;
        }
        /// <summary>
        /// Gets the singleton of the factory.
        /// </summary>
        static public WriterFactory Instance { get; } = new WriterFactory();
    }
}
