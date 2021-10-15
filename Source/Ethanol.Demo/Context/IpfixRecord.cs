using CsvHelper.Configuration.Attributes;
using System;

namespace Ethanol.Demo
{

    /// <summary>
    /// Represents a single record in the collection of TCP connection list.
    /// </summary>
    public partial class TcpconRecord
    {
        [Name("FlowKey")]
        public string FlowKeyString { get; set; }

        [Ignore]
        public FlowKey FlowKey => new FlowKey("TCP", LocalAddress, LocalPort, RemoteAddress, RemotePort);

        [Name("LocalAddress")]
        public string LocalAddress {  get; set; }

        [Name("LocalPort")]
        public int LocalPort { get; set; }

        [Name("RemoteAddress")]
        public string RemoteAddress { get; set; }

        [Name("RemotePort")]
        public int RemotePort { get; set; }

        [Name("State")]
        public string State { get; set; }

        [Name("ProcessName")]
        public string ProcessName { get; set; }

        [Name("CreationTime")]
        public DateTime CreationTime { get; set; }

        [Name("CurrentTime")]
        public DateTime CurrentTime { get; set; }
    }

    public partial class IpfixRecord
    {
        [Ignore]
        public FlowKey FlowKey => new FlowKey(Protocol, SrcIp, SrcPort, DstIp, DstPort);
        
        [Name("ProcessName")]
        public string ProcessName { get; set; }
    }

    /// <summary>
    /// Represents a single flow record as exported from nfdump. 
    /// This record contains many properties but only a few are relevant for further processing. 
    /// </summary>
    public partial class IpfixRecord
    {
        [Name("ts")]
        public string TimeStart { get; set; }
        [Name("te")]
        public string TimeEnd { get; set; }
        [Name("td")]
        public double TimeDuration { get; set; }
        [Name("sa")]
        public string SrcIp { get; set; }
        [Name("da")]
        public string DstIp { get; set; }
        [Name("sp")]
        public int SrcPort { get; set; }
        [Name("dp")]
        public int DstPort { get; set; }
        [Name("pr")]
        public string Protocol { get; set; }
        [Name("flg")]
        public string TcpFlags { get; set; }
        [Name("fwd")]
        public string fwd { get; set; }
        [Name("stos")]
        public string stos { get; set; }

        [Name("ipkt")]
        public int InPackets { get; set; }
        [Name("ibyt")]
        public int InBytes { get; set; }
        [Name("opkt")]
        public int OutPackets { get; set; }
        [Name("obyt")]
        public int OutBytes { get; set; }
        [Name("fl")]
        public string Flows { get; set; }
        [Name("bps")]
        public string bps { get; set; }
        [Name("pps")]
        public string pps { get; set; }
        [Name("bpp")]
        public string bpp { get; set; }
        [Name("in")]
        public string @in { get; set; }
        [Name("out")]
        public string @out { get; set; }

        [Name("sas")]
        public string sas { get; set; }
        [Name("das")]
        public string das { get; set; }
        [Name("smk")]
        public string smk { get; set; }
        [Name("dmk")]
        public string dmk { get; set; }
        [Name("dtos")]
        public string dtos { get; set; }
        [Name("dir")]
        public string dir { get; set; }
        [Name("nh")]
        public string nh { get; set; }
        [Name("nhb")]
        public string nhb { get; set; }
        [Name("svln")]
        public string svln { get; set; }
        [Name("dvln")]
        public string dvln { get; set; }
        [Name("ismc")]
        public string ismc { get; set; }

        [Name("odmc")]
        public string odmc { get; set; }
        [Name("idmc")]
        public string idmc { get; set; }
        [Name("osmc")]
        public string osmc { get; set; }
        [Name("mpls1")]
        public string mpls1 { get; set; }
        [Name("mpls2")]
        public string mpls2 { get; set; }
        [Name("mpls3")]
        public string mpls3 { get; set; }
        [Name("mpls4")]
        public string mpls4 { get; set; }
        [Name("mpls5")]
        public string mpls5 { get; set; }
        [Name("mpls6")]
        public string mpls6 { get; set; }
        [Name("mpls7")]
        public string mpls7 { get; set; }

        [Name("mpls8")]
        public string mpls8 { get; set; }
        [Name("mpls9")]
        public string mpls9 { get; set; }
        [Name("mpls10")]
        public string mpls10 { get; set; }
        [Name("cl")]
        public string cl { get; set; }
        [Name("sl")]
        public string sl { get; set; }
        [Name("al")]
        public string al { get; set; }
        [Name("ra")]
        public string ra { get; set; }
        [Name("eng")]
        public string eng { get; set; }
        [Name("exid")]
        public string exid { get; set; }
        [Name("tr")]
        public string tr { get; set; }
        [Name("apptag")]
        public string apptag { get; set; }

