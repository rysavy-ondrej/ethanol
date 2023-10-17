namespace Ethanol.ContextBuilder.Context
{
    /// <summary>
    /// The DNS query response flag, also known as the QR bit, is a single bit field in the DNS header that indicates 
    /// whether a DNS message is a query or a response.
    /// Specifically, the QR bit is set to 0 in a DNS query message and 1 in a DNS response message.
    /// </summary>
    /// <remarks>
    /// This flag is essential to distinguish between query and response messages in the DNS protocol, allowing proper parsing and processing.
    /// </remarks>
    public enum DnsQueryResponseFlag
    {
        /// <summary>
        /// Represents a DNS query message. When the QR bit is set to this value, the DNS message is recognized as a query.
        /// </summary>
        Query = 0,

        /// <summary>
        /// Represents a DNS response message. When the QR bit is set to this value, the DNS message is recognized as a response.
        /// </summary>
        Response = 1
    }
}
