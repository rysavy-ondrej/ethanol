namespace Ethanol.DataObjects
{
    /// <summary>
    /// Represents the DNS response codes indicating the status of a DNS query.
    /// </summary>
    /// <remarks>
    /// These response codes are part of the DNS protocol and convey the status of a DNS query from the server to the client.
    /// </remarks>
    public enum DnsResponseCode
    {
        /// <summary>
        /// No error condition. The DNS query was processed successfully.
        /// </summary>
        NoError = 0,

        /// <summary>
        /// Format error. The name server was unable to interpret the query.
        /// </summary>
        FormErr = 1,

        /// <summary>
        /// Server failure. The name server was unable to process the query due to a problem on the server's side.
        /// </summary>
        ServFail = 2,

        /// <summary>
        /// Non-existent domain. The domain name referenced in the query does not exist.
        /// </summary>
        NXDomain = 3,

        /// <summary>
        /// Not implemented. The server does not support the requested query type.
        /// </summary>
        NotImp = 4,

        /// <summary>
        /// Query refused. The server refused to process the query for some policy reason.
        /// </summary>
        Refused = 5,

        /// <summary>
        /// Name exists when it should not. This indicates a conflict with a pre-existing condition, typically a name that already exists.
        /// </summary>
        YXDomain = 6,

        /// <summary>
        /// Resource record set exists when it should not. This indicates a conflict with a pre-existing set of resource records.
        /// </summary>
        YXRRSet = 7,

        /// <summary>
        /// Resource record set that should exist does not. Indicates that a required resource record set is missing.
        /// </summary>
        NXRRSet = 8,

        /// <summary>
        /// Server not authoritative for the zone or domain. The server received a query for a domain for which it is not authoritative.
        /// </summary>
        NotAuth = 9,

        /// <summary>
        /// Name not contained in zone. Indicates that the name is outside the scope of the zone specified in the query.
        /// </summary>
        NotZone = 10,

        /// <summary>
        /// Bad OPT version. Indicates a mismatch between the DNS protocol versions used by the server and the client.
        /// </summary>
        BadVers = 16
    }
}
