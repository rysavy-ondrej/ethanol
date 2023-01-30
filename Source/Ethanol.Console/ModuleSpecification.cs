using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

namespace Ethanol.ContextBuilder
{
    /// <summary>
    /// Records the specification of the reader/builder/writer module.
    /// </summary>
    /// <param name="Name">The name of the module.</param>
    /// <param name="Parameters">The parameters used to create the instance of the module.</param>
    public record ModuleSpecification(string Name, IReadOnlyDictionary<string,string> Parameters)
    {
        /// <summary>
        /// Parses module string. It is ModuleName:key1=val1,...,keyn=valn
        /// </summary>
        /// <param name="input">The input module string.</param>
        /// <returns>New module specification object.</returns>
        public static ModuleSpecification Parse(string input)
        {
            var splitIndex = input.IndexOf(':');
            if (splitIndex > 0)
            {
                var name = input.Substring(0, splitIndex);
                var rest = input.Substring(splitIndex + 1);
                var kvs = rest.Split(',').Select(x => splitToPair(x, '='));
                return new ModuleSpecification(name, new Dictionary<string, string>(kvs));
            }
            else
            {
                return new ModuleSpecification(input, new Dictionary<string, string>());
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
            return new KeyValuePair<string, string>(input, String.Empty);
        }
    }
}