        [Name("hhost")]
        public string hhost { get; set; }
        [Name("hhost1")]
        public string hhost1 { get; set; }
        [Name("hhost2")]
        public string hhost2 { get; set; }
        [Name("hhost3")]
        public string hhost3 { get; set; }
        [Name("hurl")]
        public string hurl { get; set; }
        [Name("hurl1")]
        public string hurl1 { get; set; }
        [Name("scc")]
        public string scc { get; set; }
        [Name("dcc")]
        public string dcc { get; set; }
        [Name("scid")]
        public string scid { get; set; }
        [Name("scing")]
        public string scing { get; set; }

        [Name("scled")]
        public string scled { get; set; }
        [Name("svia")]
        public string svia { get; set; }
        [Name("srt")]
        public string srt { get; set; }
        [Name("sok")]
        public string sok { get; set; }
        [Name("sbye")]
        public string sbye { get; set; }
        [Name("ssts")]
        public string ssts { get; set; }
        [Name("srip")]
        public string srip { get; set; }
        [Name("saud")]
        public string saud { get; set; }
        [Name("svid")]
        public string svid { get; set; }
        [Name("spt")]
        public string spt { get; set; }
        [Name("rpkt")]
        public string rpkt { get; set; }

        [Name("roct")]
        public string roct { get; set; }
        [Name("rjit")]
        public string rjit { get; set; }
        [Name("rlst")]
        public string rlst { get; set; }
        [Name("rcod")]
        public string rcod { get; set; }
        [Name("rsc")]
        public string rsc { get; set; }
        [Name("rd")]
        public string rd { get; set; }
        [Name("asnt")]
        public string asnt { get; set; }
        [Name("asntmin")]
        public string asntmin { get; set; }
        [Name("asntmax")]
        public string asntmax { get; set; }
        [Name("acnt")]
        public string acnt { get; set; }

        [Name("acntmin")]
        public string acntmin { get; set; }
        [Name("acntmax")]
        public string acntmax { get; set; }
        [Name("asrt")]
        public string asrt { get; set; }
        [Name("asrtmin")]
        public string asrtmin { get; set; }
        [Name("asrtmax")]
        public string asrtmax { get; set; }
        [Name("ahist1")]
        public string ahist1 { get; set; }
        [Name("ahist2")]
        public string ahist2 { get; set; }
        [Name("ahist3")]
        public string ahist3 { get; set; }
        [Name("ahist4")]
        public string ahist4 { get; set; }

        [Name("ahist5")]
        public string ahist5 { get; set; }
        [Name("ahist6")]
        public string ahist6 { get; set; }
        [Name("ahist7")]
        public string ahist7 { get; set; }
        [Name("arlate")]
        public string arlate { get; set; }
        [Name("nrtt")]
        public string nrtt { get; set; }
        [Name("nsrt")]
        public string nsrt { get; set; }
        [Name("njdev")]
        public string njdev { get; set; }
        [Name("njavg")]
        public string njavg { get; set; }
        [Name("njmin")]
        public string njmin { get; set; }

        [Name("njmax")]
        public string njmax { get; set; }
        [Name("nddev")]
        public string nddev { get; set; }
        [Name("ndavg")]
        public string ndavg { get; set; }
        [Name("ndmin")]
        public string ndmin { get; set; }
        [Name("ndmax")]
        public string ndmax { get; set; }
        [Name("njcnt")]
        public string njcnt { get; set; }
        [Name("ndcnt")]
        public string ndcnt { get; set; }
        [Name("nrttmin")]
        public string nrttmin { get; set; }
        [Name("nrttmax")]
        public string nrttmax { get; set; }

        [Name("nrttcnt")]
        public string nrttcnt { get; set; }
        [Name("nsrtmin")]
        public string nsrtmin { get; set; }
        [Name("nsrtmax")]
        public string nsrtmax { get; set; }
        [Name("nsrtcnt")]
        public string nsrtcnt { get; set; }
        [Name("suid")]
        public string suid { get; set; }
        [Name("duid")]
        public string duid { get; set; }
        [Name("hos")]
        public string hos { get; set; }
        [Name("hosmaj")]
        public string hosmaj { get; set; }
        [Name("hosmin")]
        public string hosmin { get; set; }

