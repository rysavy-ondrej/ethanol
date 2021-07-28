using System;
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
        public abstract string Operation { get; }
        public abstract IEnumerable<ArtifactBuilder> Builders { get; }

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
    }


    /// <summary>
    /// Typed base class for all artifacts.
    /// </summary>
    /// <typeparam name="TOperation"></typeparam>
    public abstract class Artifact<TOperation> : Artifact
    {
        public override string Operation => ArtifactOperation.ToString();

        TOperation ArtifactOperation { get; set; }
    }

    public class ArtifactAllFlow : Artifact
    {
        public override string Operation => throw new NotImplementedException();

        public override IEnumerable<ArtifactBuilder> Builders => throw new NotImplementedException();
    }
}