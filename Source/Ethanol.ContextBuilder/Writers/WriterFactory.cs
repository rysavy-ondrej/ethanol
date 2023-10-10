using Ethanol.ContextBuilder.Observable;
using Ethanol.ContextBuilder.Plugins;
using Ethanol.ContextBuilder.Plugins.Attributes;
using Ethanol.ContextBuilder.Polishers;
using System;

namespace Ethanol.ContextBuilder.Writers
{
    /// <summary>
    /// Factory class for creating context writers.
    /// </summary>
    public class WriterFactory : PluginFactory<ContextWriter<ObservableEvent<IpTargetHostContext>>>
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
