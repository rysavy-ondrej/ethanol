using CsvHelper.Configuration.Attributes;
using System;
using System.CodeDom.Compiler;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Ethanol.Artifacts
{
    public static class ArtifactExtension
    {
        /// <summary>
        /// Gets the value of the given field. 
        /// </summary>
        /// <param name="name">Name of the field.</param>
        /// <returns>Field value.</returns>
        public static object GetArtifactFieldValue(this Artifact artifact, string name)
        {
            var p = artifact.GetType().GetProperty(name);
            return p.GetValue(artifact);
        }

        /// <summary>
        /// Collection of field names of the current artifact.
        /// </summary>
        public static string[] GetArtifactFields(this Artifact artifact)
        {
            return artifact.GetType().GetProperties().Where(p => HasCsvAttribute(p)).Select(x => x.Name).ToArray();
        }

        private static bool HasCsvAttribute(PropertyInfo p)
        {
            return p.GetCustomAttribute<CsvHelper.Configuration.Attributes.NameAttribute>(true) != null
                || p.GetCustomAttribute<CsvHelper.Configuration.Attributes.IndexAttribute>(true) != null
                || p.GetCustomAttribute<CsvHelper.Configuration.Attributes.NameIndexAttribute>(true) != null;
        }

        /// <summary>
        /// Dumps the current object using JSON format.
        /// </summary>
        /// <param name="writer"></param>
        public static void DumpJson(this Artifact artifact, TextWriter writer)
        {
            var fields = string.Join(',', artifact.GetArtifactFields().Select(x => $"{x}={artifact.GetArtifactFieldValue(x)}"));
            writer.Write($"{{ {fields} }}");

        }

        /// <summary>
        /// Dumps the current object using YAML format.
        /// </summary>
        /// <param name="writer"></param>
        public static void DumpYaml(this Artifact artifact, IndentedTextWriter writer)
        {
            writer.WriteLine($"Id: {artifact.Id}");
            foreach (var field in artifact.GetArtifactFields().Select(x => $"{x}: {artifact.GetArtifactFieldValue(x)}"))
            {
                writer.WriteLine(field);
            }
        }
    }

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

        public override string ToString()
        {
            var fields = String.Join(", ", this.GetArtifactFields().Select(x => $"{x}={this.GetArtifactFieldValue(x)}"));
            return $"{this.GetType().Name} {{ Id={this.Id}, {fields} }}";
        }
    }
}