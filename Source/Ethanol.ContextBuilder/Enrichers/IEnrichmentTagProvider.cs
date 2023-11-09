using Ethanol.ContextBuilder.Observable;
using System.Collections.Generic;

namespace Ethanol.ContextBuilder.Enrichers
{
    public interface IEnrichmentTagProvider<TagType, ContextType>
    {
        IEnumerable<TagType> GetTags(ObservableEvent<ContextType> value);
    }
}
