namespace Ethanol.ContextBuilder.Plugins
{

    /// <summary>
    /// Represents a record that captures the essential details required to instantiate a plugin module. This includes information 
    /// such as the module's name and its associated configuration parameters.
    /// </summary>
    /// <remarks>
    /// This specification is a convenient mechanism for dynamically constructing and configuring modules within the plugin 
    /// infrastructure based on a concise string representation.
    /// </remarks>
    public record PluginCreateRecipe(string Name, string ConfigurationString)
    {
        /// <summary>
        /// Converts a module string representation into a <see cref="PluginCreateRecipe"/> object. The module string format is 
        /// expected to be "ModuleName:key1=val1,...,keyn=valn". This method provides a mechanism to transform such a string 
        /// into a structured representation, facilitating the dynamic instantiation and configuration of plugin modules.
        /// </summary>
        /// <param name="input">The module string that contains the name of the module followed by key-value pairs separated by commas.</param>
        /// <returns>
        /// A <see cref="PluginCreateRecipe"/> instance that captures the name of the module and its associated configuration.
        /// If the input string does not follow the expected format, a default instance with an empty configuration string is returned.
        /// </returns>
        /// <example>
        /// For an input string "MyModule:key1=val1,key2=val2", the returned record would have a Name of "MyModule" and a 
        /// ConfigurationString of "{key1: val1, key2: val2}".
        /// </example>
        public static PluginCreateRecipe Parse(string input)
        {
            var splitIndex = input.IndexOf(':');
            if (splitIndex > 0)
            {
                var name = input.Substring(0, splitIndex);
                var rest = input.Substring(splitIndex + 1).Trim();
                // Ensure that we have { } in JSON string
                if (!rest.StartsWith('{')) rest = "{" + rest;
                if (!rest.EndsWith('}')) rest = rest + "}";
                var yamlString = rest.Replace("=", ": ");
                return new PluginCreateRecipe(name, yamlString);
            }
            else
            {
                return new PluginCreateRecipe(input, string.Empty);
            }
        }
    }
}
