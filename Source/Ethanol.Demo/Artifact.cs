using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Ethanol.Demo
{
    public static class ArtifactFactory
    {
        public static Dictionary<string, Type> _registeredArtifacts = new Dictionary<string, Type>();

        public static void LoadArtifactsFromAssembly(Assembly assembly)
        {
            (Type, ArtifactNameAttribute) getArtifactName(Type type)
            {
                var attName = type.GetCustomAttribute<ArtifactNameAttribute>();
                return (type,attName);
            }
            foreach(var artifactType in assembly.GetTypes().Select(getArtifactName).Where(t => t.Item2 != null))
            {
                _registeredArtifacts.Add(artifactType.Item2.Name, artifactType.Item1);
            }
        }

        public static Type GetArtifact(string artifactName)
        {
            return _registeredArtifacts[artifactName];
        }
    }

    public abstract class ArtifactBuilder
    {
        public abstract Type ArtifactType { get; }
        public abstract Type OutputType { get; }

        public abstract Func<Artifact, bool> GetPredicate(Artifact target);

    }

    /// <summary>
    /// Represents an artifact builder. It is expression thjat can be used either as
    /// a function to be executed on input collection of type <typeparamref name="OutputType"/> or
    /// to generate query string against the database with data.
    /// </summary>
    /// <typeparam name="TArtifact">The type of artifact that owns the builder.</typeparam>
    /// <typeparam name="OutputType">The type of artifact that is being selected by the builder from the datasource.</typeparam>
    public class ArtifactBuilder<TArtifact, TOutput> : ArtifactBuilder where TArtifact : Artifact
                                                                       where TOutput : Artifact
    {
        public ArtifactBuilder(Expression<Func<TArtifact, TOutput, bool>> predicate)
        {
            Predicate = predicate ?? throw new ArgumentNullException(nameof(predicate));
        }

        /// <summary>
        /// The predicate to be executed on the input data source.
        /// </summary>        
        public Expression<Func<TArtifact, TOutput, bool>> Predicate { get; }

        public override Type ArtifactType => typeof(TArtifact);

        public override Type OutputType => typeof(TOutput);

        /// <summary>
        /// Compiles the predicate expression to a function.
        /// </summary>
        /// <returns>A function representing the predicate.</returns>
        public Func<TArtifact, TOutput, bool> Compile() => Predicate.Compile();

        public override Func<Artifact, bool> GetPredicate(Artifact target)
        {
            var compiledFunc = Compile();
            bool predicate(Artifact x)
            {
                var obj1 = (TArtifact)target;
                var obj2 = (TOutput)x;
                return compiledFunc(obj1, obj2);
            }
            return predicate;
        }
    }


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
    public class ArtifactTcpFlow
    {

    }

    [ArtifactName("Dns")]
    public class ArtifactDnsFlow : Artifact
    {
            [Index(0)]
            public string FirstSeen { get; set; }

            [Index(1)]
            public string SrcIp { get; set; }

            [Index(2)]
            public string DstIp { get; set; }

            [Index(3)]
            public int DnsFlag { get; set; }

            [Index(4)]
            public int DnsQuestionType { get; set; }

            [Index(5)]
            public string DnsQuestionName { get; set; }

            [Index(6)]
            public string DnsResponseName { get; set; }

            [Index(7)]
            public string DnsResponseData { get; set; }

            [Index(8)]
            public int DnsResponseCode { get; set; }

            [Index(9)]
            public string Packets { get; set; }

            [Index(10)]
            public string Bytes { get; set; }

        public override string Operation => throw new NotImplementedException();

        public override IEnumerable<ArtifactBuilder> Builders => throw new NotImplementedException();
    }
    public class ArtifactHttpFlow
    {

    }

    public class ArtifactTlsFlow
    { 
    }

    public class ArtifactSambaFlow
    {
    }
}