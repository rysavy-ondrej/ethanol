using CsvHelper.Configuration.Attributes;
using Ethanol.Artifacts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Ethanol.Demo
{

    [ArtifactName("Tls")]
    public class ArtifactTls : IpfixArtifact
    {
        [Name("tlscver")]
        public string TlsClientVersion { get; set; }
        
        [Name("tlssver")]
        public string TlsServerVersion { get; set; }
        
        [Name("tlsciph")]
        public string TlsServerCipherSuite { get; set; }
        
        [Name("tlssni")]
        public string TlsServerName { get; set; }

        [Name("tlsscn")]
        public string TlsSubjectCommonName { get; set; }

        [Name("tlsson")]
        public string TlsSubjectOrganizationName { get; set; }        

        [Name("tlscont")]
        public string TlsContentType { get; set; }

        [Name("tlsja3")]
        public string Ja3Fingerprint { get; set; }

    }
}