        [Name("hosbld")]
        public string hosbld { get; set; }
        [Name("happ")]
        public string happ { get; set; }
        [Name("happmaj")]
        public string happmaj { get; set; }
        [Name("happmin")]
        public string happmin { get; set; }
        [Name("happbld")]
        public string happbld { get; set; }
        [Name("nooo")]
        public string nooo { get; set; }
        [Name("nretr")]
        public string nretr { get; set; }
        [Name("nooomin")]
        public string nooomin { get; set; }
        [Name("nooomax")]
        public string nooomax { get; set; }

        [Name("nooocnt")]
        public string nooocnt { get; set; }
        [Name("nretrmin")]
        public string nretrmin { get; set; }
        [Name("nretrmax")]
        public string nretrmax { get; set; }
        [Name("nretrcnt")]
        public string nretrcnt { get; set; }
        [Name("dnsqname1")]
        public string dnsqname1 { get; set; }
        [Name("dnsrname1")]
        public string dnsrname1 { get; set; }
        [Name("dnsrdata1")]
        public string dnsrdata1 { get; set; }
        [Name("dnsqname2")]
        public string dnsqname2 { get; set; }

        [Name("dnsrname2")]
        public string dnsrname2 { get; set; }
        [Name("dnsrdata2")]
        public string dnsrdata2 { get; set; }
        [Name("dnsqname3")]
        public string dnsqname3 { get; set; }
        [Name("dnsrname3")]
        public string dnsrname3 { get; set; }
        [Name("dnsrdata3")]
        public string dnsrdata3 { get; set; }
        [Name("dnsid")]
        public string dnsid { get; set; }
        [Name("dnsrcode")]
        public string dnsrcode { get; set; }
        [Name("dnsopcode")]
        public string dnsopcode { get; set; }

        [Name("dnsqrflag")]
        public string dnsqrflag { get; set; }
        [Name("dnsflags")]
        public string dnsflags { get; set; }
        [Name("dnsqcnt")]
        public string dnsqcnt { get; set; }
        [Name("dnsancnt")]
        public string dnsancnt { get; set; }
        [Name("dnsaucnt")]
        public string dnsaucnt { get; set; }
        [Name("dnsadcnt")]
        public string dnsadcnt { get; set; }
        [Name("dnsrname")]
        public string dnsrname { get; set; }
        [Name("dnsrclass")]
        public string dnsrclass { get; set; }

        [Name("dnsrtype")]
        public string dnsrtype { get; set; }
        [Name("dnsrttl")]
        public string dnsrttl { get; set; }
        [Name("dnsrdata")]
        public string DnsResponseData { get; set; }
        [Name("dnsqname")]
        public string DnsQueryName { get; set; }
        [Name("dnsqtype")]
        public string dnsqtype { get; set; }
        [Name("dnsqclass")]
        public string dnsqclass { get; set; }
        [Name("sourceid")]
        public string sourceid { get; set; }
        [Name("dhcpoip")]
        public string dhcpoip { get; set; }

        [Name("dhcphmac")]
        public string dhcphmac { get; set; }
        [Name("dhcptype")]
        public string dhcptype { get; set; }
        [Name("dhcpltime")]
        public string dhcpltime { get; set; }
        [Name("dhcpsip")]
        public string dhcpsip { get; set; }
        [Name("dhcpdname")]
        public string dhcpdname { get; set; }
        [Name("dhcphname")]
        public string dhcphname { get; set; }
        [Name("dhcpipreq")]
        public string dhcpipreq { get; set; }
        [Name("smb1cmd")]
        public string smb1cmd { get; set; }

        [Name("smb2cmd")]
        public string smb2cmd { get; set; }
        [Name("smbtree")]
        public string smbtree { get; set; }
        [Name("smbfile")]
        public string smbfile { get; set; }
        [Name("smbfiletype")]
        public string smbfiletype { get; set; }
        [Name("smbop")]
        public string smbop { get; set; }
        [Name("smbdel")]
        public string smbdel { get; set; }
        [Name("smberr")]
        public string smberr { get; set; }
        [Name("tcpttl")]
        public string tcpttl { get; set; }
        [Name("tcpsynsize")]
        public string tcpsynsize { get; set; }

        [Name("tcpwinsize")]
        public string tcpwinsize { get; set; }
        [Name("tdsreq")]
        public string tdsreq { get; set; }
        [Name("tdsver")]
        public string tdsver { get; set; }
        [Name("tdscver")]
        public string tdscver { get; set; }
        [Name("tdssver")]
        public string tdssver { get; set; }
        [Name("tdsdb")]
        public string tdsdb { get; set; }
        [Name("tdsuser")]
        public string tdsuser { get; set; }
        [Name("tdshost")]
        public string tdshost { get; set; }
        [Name("tdsres")]
        public string tdsres { get; set; }

