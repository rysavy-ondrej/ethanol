using System;

namespace Ethanol.ContextBuilder.Enrichers.TagObjects
{
    /// <summary>
    /// Represents a record detailing specific information about a web application. 
    /// This can include attributes such as the application's tag, name, description, 
    /// web address, and its associated category.
    /// </summary>
    [Serializable]
    public record NetifyTag
    {
        /// <summary>
        /// Gets or initializes the tag or label uniquely associated with the web application.
        /// This is often a concise identifier used for quick referencing.
        /// </summary>
        public string? Tag { get; init; }

        /// <summary>
        /// Gets or initializes a short, typically abbreviated, name for the web application.
        /// This name is useful for places where space might be limited or for quick referencing.
        /// </summary>
        public string? ShortName { get; init; }

        /// <summary>
        /// Gets or initializes the full, formal name or title of the web application.
        /// This provides a clear understanding of the application's branding or naming.
        /// </summary>
        public string? FullName { get; init; }

        /// <summary>
        /// Gets or initializes the category or type to which the web application belongs.
        /// Categories help in classifying applications based on their primary function or 
        /// the industry they cater to, e.g., "Finance", "Gaming", "Productivity", etc.
        /// </summary>
        public string? Category { get; init; }
    }
}
