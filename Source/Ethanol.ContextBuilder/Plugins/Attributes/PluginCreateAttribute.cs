using System;

namespace Ethanol.ContextBuilder.Plugins.Attributes
{
    /// <summary>
    /// Represents a custom attribute used to mark and identify the specific method responsible for instantiating 
    /// a plugin. This annotation is crucial when using reflection-based mechanisms for dynamic plugin creation, as it 
    /// indicates the correct entry point for plugin instantiation.
    /// </summary>
    /// <remarks>
    /// The method annotated with this attribute is expected to adhere to certain conventions:
    /// <para/>
    /// <list type="number">
    /// <item/> It should be a static method.
    /// <item/> It should accept a single argument, which will be the configuration data for the plugin.
    /// <item/> It should return an instance of the plugin or an interface the plugin implements.
    /// </list>
    /// <para/>
    /// By following these conventions, the system can reliably and dynamically create plugin instances 
    /// at runtime based on the provided configuration.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class PluginCreateAttribute : Attribute
    {
    }
}
