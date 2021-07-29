using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.Net;

namespace Ethanol.Demo
{
    /// <summary>
    /// This class defines operators according to https://www.cs.uct.ac.za/mit_notes/database/htmls/chp18.html#concepts-of-time.
    /// </summary>
    public static class T
    {
        /// <summary>
        /// E1 occurs before E2.
        /// </summary>
        public static bool Before(DateTime e1, DateTime e2)
        {
            return e1 < e2;
        }

        /// <summary>
        /// E1 takes place during E2,S2
        /// </summary>
        public static bool During(DateTime e1, DateTime e2, TimeSpan s2)
        {
            return e1 >= e2 && e1 < e2 + s2;
        }
    }

    [ArtifactName("Tls")]
    public class ArtifactTlsFlow : Artifact
    {
        public override IEnumerable<ArtifactBuilder> Builders =>
            new[]
            {
                new ArtifactBuilder<ArtifactTlsFlow, ArtifactDnsFlow>("DstDomainName", (tls,dns) => dns.DstIp == tls.SrcIp && dns.DnsResponseData == tls.DstIp && T.During(tls.FirstSeenDateTime, dns.FirstSeenDateTime, TimeSpan.FromMinutes(10)) )
            };

        [Index(0)]
        public string FirstSeen { get; set; }
        [Index(1)]
        public string Duration { get; set; }
        [Index(2)]
        public string Protocol { get; set; }

        [Index(3)]
        public string SrcIp { get; set; }
        [Index(4)]
        public int SrcPt { get; set; }

        [Index(5)]
        public string DstIp { get; set; }
        [Index(6)]
        public int DstPt { get; set; }

        [Index(7)]
        public string TlsClientVersion { get; set; }
        [Index(8)]
        public string TlsServerVersion { get; set; }
        [Index(9)]
        public string TlsServerCipherSuite { get; set; }
        [Index(10)]
        public string TlsServerName { get; set; }
        [Index(11)]
        public string TlsSubjectOrganizationName { get; set; }
        [Index(12)]
        public int Packets { get; set; }
        [Index(13)]
        public long Bytes { get; set; }


        [Ignore]
        public DateTime FirstSeenDateTime => DateTime.TryParse(FirstSeen, out var d) ? d : DateTime.MinValue;
        [Ignore]
        public IPAddress SrcIpAddress => IPAddress.TryParse(SrcIp, out var x) ? x : null;
        [Ignore]
        public IPAddress DstIpAddress => IPAddress.TryParse(DstIp, out var x) ? x : null;

        public override string Operation => throw new NotImplementedException();
    }
}

// 0-Date first seen          
// 1-Duration 
// 2-Proto
// 3-Src IP Addr 
// 4-Src Pt                             
// 5-Dst IP Addr 
// 6-Dst Pt  
// 7-TLS Client version  
// 8-TLS Server version  
// 9-TLS Server cipher suite            
// 10-TLS server name (SNI)    
// 11-TLS Subject organisation name  
// 12-Packets    
// 13-Bytes
