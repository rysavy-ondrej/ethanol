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
    public class ArtifactSamba : IpfixArtifact
    {
        [SourceMember("smb1cmd")]
        [Name("smb1cmd")]
        public string Smb1Command { get; set; }

        [SourceMember("smb2cmd")]
        [Name("smb2cmd")]
        public string Smb2Command { get; set; }
        [SourceMember("smbop")]
        [Name("smbop")]
        public string SmbOperation { get; set; }
        [SourceMember("smbfiletype")]
        [Name("smbfiletype")]
        public string SmbFileType { get; set; }
        [SourceMember("smbtree")]
        [Name("smbtree")]
        public string SmbTreePath { get; set; }
        [SourceMember("smbfile")]
        [Name("smbfile")]
        public string SmbFilePath { get; set; }
    }
}