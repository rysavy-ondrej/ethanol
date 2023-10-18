namespace Ethanol.ContextBuilder.Plugins.Attributes
{
    /// <summary>
    /// Defines the various categories a plugin can belong to within the system.
    /// </summary>
    public enum PluginCategory
    {
        /// <summary>
        /// Represents a plugin responsible for reading data or content.
        /// </summary>
        Reader,

        /// <summary>
        /// Represents a plugin responsible for writing or outputting data.
        /// </summary>
        Writer,

        /// <summary>
        /// Represents a plugin responsible for constructing context data.
        /// </summary>
        Builder,

        /// <summary>
        /// Represents a plugin that adds additional details or information to an existing data or entity.
        /// </summary>
        Enricher
    }
}
