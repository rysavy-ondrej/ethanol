using CsvHelper.Configuration.Attributes;
using Ethanol.Artifacts;
using System;

namespace Ethanol.Demo
{
    /// <summary>
    /// Base class for all IPFIX artifacts.
    /// </summary>
    public abstract class IpfixArtifact : Artifact
    {
        [Name("ts")]
        public  string FirstSeen { get; set; }

        [Name("td")]
        public  string Duration { get; set; }

        [Name("pr")]
        public  string Protocol { get; set; }

        [Name("sa")]
        public  string SrcIp { get; set; }

        [Name("sp")]
        public string SrcPt { get; set; }

        [Name("da")]
        public  string DstIp { get; set; }

        [Name("dp")]
        public string DstPt { get; set; }

        [Name("ipkt")]
        public string Packets { get; set; }

        [Name("ibyt")]
        public string Bytes { get; set; }

        [Name("fl")]
        public string Flows { get; set; }

        [Ignore]
        public override long StartTime => this.GetStart().Ticks;
        [Ignore]
        public override long EndTime => StartTime + (this.GetDuration().Ticks + 1);
        [Ignore]
        public int SourcePort => Int32.TryParse(SrcPt, out var result) ? result : 0;
        [Ignore]
        public int DestinationPort => Int32.TryParse(DstPt, out var result) ? result : 0;

        [Ignore]
        public string Key => $"{Protocol}#{SrcIp}:{SrcPt}->{DstIp}:{DstPt}";
    }
}