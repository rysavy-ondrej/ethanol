using System;

namespace Ethanol.ContextBuilder.Plugins.Attributes
{
    /// <summary>
    /// The type of plugin object.
    /// </summary>
    public enum PluginType { Reader, Writer, Builder }

    /// <summary>
    /// A class attribute providing meta infomrmation on the plugin implementation.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class PluginAttribute : Attribute
    {
        public PluginAttribute(PluginType pluginType, string Name, string Description, string DocUrl = null)
        {
            PluginType = pluginType;
            this.Name = Name;
            this.Description = Description;
            this.DocUrl = DocUrl;
        }

        /// <summary>
        /// Plugin type.
        /// </summary>
        public PluginType PluginType { get; }
        /// <summary>
        /// Plugin name. The name is used for searching among the avilable plugins.
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// The description protividing basic information on the plugin.
        /// </summary>
        public string Description { get; }
        /// <summary>
        /// URL of detailed documentation of the plugin.
        /// </summary>
        public string DocUrl { get; }
    }

    /// <summary>
    /// Used to annotate the Create method for instantiating the plugin. The method should be static and
    /// accept a single argument representing the plugin configuration.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class PluginCreateAttribute : Attribute
    {

    }

    /// <summary>
    /// Detemines whether the configuratino parameter is required or it is optional.
    /// </summary>
    public enum PluginParameterFlag { Required, Optional }
    /// <summary>
    /// Annotates the property in plugin's configuration object.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class PluginParameterAttribute : Attribute
    {
        /// <summary>
        /// Creates a new attribute for plugin parameter.
        /// </summary>
        /// <param name="Name">The name of the parameter.</param>
        /// <param name="Flag">The flag of the parameter.</param>
        /// <param name="Description">The short description of the parameter.</param>
        public PluginParameterAttribute(string Name, PluginParameterFlag Flag, string Description)
        {
            this.Name = Name;
            this.Flag = Flag;
            this.Description = Description;
        }
        /// <summary>
        /// The name of the paremeter in the configuration.
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// Flag specifying additional properties of the parameter.
        /// </summary>
        public PluginParameterFlag Flag { get; }
        /// <summary>
        /// The short description of the parameter.
        /// </summary>
        public string Description { get; }
    }
}
