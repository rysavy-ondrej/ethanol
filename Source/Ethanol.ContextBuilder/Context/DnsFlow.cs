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

    /// <summary>
    /// Represents the DNS flow, derived from the general IP flow.
    /// </summary>
    public class DnsFlow : IpFlow
    {
        /// <summary>
        /// Represents a unique identifier associated with the DNS request or response.
        /// </summary>
        public string Identifier { get; set; }

        /// <summary>
        /// Gets or sets the count of questions in the DNS request or response.
        /// </summary>
        public int QuestionCount { get; set; }

        /// <summary>
        /// Gets or sets the count of answers provided in the DNS response.
        /// </summary>
        public int AnswerCount { get; set; }

        /// <summary>
        /// Gets or sets the count of authority resource records in the DNS response.
        /// </summary>
        public int AuthorityCount { get; set; }

        /// <summary>
        /// Gets or sets the count of additional resource records in the DNS response.
        /// </summary>
        public int AdditionalCount { get; set; }

        /// <summary>
        /// Represents the type of the DNS response record.
        /// </summary>
        public DnsRecordType ResponseType { get; set; }

        /// <summary>
        /// Represents the class of the DNS response record.
        /// </summary>
        public DnsClass ResponseClass { get; set; }

        /// <summary>
        /// Represents the time-to-live (TTL) value of the DNS response, indicating how long the record should be cached.
        /// </summary>
        public int ResponseTTL { get; set; }

        /// <summary>
        /// Represents the domain name associated with the DNS response.
        /// </summary>
        public string ResponseName { get; set; }

        /// <summary>
        /// Represents the response code of the DNS query, indicating the success or failure of the query.
        /// </summary>
        public DnsResponseCode ResponseCode { get; set; }

        /// <summary>
        /// Represents additional data or information associated with the DNS response.
        /// </summary>
        public string ResponseData { get; set; }

        /// <summary>
        /// Represents the type of the DNS question record.
        /// </summary>
        public DnsRecordType QuestionType { get; set; }

        /// <summary>
        /// Represents the class of the DNS question record.
        /// </summary>
        public DnsClass QuestionClass { get; set; }

        /// <summary>
        /// Represents the domain name associated with the DNS question.
        /// </summary>
        public string QuestionName { get; set; }

        /// <summary>
        /// Represents flags associated with the DNS query or response, providing additional metadata or control information.
        /// </summary>
        public string Flags { get; set; }

        /// <summary>
        /// Represents the operation code (opcode) of the DNS message, indicating the kind of query or operation being requested.
        /// </summary>
        public DnsOpCode Opcode { get; set; }

        /// <summary>
        /// Represents a flag indicating whether the DNS message is a query or a response.
        /// </summary>
        public DnsQueryResponseFlag QueryResponseFlag { get; set; }
    }
}
