using Ethanol.ContextBuilder.Observable;
using Ethanol.ContextBuilder.Plugins.Attributes;

namespace Ethanol.ContextBuilder.Enrichers
{
    [Plugin(PluginCategory.Enricher, "VoidContextEnricher", "Does not enrich the context. Used to fill the space in the processing pipeline.")]
    public class VoidContextEnricher : IdentityTransformer<object>
    {
        public class Configuration
        {
        }
        [PluginCreate]
        internal static VoidContextEnricher Create(Configuration configuration)
        {
            return new VoidContextEnricher();
        }
    }
}
