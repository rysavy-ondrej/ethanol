using System.Collections.Generic;
using System.Linq;

namespace Ethanol.Demo
{
    /// <summary>
    /// Represents a context for artifacts.
    /// </summary>
    class Context
    {
        /// <summary>
        /// 
        /// </summary>
        private readonly Dictionary<string, Artifact> _keyValuePairs;

        public Context()
        {
            _keyValuePairs = new Dictionary<string, Artifact>();
        }

        public IEnumerable<object> Facts => _keyValuePairs.Values.Cast<object>();

        public void Add(string name, Artifact value)
        {
            _keyValuePairs.Add(name, value);
        }
    }
}
