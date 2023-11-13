namespace Ethanol.ContextBuilder.Context
{
    /// <summary>
    /// Represents the class field in the DNS protocol.
    /// </summary>
    /// <remarks>
    /// The values in this enumeration correspond to the standard DNS class codes.
    /// </remarks>
    public enum DnsClass
    {
        /// <summary>
        /// Indicates an unspecified or unknown class.
        /// </summary>
        None = 0,

        /// <summary>
        /// Specifies the Internet system.
        /// </summary>
        Internet = 1,

        /// <summary>
        /// Specifies the Chaos system.
        /// </summary>
        Chaos = 3,

        /// <summary>
        /// Specifies the Hesoid system.
        /// </summary>
        Hesoid = 4
    }
}
