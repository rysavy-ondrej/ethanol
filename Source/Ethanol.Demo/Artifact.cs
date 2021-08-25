using CsvHelper.Configuration.Attributes;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Ethanol.Demo
{
    public abstract class Artifact
    {
        /// <summary>
        /// Id string of the current artifact. It can have various format, hash value, index in table of objects, etc.
        /// </summary>
        [Ignore]
        public string Id { get; set; }


        /// <summary>
        /// Get the start time of the logical event interval for this artifact.
        /// </summary>
        [Ignore]
        public abstract long StartTime { get; }
        /// <summary>
        ///  Get the end time of the logical event interval for this artifact.
        /// </summary>
        [Ignore]
        public abstract long EndTime { get; }
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
        public void DumpYaml(IndentedTextWriter writer)
        {
            writer.WriteLine($"Id: {Id}");
            foreach (var field in Fields.Select(x => $"{x}: {Field(x)}"))
            {
                writer.WriteLine(field);
            }
        }

        public override string ToString()
        {
            var fields = String.Join(", ", Fields.Select(x => $"{x}={Field(x)}"));
            return $"{this.GetType().Name} {{ {fields} }}";
        }


        /// <summary>
        /// Loads the relevant facts of this artifact to the context.
        /// </summary>
        /// <param name="ctx">The target context to be enriched.</param>
        /// <param name="artifactServiceProvider">The artifact service provider.</param>
        public void LoadToContext(Context ctx, ArtifactServiceProvider artifactServiceProvider, params FactLoader[] loaders)
        {
            foreach (var builder in loaders)
            {
                var provider = artifactServiceProvider.GetService(builder.InputType);
                if (provider != null)
                {
                    foreach (var fact in builder.Query(this, provider.GetQueryable<Artifact>()))
                    {
                        ctx.Add(fact.Label, fact.Artifact);
                    }
                }
            }
        }
    }
}