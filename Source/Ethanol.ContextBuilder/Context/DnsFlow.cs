namespace Ethanol.ContextBuilder.Context
{
    public enum DnsRecordType { None = 0, A = 1, AAAA = 28, MX = 15, CNAME = 5, NS = 2, SOA = 6, PTR = 12, TXT = 16 }
    public enum DnsClass { None = 0, Internet = 1, Chaos = 3, Hesoid = 4 }

    public enum DnsResponseCode
    {
        // No error condition
        NoError = 0,

        // Format error - The name server was unable to interpret the query
        FormErr = 1,

        // Server failure - The name server was unable to process the query due to a problem on the server's side
        ServFail = 2,

        // Non-existent domain - The domain name in the query does not exist
        NXDomain = 3,

        // Not implemented - The server does not support the requested query type
        NotImp = 4,

        // Query refused - The server refused to process the query for some reason
        Refused = 5,

        // Name exists when it should not - The domain name in the query is too long
        YXDomain = 6,

        // Resource record set exists when it should not - The resource record set in the query is too long
        YXRRSet = 7,

        // Resource record set that should exist does not - The resource record set in the query does not exist
        NXRRSet = 8,

        // Server not authoritative for zone or domain - The server is not authoritative for the domain in the query
        NotAuth = 9,

        // Name not contained in zone - The name is not contained within the zone specified in the query
        NotZone = 10,

        // Bad OPT version - The version of the DNS protocol used by the server is not supported by the client
        BadVers = 16
    }

    /// <summary>
    ///  DNS OPCODE identifies the type of DNS query or DNS update operation that is being performed in the message.
    /// </summary>
    public enum DnsOpCode
    {
        Query = 0,
        IQuery = 1,
        Status = 2,
        Notify = 4,
        Update = 5
    }
    /// <summary>
    /// The DNS query response flag, also known as the QR bit, is a single bit field in the DNS header that indicates whether a DNS message is a query or a response.
    /// Specifically, the QR bit is set to 0 in a DNS query message and 1 in a DNS response message.
    /// </summary>
    public enum DnsQueryResponseFlag
    {
        Query = 0,
        Response = 1
    }

 
    public class DnsFlow : IpFlow
    {
        public string Identifier { get; set; }
        public int QuestionCount { get; set; }
        public int AnswerCount { get; set; }
        public int AuthorityCount { get; set; }
        public int AdditionalCount { get; set; }
        public DnsRecordType ResponseType { get; set; }
        public DnsClass ResponseClass { get; set; }
        public int ResponseTTL { get; set; }
        public string ResponseName { get; set; }
        public DnsResponseCode ResponseCode { get; set; }
        public string ResponseData { get; set; }
        public DnsRecordType QuestionType { get; set; }
        public DnsClass QuestionClass { get; set; }
        public string QuestionName { get; set; }
        public string Flags { get; set; }
        public DnsOpCode Opcode { get; set; }
        public string QueryResponseFlag { get; set; }
    }
}
