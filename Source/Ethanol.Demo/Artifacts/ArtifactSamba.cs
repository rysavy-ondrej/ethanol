using CsvHelper.Configuration.Attributes;
using Ethanol.Artifacts;
using System;
using System.Collections.Generic;
using System.Net;

namespace Ethanol.Demo
{
    [ArtifactName("Samba")]
    public class ArtifactSamba : IpfixArtifact
    {        
        [Name("smb1cmd")]
        public string Smb1Command { get; set; }

        [Name("smb2cmd")]
        public string Smb2Command { get; set; }
        [Name("smbop")]
        public string SmbOperation { get; set; }
        [Name("smbfiletype")]
        public string SmbFileType { get; set; }

        [Name("smbtree")]
        public string SmbTreePath { get; set; }
        [Name("smbfile")]
        public string SmbFilePath { get; set; }
    }
}