        [Name("tdstoken")]
        public string tdstoken { get; set; }
        [Name("tdstmr")]
        public string tdstmr { get; set; }
        [Name("tdserr")]
        public string tdserr { get; set; }
        [Name("tdsenvch")]
        public string tdsenvch { get; set; }
        [Name("tdssql")]
        public string tdssql { get; set; }
        [Name("tdsrpc")]
        public string tdsrpc { get; set; }
        [Name("tdsservname")]
        public string tdsservname { get; set; }
        [Name("smtphelo")]
        public string smtphelo { get; set; }
        [Name("smtpfrom")]
        public string smtpfrom { get; set; }

        [Name("mailuser")]
        public string mailuser { get; set; }
        [Name("mailfailauth")]
        public string mailfailauth { get; set; }
        [Name("mailstarttls")]
        public string mailstarttls { get; set; }
        [Name("hmethod")]
        public string hmethod { get; set; }
        [Name("hrcode")]
        public string hrcode { get; set; }
        [Name("nsx_ruleid")]
        public string nsx_ruleid { get; set; }
        [Name("nsx_vnicindex")]
        public string nsx_vnicindex { get; set; }
        [Name("nsx_vmuuid")]
        public string nsx_vmuuid { get; set; }

        [Name("arp_hrd")]
        public string arp_hrd { get; set; }
        [Name("arp_op")]
        public string arp_op { get; set; }
        [Name("nas")]
        public string nas { get; set; }
        [Name("pas")]
        public string pas { get; set; }
        [Name("mysqlver")]
        public string mysqlver { get; set; }
        [Name("mysqlsver")]
        public string mysqlsver { get; set; }
        [Name("mysqlauths")]
        public string mysqlauths { get; set; }
        [Name("mysqluser")]
        public string mysqluser { get; set; }
        [Name("mysqlauthm")]
        public string mysqlauthm { get; set; }

        [Name("mysqldb")]
        public string mysqldb { get; set; }
        [Name("mysqlcpblts")]
        public string mysqlcpblts { get; set; }
        [Name("mysqlcpbltc")]
        public string mysqlcpbltc { get; set; }
        [Name("mysqlcmd")]
        public string mysqlcmd { get; set; }
        [Name("mysqlerr")]
        public string mysqlerr { get; set; }
        [Name("mysqlsql")]
        public string mysqlsql { get; set; }
        [Name("pgsqlver")]
        public string pgsqlver { get; set; }
        [Name("pgsqlsver")]
        public string pgsqlsver { get; set; }

        [Name("pgsqlauthm")]
        public string pgsqlauthm { get; set; }
        [Name("pgsqluser")]
        public string pgsqluser { get; set; }
        [Name("pgsqldb")]
        public string pgsqldb { get; set; }
        [Name("pgsqlmsgs")]
        public string pgsqlmsgs { get; set; }
        [Name("pgsqlmsgc")]
        public string pgsqlmsgc { get; set; }
        [Name("pgsqlerrs")]
        public string pgsqlerrs { get; set; }
        [Name("pgsqlerrc")]
        public string pgsqlerrc { get; set; }
        [Name("pgsqlsql")]
        public string pgsqlsql { get; set; }

        [Name("tlscont")]
        public string tlscont { get; set; }
        [Name("tlshshk")]
        public string tlshshk { get; set; }
        [Name("tlssetup")]
        public string tlssetup { get; set; }
        [Name("tlssver")]
        public string tlssver { get; set; }
        [Name("tlsciph")]
        public string tlsciph { get; set; }
        [Name("tlssrnd")]
        public string tlssrnd { get; set; }
        [Name("tlsssid")]
        public string tlsssid { get; set; }
        [Name("tlsalpn")]
        public string tlsalpn { get; set; }
        [Name("tlssni")]
        public string TlsServerName { get; set; }

        [Name("tlssnlen")]
        public string tlssnlen { get; set; }
        [Name("tlscver")]
        public string TlsClientVersion { get; set; }
        [Name("tlsciphs")]
        public string tlsciphs { get; set; }
        [Name("tlscrnd")]
        public string tlscrnd { get; set; }
        [Name("tlscsid")]
        public string tlscsid { get; set; }
        [Name("tlsext")]
        public string tlsext { get; set; }
        [Name("tlsexl")]
        public string tlsexl { get; set; }
        [Name("tlsec")]
        public string tlsec { get; set; }
        [Name("tlsecpf")]
        public string tlsecpf { get; set; }

