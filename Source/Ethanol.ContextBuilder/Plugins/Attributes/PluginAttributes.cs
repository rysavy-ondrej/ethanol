using System;

namespace Ethanol.ContextBuilder.Plugins.Attributes
{
    public enum PluginType { Reader, Writer, Builder }
    [AttributeUsage(AttributeTargets.Class)]
    public class PluginAttribute : Attribute
    {
        public PluginAttribute(PluginType pluginType, string Name, string Description, string DocUrl=null)
        {
            PluginType = pluginType;
            this.Name = Name;
            this.Description = Description;
            this.DocUrl = DocUrl;
        }

        public PluginType PluginType { get; }
        public string Name { get; }
        public string Description { get; }
        public string DocUrl { get; }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class PluginCreateAttribute : Attribute
    {

    }

    public enum PluginParameterFlag { Required, Optional }
    [AttributeUsage(AttributeTargets.Parameter)]
    public class PluginParameterAttribute : Attribute
    {
        public PluginParameterAttribute(string Name, PluginParameterFlag Flag, string Description)
        {
            this.Name = Name;
            this.Flag = Flag;
            this.Description = Description;
        }

        public string Name { get; }
        public PluginParameterFlag Flag { get; }
        public string Description { get; }
    }
}
