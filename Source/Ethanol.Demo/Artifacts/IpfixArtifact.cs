using CsvHelper.Configuration.Attributes;
using Ethanol.Artifacts;

namespace Ethanol.Demo
{
    /// <summary>
    /// Base class for all IPFIX artifacts.
    /// </summary>
    public abstract class IpfixArtifact : Artifact
    {
        /// <summary>
        /// Timestamp of the first packet of the flow.
        /// </summary>
        public abstract string FirstSeen { get; set; }
        /// <summary>
        /// Duration of the event related to the artifact, e.g., duration of the flow.
        /// </summary>
        public abstract string Duration { get; set; }
        /// <summary>
        /// Source IP address of the flow object.
        /// </summary>
        public abstract string SrcIp { get; set;  }
        /// <summary>
        /// Destionation IP address of the flow object.
        /// </summary>
        public abstract string DstIp { get; set;  }

        public abstract int SrcPt { get; set; }

        public abstract int DstPt { get; set; }

        public abstract string Protocol { get; set; }

        [Ignore]
        public override long StartTime => this.GetStart().Ticks;
        [Ignore]
        public override long EndTime => StartTime + (this.GetDuration().Ticks + 1);
    }
}