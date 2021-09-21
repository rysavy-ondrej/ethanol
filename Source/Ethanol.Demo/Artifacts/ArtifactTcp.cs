using AutoMapper;
using AutoMapper.Configuration.Annotations;
using CsvHelper.Configuration.Attributes;
using Ethanol.Artifacts;
using System;
using System.Collections.Generic;
using System.Net;

namespace Ethanol.Demo
{
    [AutoMap(typeof(RawIpfixRecord))]
    public class ArtifactTcp : IpfixArtifact
    {
        [SourceMember("flg")]
        [Name("flg")]
        public string Flags { get; set; }

        [SourceMember("pps")]
        [Name("pps")]
        public string PacketPerSecond { get; set; }
        [SourceMember("bps")]
        [Name("bps")]
        public string BitsPerSeconds { get; set; }
        [SourceMember("bpp")]
        [Name("bpp")]
        public string BytesPerPacket { get; set; }
        [SourceMember("tcpwinsize")]
        [Name("tcpwinsize")]
        public string TcpWindowSize { get; set; }
        [SourceMember("tcpsynsize")]
        [Name("tcpsynsize")]
        public string TcpSynSize { get; set; }
        [SourceMember("tcpttl")]
        [Name("tcpttl")]
        public string TcpTimeToLive { get; set; }

    }
}