using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Ethanol.Demo
{
    /// <summary>
    /// Base class for all artifacts.
    /// </summary>
    public abstract class Artifact
    {
        public virtual IEnumerable<ArtifactBuilder> Builders => new ArtifactBuilder[] { };

        public object Field(string name)
        {
            var p = GetType().GetProperty(name);
            return p.GetValue(this);
        }

        public string[] Fields => GetType().GetProperties().Where(p => p.GetCustomAttributes(typeof(CsvHelper.Configuration.Attributes.IndexAttribute),false).Length > 0).Select(x => x.Name).ToArray();

        public void Dump(TextWriter writer)
        {
            var fields = String.Join(',', Fields.Select(x => $"{x}={Field(x)}"));
            writer.Write($"{{ {fields} }}");
              
        }

        internal void DumpYaml(IndentedTextWriter writer)
        {
            foreach (var field in Fields.Select(x => $"{x}: {Field(x)}"))
            {
                writer.WriteLine(field);
            }
        }
    }
}