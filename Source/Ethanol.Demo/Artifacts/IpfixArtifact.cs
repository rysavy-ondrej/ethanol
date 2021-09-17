using AutoMapper;
using AutoMapper.Configuration.Annotations;
using CsvHelper.Configuration.Attributes;
using Ethanol.Artifacts;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Ethanol.Demo
{
    /// <summary>
    /// Base class for all IPFIX artifacts.
    /// </summary>
    [AutoMap(typeof(RawIpfixRecord))]
    public abstract class IpfixArtifact : Artifact
    {
        [SourceMember("ts")]
        [Name("ts")]
        public  string FirstSeen { get; set; }
        [SourceMember("td")]
        [Name("td")]
        public  string Duration { get; set; }
        [SourceMember("pr")]
        [Name("pr")]
        public  string Protocol { get; set; }
        [SourceMember("sa")]
        [Name("sa")]
        public  string SrcIp { get; set; }
        [SourceMember("sp")]
        [Name("sp")]
        public string SrcPt { get; set; }
        [SourceMember("da")]
        [Name("da")]
        public  string DstIp { get; set; }
        [SourceMember("dp")]
        [Name("dp")]
        public string DstPt { get; set; }
        [SourceMember("ipkt")]
        [Name("ipkt")]
        public string Packets { get; set; }
        [SourceMember("ibyt")]
        [Name("ibyt")]
        public string Bytes { get; set; }
        [SourceMember("fl")]
        [Name("fl")]
        public string Flows { get; set; }
        [CsvHelper.Configuration.Attributes.Ignore]
        public override long StartTime => this.GetStart().Ticks;
        [CsvHelper.Configuration.Attributes.Ignore]
        public override long EndTime => StartTime + (this.GetDuration().Ticks + 1);
        [CsvHelper.Configuration.Attributes.Ignore]
        public int SourcePort => Int32.TryParse(SrcPt, out var result) ? result : 0;
        [CsvHelper.Configuration.Attributes.Ignore]
        public int DestinationPort => Int32.TryParse(DstPt, out var result) ? result : 0;

        [CsvHelper.Configuration.Attributes.Ignore]
        public string Key => $"{Protocol}#{SrcIp}:{SrcPt}->{DstIp}:{DstPt}";
    }
}