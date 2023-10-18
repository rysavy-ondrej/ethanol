using System;

namespace Ethanol.ContextBuilder.Plugins.Attributes
{
    /// <summary>
    /// Represents a custom attribute that provides metadata about a plugin implementation. This metadata can be used
    /// for various purposes such as reflection-based plugin loading, documentation generation, and more.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class PluginAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PluginAttribute"/> class with specified metadata.
        /// </summary>
        /// <param name="pluginType">The category of the plugin (e.g., Reader, Writer, Builder, Enricher).</param>
        /// <param name="Name">The name of the plugin.</param>
        /// <param name="Description">A brief description explaining the purpose or functionality of the plugin.</param>
        /// <param name="DocUrl">An optional URL pointing to the documentation or further information about the plugin. Defaults to null.</param>

        public PluginAttribute(PluginCategory pluginType, string Name, string Description, string DocUrl = null)
        {
            this.Category = pluginType;
            this.Name = Name;
            this.Description = Description;
            this.DocUrl = DocUrl;
        }

        /// <summary>
        /// Gets the category of the plugin.
        /// </summary>
        public PluginCategory Category { get; }

        /// <summary>
        /// Gets the name of the plugin.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets a description of the plugin.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Gets the URL pointing to the documentation or further details about the plugin. May be null if not provided.
        /// </summary>
        public string DocUrl { get; }
    }
}
