namespace Ethanol.DataObjects
{
    /// <summary>
    /// Represents the type of DNS (Domain Name System) record.
    /// </summary>
    /// <remarks>
    /// The values in this enumeration correspond to the standard DNS record type codes.
    /// </remarks>
    public enum DnsRecordType
    {
        /// <summary>
        /// Indicates an unspecified or unknown record type.
        /// </summary>
        None = 0,

        /// <summary>
        /// Address record, maps a domain name to an IPv4 address.
        /// </summary>
        A = 1,

        /// <summary>
        /// IPv6 address record, maps a domain name to an IPv6 address.
        /// </summary>
        AAAA = 28,

        /// <summary>
        /// Mail exchange record, maps a domain name to a list of mail exchange servers for that domain.
        /// </summary>
        MX = 15,

        /// <summary>
        /// Canonical name record, alias of one name to another.
        /// </summary>
        CNAME = 5,

        /// <summary>
        /// Name server record, specifies that a DNS zone is delegated to a given authoritative server.
        /// </summary>
        NS = 2,

        /// <summary>
        /// Start of Authority record, indicates authoritative information about a DNS zone.
        /// </summary>
        SOA = 6,

        /// <summary>
        /// Pointer record, a domain name pointer. Often used for reverse DNS lookups.
        /// </summary>
        PTR = 12,

        /// <summary>
        /// Text record, typically carries machine-readable data such as SPF, DKIM, etc.
        /// </summary>
        TXT = 16
    }
}
