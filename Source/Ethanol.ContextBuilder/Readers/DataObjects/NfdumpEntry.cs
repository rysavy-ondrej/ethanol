using CsvHelper.Configuration.Attributes;

namespace Ethanol.ContextBuilder.Readers.DataObjects
{
    /// <summary>
    /// Represents a single flow record as exported from nfdump with "-o csv" option.
    /// <para/>
    /// The raw CSV ouput from nfdump contains many columns most of them are not used in this tool. 
    /// However, this is the easies and possibly the best option to get the output from nfdump version as 
    /// used in Flowmon. 
    /// <para/>
    /// This class is supposed to be used with CsvHelper library. 
    /// </summary>
    internal class NfdumpEntry
    {
        [Name("pr")]
        public string pr { get; set; }

        [Name("sa")]
        public string sa { get; set; }

        [Name("sp")]
        public int sp { get; set; }

        [Name("da")]
        public string da { get; set; }

        [Name("dp")]
        public int dp { get; set; }

        [Name("ts")]
        public string ts { get; set; }

        [Name("td")]
        public double td { get; set; }

        [Name("ipkt")]
        public int ipkt { get; set; }

        [Name("ibyt")]
        public int ibyt { get; set; }

        [Name("opkt")]
        public int opkt { get; set; }

        [Name("obyt")]
        public int obyt { get; set; }

        [Name("apptag")]
        public string apptag { get; set; }

        #region TLS Fields

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
        public string tlssni { get; set; }

        [Name("tlssnlen")]
        public string tlssnlen { get; set; }
        [Name("tlscver")]
        public string tlscver { get; set; }
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
        public string tlsscn { get; set; }
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
        public string tlsja3 { get; set; }
        [Name("tlssnum")]
        public string tlssnum { get; set; }
        [Name("tlssan")]
        public string tlssan { get; set; }
        [Name("tlsscm")]
        public string tlsscm { get; set; }
        #endregion


        #region DNS Fields
        [Name("dnsqname")]
        public string dnsqname { get; set; }
        [Name("dnsrdata")]
        public string dnsrdata { get; set; }
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
        [Name("dnsqtype")]
        public string dnsqtype { get; set; }
        [Name("dnsqclass")]
        public string dnsqclass { get; set; }
        #endregion

        #region HTTP Fields

        [Name("hmethod")]
        public string hmethod { get; set; }
        [Name("hhost")]
        public string hhost { get; set; }
        [Name("hurl")]
        public string hurl { get; set; }
        [Name("hrcode")]
        public int hrcode { get; set; }
        [Name("hos")]
        public string hos { get; set; }
        [Name("happ")]
        public string happ { get; set; }
        #endregion
    }
}



