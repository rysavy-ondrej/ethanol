using Ethanol.ContextBuilder.Plugins;
using Ethanol.ContextBuilder.Plugins.Attributes;
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
            return plugin.Plugin.Category == PluginCategory.Writer;
        }
        /// <summary>
        /// Gets the singleton of the factory.
        /// </summary>
        static public WriterFactory Instance { get; } = new WriterFactory();
    }
}
