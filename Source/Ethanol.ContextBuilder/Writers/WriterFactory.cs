using Ethanol.ContextBuilder.Observable;
using Ethanol.ContextBuilder.Plugins;
using Ethanol.ContextBuilder.Plugins.Attributes;
using Ethanol.ContextBuilder.Polishers;
using System;

namespace Ethanol.ContextBuilder.Writers
{
    /// <summary>
    /// Factory responsible for the instantiation and management of context writer plugins.
    /// </summary>
    /// <remarks>
    /// The <see cref="WriterFactory"/> class specializes in creating instances of context writers, specifically those that handle 
    /// <see cref="ObservableEvent{IpTargetHostContext}"/>. It extends from the generic <see cref="PluginFactory{T}"/>, ensuring it adheres to 
    /// factory standards for plugin creation and management.
    /// </remarks>
    public class WriterFactory : PluginFactory<ContextWriter<ObservableEvent<IpTargetHostContext>>>
    {
        /// <summary>
        /// Filters available plugins to identify those that fall under the 'Writer' category.
        /// </summary>
        /// <param name="plugin">A tuple containing the plugin type and its associated metadata.</param>
        /// <returns>Returns true if the plugin is categorized as a 'Writer'; otherwise, false.</returns>
        protected override bool FilterPlugins((Type Type, PluginAttribute Plugin) plugin)
        {
            return plugin.Plugin.Category == PluginCategory.Writer;
        }

        /// <summary>
        /// Provides a global point of access to the WriterFactory instance, ensuring a singleton pattern.
        /// </summary>
        /// <remarks>
        /// The singleton pattern ensures that only one instance of the <see cref="WriterFactory"/> class exists, 
        /// promoting a centralized access point to manage and retrieve context writer plugins.
        /// </remarks>
        static public WriterFactory Instance { get; } = new WriterFactory();
    }

}
