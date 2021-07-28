using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Ethanol.Demo
{


    public abstract class ArtifactBuilder
    {
    }

    /// <summary>
    /// Represents an artifact builder. It is expression thjat can be used either as
    /// a function to be executed on input collection of type <typeparamref name="OutputType"/> or
    /// to generate query string against the database with data.
    /// </summary>
    /// <typeparam name="ArtifactType">The type of artifact that owns the builder.</typeparam>
    /// <typeparam name="OutputType">The type of artifact that is being selected by the builder from the datasource.</typeparam>
    public class ArtifactBuilder<ArtifactType, OutputType>
    {
        public ArtifactBuilder(Expression<Func<ArtifactType, OutputType, bool>> predicate)
        {
            Predicate = predicate ?? throw new ArgumentNullException(nameof(predicate));
        }

        /// <summary>
        /// The predicate to be executed on the input data source.
        /// </summary>        
        public Expression<Func<ArtifactType, OutputType, bool>> Predicate { get; }

        /// <summary>
        /// Compiles the predicate expression to a function.
        /// </summary>
        /// <returns>A function representing the predicate.</returns>
        public Func<ArtifactType, OutputType, bool> Compile() => Predicate.Compile();
    }


    /// <summary>
    /// Base class for all artifacts.
    /// </summary>
    public abstract class Artifact
    {
        [ArtifactField]
        public long Id { get; set; }


        public abstract string Operation { get; }
        public IEnumerable<ArtifactBuilder> Builders { get; }

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

    public enum FlowOperation
    {
        FlowStart,
        FlowEnd
    }
    public class FlowArtifact : Artifact<FlowOperation>
    {

        [ArtifactField]
        public string ApplicationProtocol { get; set; }

        [ArtifactField]
        public string DestIp { get; set; }

        [ArtifactField]
        public int DestPort { get; set; }
        
        [ArtifactField]
        public DateTimeOffset EndTime { get; set; }

        [ArtifactField]
        public long InBytes { get; set; }

        [ArtifactField]
        public int NetworkDirection { get; set; }

        [ArtifactField]
        public long OutBytes { get; set; }

        [ArtifactField]
        public int PacketCount { get; set; }

        [ArtifactField]
        public string ProtocolInfo { get; set; }

        [ArtifactField]
        public int Protocol { get; set; }

        [ArtifactField]
        public string SrcIp { get; set; }

        [ArtifactField]
        public int SrcPort { get; set; }

        [ArtifactField]
        public DateTimeOffset StartTime { get; set; }

        [ArtifactField]
        public int TcpFlags { get; set; }

        [ArtifactField]
        public int TransportProtocol { get; set; }
    }

    public class ArtifactDefaultFlow
    {

    }
    public class ArtifactExtendedFlow
    {

    }

    public class ArtifactDnsFlow
    {

    }
    public class ArtifactHttpFlow
    {

    }

    public class ArtifactExtendedTlsFlow
    {

    }
}