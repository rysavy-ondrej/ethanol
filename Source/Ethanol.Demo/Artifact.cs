using CsvHelper.Configuration.Attributes;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace Ethanol.Demo
{
    /// <summary>
    /// Base class for all artifacts.
    /// </summary>
    public abstract class Artifact
    {


        /// <summary>
        /// Id string of the current artifact. It can have various format, hash value, index in table of objects, etc.
        /// </summary>
        [Ignore]
        public string Id { get; set; }

        /// <summary>
        /// Start of the event realted to the artifact, e.g., flow first seen value.
        /// </summary>
        [Ignore]
        public abstract DateTime Start { get; }
        /// <summary>
        /// Duration of the event related to the artifact, e.g., duration of the flow.
        /// </summary>
        [Ignore]
        public abstract TimeSpan Duration { get; }
        /// <summary>
        /// SOurce IP address of the flow object.
        /// </summary>
        [Ignore]
        public abstract IPAddress Source { get; }
        /// <summary>
        /// Destionation IP address of the flow object.
        /// </summary>
        [Ignore]
        public abstract IPAddress Destination { get; }


        /// <summary>
        /// Collection of context fact builders. The builders are used to enrich the context for the current object.
        /// </summary>
        public virtual IEnumerable<FactBuilder> Builders => new FactBuilder[] { };


        /// <summary>
        /// Gets the value of the given field. 
        /// </summary>
        /// <param name="name">Name of the field.</param>
        /// <returns>Field value.</returns>
        public object Field(string name)
        {
            var p = GetType().GetProperty(name);
            return p.GetValue(this);
        }

        /// <summary>
        /// Collection of field names of the current artifact.
        /// </summary>
        public string[] Fields => GetType().GetProperties().Where(p => p.GetCustomAttributes(typeof(CsvHelper.Configuration.Attributes.IndexAttribute), false).Length > 0).Select(x => x.Name).ToArray();


        /// <summary>
        /// Dumps the current object using JSON format.
        /// </summary>
        /// <param name="writer"></param>
        public void DumpJson(TextWriter writer)
        {
            var fields = String.Join(',', Fields.Select(x => $"{x}={Field(x)}"));
            writer.Write($"{{ {fields} }}");

        }

        /// <summary>
        /// Dumps the current object using YAML format.
        /// </summary>
        /// <param name="writer"></param>
        internal void DumpYaml(IndentedTextWriter writer)
        {
            writer.WriteLine($"Id: {Id}");
            foreach (var field in Fields.Select(x => $"{x}: {Field(x)}"))
            {
                writer.WriteLine(field);
            }
        }
    }
}