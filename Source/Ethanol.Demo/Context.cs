using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Ethanol.Demo
{
    /// <summary>
    /// Represents a context for artifacts.
    /// </summary>
    class Context
    {
        /// <summary>
        /// COntext consits of (key,value) pairs of artifacts.
        /// </summary>
        private readonly List<KeyValuePair<string, Artifact>> _keyValuePairs;

        public Context()
        {
            _keyValuePairs = new List<KeyValuePair<string, Artifact>>();
        }

        /// <summary>
        /// Gets the collection of facts.
        /// </summary>
        public ICollection<KeyValuePair<string,Artifact>> Facts => _keyValuePairs;

        public void Add(string name, Artifact value)
        {
            _keyValuePairs.Add(KeyValuePair.Create(name, value));
        }

        public void Dump(TextWriter writer)
        {
            foreach(var fact in _keyValuePairs)
            {
                writer.Write($"{fact.Key}: ");
                fact.Value.Dump(writer);
                writer.WriteLine();
            }
        }

        public IEnumerable<KeyValuePair<string, Artifact>> GetArtifact(string name)
        {
            return _keyValuePairs.Where(x => x.Key == name);
        }
        public IEnumerable<KeyValuePair<string, TArtifact>> GetFacts<TArtifact>()
        {
            return _keyValuePairs.Where(x => x.Value is TArtifact).Cast<KeyValuePair<string, TArtifact>>();
        }
        public IEnumerable<KeyValuePair<string, Artifact>> GetFacts(Type type)
        {
            return _keyValuePairs.Where(x => x.Value.GetType() == type);
        }
    }
}
