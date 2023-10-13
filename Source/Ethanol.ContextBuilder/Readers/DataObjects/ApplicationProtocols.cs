namespace Ethanol.ContextBuilder.Readers.DataObjects
{
    /// <summary>
    /// Specifies the identified application protocols. 
    /// </summary>
    public enum ApplicationProtocols
    {
        /// <summary>
        /// The DNS request with response.
        /// </summary>
        DNS,
        /// <summary>
        /// The plain HTTP request.
        /// </summary>
        HTTP,
        /// <summary>
        /// The SSL connection with additional handshake information.
        /// </summary>
        SSL,
        /// <summary>
        /// The HTTPS connection without handshake information.
        /// </summary>
        HTTPS,
        /// <summary>
        /// Other application protocols not examined by the tool.
        /// </summary>
        Other
    }
}
