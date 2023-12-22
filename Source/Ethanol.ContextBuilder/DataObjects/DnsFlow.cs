using Ethanol.DataObjects;
namespace Ethanol.ContextBuilder.Context
{
    /// <summary>
    /// Represents the DNS flow, derived from the general IP flow.
    /// </summary>
    public class DnsFlow : IpFlow
    {
        /// <summary>
        /// Represents a unique identifier associated with the DNS request or response.
        /// </summary>
        public string? Identifier { get; set; }

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
        public long ResponseTTL { get; set; }

        /// <summary>
        /// Represents the domain name associated with the DNS response.
        /// </summary>
        public string? ResponseName { get; set; }

        /// <summary>
        /// Represents the response code of the DNS query, indicating the success or failure of the query.
        /// </summary>
        public DnsResponseCode ResponseCode { get; set; }

        /// <summary>
        /// Represents additional data or information associated with the DNS response.
        /// </summary>
        public string? ResponseData { get; set; }

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
        public string? QuestionName { get; set; }

        /// <summary>
        /// Represents flags associated with the DNS query or response, providing additional metadata or control information.
        /// </summary>
        public string? Flags { get; set; }

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
