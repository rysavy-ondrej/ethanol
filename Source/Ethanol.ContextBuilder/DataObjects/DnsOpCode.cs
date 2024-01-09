namespace Ethanol.DataObjects
{
    /// <summary>
    /// DNS OPCODE identifies the type of DNS query or DNS update operation that is being performed in the message.
    /// </summary>
    /// <remarks>
    /// The DNS OPCODE is a field in the DNS header that communicates the kind of operation the message represents.
    /// </remarks>
    public enum DnsOpCode
    {
        /// <summary>
        /// A standard query. This represents a typical DNS query where a client requests information about a domain.
        /// </summary>
        Query = 0,

        /// <summary>
        /// An inverse query. This is an obsolete query type where a client requests the domain name associated with a given IP address.
        /// </summary>
        IQuery = 1,

        /// <summary>
        /// A status query. The client is checking the status or health of the server without making a specific data request.
        /// </summary>
        Status = 2,

        /// <summary>
        /// A notify operation. The server notifies the secondary servers of changes to a zone.
        /// </summary>
        Notify = 4,

        /// <summary>
        /// An update operation. Used for dynamic updates to add, delete, or modify records in a domain.
        /// </summary>
        Update = 5
    }
}
