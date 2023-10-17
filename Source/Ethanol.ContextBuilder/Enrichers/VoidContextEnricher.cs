using Ethanol.ContextBuilder.Observable;
using Ethanol.ContextBuilder.Plugins.Attributes;

namespace Ethanol.ContextBuilder.Enrichers
{
    /// <summary>
    /// Represents a no-op context enricher that does not modify or enrich the incoming data.
    /// This class is intended for use in situations where a placeholder or no-action operation is required in a processing pipeline.
    /// </summary>
    [Plugin(PluginCategory.Enricher, "VoidContextEnricher", "Does not enrich the context. Used to fill the space in the processing pipeline.")]
    public class VoidContextEnricher : IdentityTransformer<object>
    {
        /// <summary>
        /// Represents the configuration for the <see cref="VoidContextEnricher"/> class.
        /// Currently, this class is empty, indicating that no additional configuration is required.
        /// </summary>
        public class Configuration
        {
        }

        /// <summary>
        /// Factory method to create an instance of <see cref="VoidContextEnricher"/> using the provided configuration.
        /// As the enricher does not perform any operations, the configuration is not utilized in the current implementation.
        /// </summary>
        /// <param name="configuration">The configuration for the enricher. Currently not used.</param>
        /// <returns>A new instance of <see cref="VoidContextEnricher"/>.</returns>
        [PluginCreate]
        internal static VoidContextEnricher Create(Configuration configuration)
        {
            return new VoidContextEnricher();
        }
    }
}