        [Name("tlscklen")]
        public string tlscklen { get; set; }
        [Name("tlsicn")]
        public string tlsicn { get; set; }
        [Name("tlsscn")]
        public string TlsServerCommonName { get; set; }
        [Name("tlsson")]
        public string tlsson { get; set; }
        [Name("tlsvfrom")]
        public string tlsvfrom { get; set; }
        [Name("tlsvto")]
        public string tlsvto { get; set; }
        [Name("tlssalg")]
        public string tlssalg { get; set; }
        [Name("tlspkalg")]
        public string tlspkalg { get; set; }
        [Name("tlspklen")]
        public string tlspklen { get; set; }

        [Name("tlsja3")]
        public string TlsJa3 { get; set; }
        [Name("tlssnum")]
        public string tlssnum { get; set; }
        [Name("tlssan")]
        public string tlssan { get; set; }
        [Name("tlsscm")]
        public string tlsscm { get; set; }
        [Name("iec104pktlen")]
        public string iec104pktlen { get; set; }
        [Name("iec104fmt")]
        public string iec104fmt { get; set; }
        [Name("iec104asdutype")]
        public string iec104asdutype { get; set; }
        [Name("iec104asduobjcount")]
        public string iec104asduobjcount { get; set; }

        [Name("iec104asducot")]
        public string iec104asducot { get; set; }
        [Name("iec104asduorg")]
        public string iec104asduorg { get; set; }
        [Name("iec104asduaddr")]
        public string iec104asduaddr { get; set; }
        [Name("coapver")]
        public string coapver { get; set; }
        [Name("coapmid")]
        public string coapmid { get; set; }
        [Name("coapcode")]
        public string coapcode { get; set; }
        [Name("coapopcount")]
        public string coapopcount { get; set; }
        [Name("coaptype")]
        public string coaptype { get; set; }

        [Name("coapaccept")]
        public string coapaccept { get; set; }
        [Name("coapcontentfmt")]
        public string coapcontentfmt { get; set; }
        [Name("coaptoken")]
        public string coaptoken { get; set; }
        [Name("coapurihost")]
        public string coapurihost { get; set; }
        [Name("coapuripath")]
        public string coapuripath { get; set; }
        [Name("coapuriquery")]
        public string coapuriquery { get; set; }
        [Name("gooseappid")]
        public string gooseappid { get; set; }

        [Name("goosecbref")]
        public string goosecbref { get; set; }
        [Name("goosedataset")]
        public string goosedataset { get; set; }
        [Name("gooseid")]
        public string gooseid { get; set; }
        [Name("goosestnum")]
        public string goosestnum { get; set; }
        [Name("mmstype")]
        public string mmstype { get; set; }
        [Name("mmsconfservicereq")]
        public string mmsconfservicereq { get; set; }
        [Name("mmsconfserviceresp")]
        public string mmsconfserviceresp { get; set; }

        [Name("mmsunconfservice")]
        public string mmsunconfservice { get; set; }
        [Name("dlmstype")]
        public string dlmstype { get; set; }
        [Name("dlmssubtype")]
        public string dlmssubtype { get; set; }
        [Name("dlmsclassid")]
        public string dlmsclassid { get; set; }
        [Name("dlmsobis")]
        public string dlmsobis { get; set; }
        [Name("dlmsattrmethodid")]
        public string dlmsattrmethodid { get; set; }
        [Name("dlmsdatatype")]
        public string dlmsdatatype { get; set; }

        [Name("dlmsdatalength")]
        public string dlmsdatalength { get; set; }
        [Name("dlmsdataaccessresult")]
        public string dlmsdataaccessresult { get; set; }
        [Name("dlmsactionresult")]
        public string dlmsactionresult { get; set; }
        [Name("radiusnataddress")]
        public string radiusnataddress { get; set; }
        [Name("radiusportstart")]
        public string radiusportstart { get; set; }
        [Name("radiusportend")]
        public string radiusportend { get; set; }

        [Name("radiuscalledstationid")]
        public string radiuscalledstationid { get; set; }
        [Name("radiuscallingstationid")]
        public string radiuscallingstationid { get; set; }
        [Name("radiuslogin")]
        public string radiuslogin { get; set; }
        [Name("vxlanvni")]
        public string vxlanvni { get; set; }
        [Name("sn")]
        public string sn { get; set; }
        [Name("dn")]
        public string dn { get; set; }
        [Name("nsa")]
        public string nsa { get; set; }
        [Name("nda")]
        public string nda { get; set; }
        [Name("nsp")]
        public string nsp { get; set; }
        [Name("ndp")]
        public string ndp { get; set; }
    }
}
    


