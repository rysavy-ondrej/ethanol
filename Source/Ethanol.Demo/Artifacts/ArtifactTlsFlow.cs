using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.Net;

namespace Ethanol.Demo
{
    /// <summary>
    /// This class defines a simple interval in time and some common operations.
    /// See https://www.cs.uct.ac.za/mit_notes/database/htmls/chp18.html#concepts-of-time.
    /// </summary>
    public record DateTimeInterval(DateTime Start, TimeSpan Duration)
    {
        public DateTimeInterval(DateTime point, TimeSpan before, TimeSpan after) : this(point-before, before+after) {}

        public bool Before(DateTime e1)
        {
            return e1 < Start;
        }
        public bool Before(DateTimeInterval e1)
        {
            return e1.Start + e1.Duration < Start;
        }
        public bool During(DateTime e1)
        {
            return e1 >= Start && e1 < Start + Duration;
        }
        public bool During(DateTimeInterval e1)
        {
            return e1.Start + Duration >= Start && e1.Start < Start + Duration;
        }

    }

 

    [ArtifactName("Tls")]
    public class ArtifactTlsFlow : Artifact
    {  
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
        public TimeSpan DurationTimeSpan => TimeSpan.TryParse(Duration, out var d) ? d : TimeSpan.MinValue;
        [Ignore]
        public IPAddress SrcIpAddress => IPAddress.TryParse(SrcIp, out var x) ? x : null;
        [Ignore]
        public IPAddress DstIpAddress => IPAddress.TryParse(DstIp, out var x) ? x : null;

        public override IEnumerable<ArtifactBuilder> Builders
        {
            get
            {

                if (this.DstPt == 443)
                {   // this is HTTPS
                    return new ArtifactBuilder[] { DomainNameBuilder, SurroundingTls, ReverseTls, RelatedHttp };
                }
                else
                {
                    return new ArtifactBuilder[] { DomainNameBuilder, SurroundingTls, ReverseTls };
                }
            }
        }
        #region Builders
        static ArtifactBuilder DomainNameBuilder = new ArtifactBuilder<ArtifactTlsFlow, ArtifactDnsFlow>("DstDomainName", (tls, dns) => dns.DstIp == tls.SrcIp && dns.DnsResponseData == tls.DstIp && new DateTimeInterval(dns.FirstSeenDateTime, TimeSpan.FromMinutes(10)).During(tls.FirstSeenDateTime));
        static ArtifactBuilder SurroundingTls = new ArtifactBuilder<ArtifactTlsFlow, ArtifactTlsFlow>("SurroundingTls", (tls, other) => ((tls.SrcIp == other.SrcIp && tls.DstIp == other.DstIp) || (tls.SrcIp == other.DstIp && tls.DstIp == other.SrcIp)) && new DateTimeInterval(tls.FirstSeenDateTime, TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5)).During(other.FirstSeenDateTime));
        static ArtifactBuilder ReverseTls = new ArtifactBuilder<ArtifactTlsFlow, ArtifactTlsFlow>("ReverseTls", (tls, other) => tls.SrcIp == other.DstIp && tls.DstIp == other.SrcIp && tls.SrcPt == other.DstPt && tls.DstPt == other.SrcPt && new DateTimeInterval(tls.FirstSeenDateTime, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1)).During(other.FirstSeenDateTime));
        static ArtifactBuilder RelatedHttp = new ArtifactBuilder<ArtifactTlsFlow, ArtifactHttpFlow>("RelatedHttp", (tls, http) => (tls.SrcIp == http.SrcIp && tls.DstIp == http.DstIp) && new DateTimeInterval(tls.FirstSeenDateTime, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1)).During(http.FirstSeenDateTime));

        #endregion

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
