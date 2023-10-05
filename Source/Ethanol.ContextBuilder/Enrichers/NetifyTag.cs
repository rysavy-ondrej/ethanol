namespace Ethanol.ContextBuilder.Enrichers
{
    /// <summary>
    /// Represents a web application record.
    /// </summary>
    public record NetifyTag
    {
        /// <summary>
        /// The tag or label associated with the application.
        /// </summary>
        public string Tag { get; init; }

        /// <summary>
        /// A short name or abbreviation for the application.
        /// </summary>
        public string ShortName { get; init; }

        /// <summary>
        /// The full name or title of the application.
        /// </summary>
        public string FullName { get; init; }

        /// <summary>
        /// A longer description or summary of the application.
        /// </summary>
        public string Description { get; init; }

        /// <summary>
        /// The URL or web address for the application.
        /// </summary>
        public string Url { get; init; }

        /// <summary>
        /// The category or type of the application.
        /// </summary>
        public string Category { get; init; }
    }
}
