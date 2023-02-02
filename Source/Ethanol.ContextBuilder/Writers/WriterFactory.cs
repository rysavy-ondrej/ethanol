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
            return plugin.Plugin.PluginType == PluginType.Writer;
        }
        static public WriterFactory Instance => new WriterFactory();
    }
}
