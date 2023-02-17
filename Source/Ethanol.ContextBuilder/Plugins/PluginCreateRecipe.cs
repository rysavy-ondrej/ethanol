using System.Collections.Generic;

namespace Ethanol.ContextBuilder.Plugins
{

    /// <summary>
    /// Records the specification of the reader/builder/writer module.
    /// </summary>
    /// <param name="Name">The name of the module.</param>
    /// <param name="Parameters">The parameters used to create the instance of the module.</param>
    public record PluginCreateRecipe(string Name, string ConfigurationString)
    {
        /// <summary>
        /// Parses module string. It is ModuleName:key1=val1,...,keyn=valn
        /// </summary>
        /// <param name="input">The input module string.</param>
        /// <returns>New module specification object.</returns>
        public static PluginCreateRecipe Parse(string input)
        {
            var splitIndex = input.IndexOf(':');
            if (splitIndex > 0)
            {
                var name = input.Substring(0, splitIndex);
                var rest = input.Substring(splitIndex + 1).Trim();
                // ensure that we have { } in json string
                if (!rest.StartsWith('{')) rest = "{" + rest;
                if (!rest.EndsWith('}')) rest = rest + "}";
                var yamlString = rest.Replace("=", ": ");
                return new PluginCreateRecipe(name, yamlString);
            }
            else
            {
                return new PluginCreateRecipe(input, "{ }");
            }
        }

        private static KeyValuePair<string, string> splitToPair(string input, char sep)
        {
            var splitIndex = input.IndexOf(sep);
            if (splitIndex > 0)
            {
                var key = input.Substring(0, splitIndex);
                var val = input.Substring(splitIndex + 1);
                return new KeyValuePair<string, string>(key, val);
            }
            return new KeyValuePair<string, string>(input, string.Empty);
        }
    }
}
