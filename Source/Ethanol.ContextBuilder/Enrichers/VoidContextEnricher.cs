using Ethanol.ContextBuilder.Observable;

namespace Ethanol.ContextBuilder.Enrichers
{
    /// <summary>
    /// Represents a no-op context enricher that does not modify or enrich the incoming data.
    /// This class is intended for use in situations where a placeholder or no-action operation is required in a processing pipeline.
    /// </summary>

    public class VoidContextEnricher : IdentityTransformer<object>
    {
    }
}
