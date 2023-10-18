using System;

namespace Ethanol.ContextBuilder.Plugins.Attributes
{
    /// <summary>
    /// Represents a custom attribute used to annotate and provide metadata for properties within a plugin's configuration object.
    /// This metadata aids in the dynamic interpretation and validation of configuration properties, ensuring they are correctly
    /// provided and consumed when the plugin is instantiated and executed.
    /// </summary>
    /// <remarks>
    /// Utilizing this attribute helps in maintaining a strong, self-descriptive contract for the configuration of plugins.
    /// It allows for both documentation generation tools and runtime systems to introspectively understand the expected 
    /// configuration parameters, their semantics, and any special behaviors or constraints associated with them.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class PluginParameterAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PluginParameterAttribute"/> class, specifying the core metadata 
        /// associated with a plugin configuration property.
        /// </summary>
        /// <param name="Name">The unique identifier or key used to specify the parameter within the configuration.</param>
        /// <param name="Flag">Provides additional contextual information or behaviors associated with the parameter. 
        /// For instance, it might indicate if the parameter is optional, if it has a default value, or other such behaviors.</param>
        /// <param name="Description">A brief human-readable description explaining the purpose, expected values, 
        /// and impact of the parameter on the plugin's behavior.</param>
        public PluginParameterAttribute(string Name, PluginParameterFlag Flag, string Description)
        {
            this.Name = Name;
            this.Flag = Flag;
            this.Description = Description;
        }

        /// <summary>
        /// Gets the unique identifier or key for the parameter. This name is used within the configuration to 
        /// specify the value for the parameter.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the flag that indicates additional properties or behaviors associated with the parameter.
        /// This could include information about the parameter's optionality, default behaviors, and more.
        /// </summary>
        public PluginParameterFlag Flag { get; }

        /// <summary>
        /// Gets a concise description that conveys the intent and expected values for the parameter. 
        /// This can be useful for generating documentation or user interface hints.
        /// </summary>
        public string Description { get; }
    }

}
