namespace Ethanol.DataObjects
{
    /// <summary>
    /// Represents the HTTP flow, derived from the general IP flow.
    /// </summary>
    public class HttpFlow : IpFlow
    {
        /// <summary>
        /// Gets or sets the URL associated with the HTTP request or response.
        /// </summary>
        public string? Url { get; set; }

        /// <summary>
        /// Gets or sets the hostname of the server to which the request is addressed.
        /// </summary>
        public string? Hostname { get; set; }

        /// <summary>
        /// Gets or sets the result code of the HTTP response, typically indicating the status of the request.
        /// </summary>
        public string? ResultCode { get; set; }

        /// <summary>
        /// Gets or sets the HTTP method (e.g., GET, POST) used in the request.
        /// </summary>
        public string? Method { get; set; }

        /// <summary>
        /// Gets or sets information about the operating system from which the request originated, usually extracted from the User-Agent header.
        /// </summary>
        public string? OperatingSystem { get; set; }

        /// <summary>
        /// Gets or sets additional information related to the application initiating or handling the HTTP request or response.
        /// </summary>
        public string? ApplicationInformation { get; set; }
    }
}
