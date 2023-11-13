using Ethanol.ContextBuilder.Observable;
using System.Collections.Generic;

namespace Ethanol.ContextBuilder.Enrichers
{
    /// <summary>
    /// Defines an interface for providing tags based on a given context.
    /// </summary>
    /// <typeparam name="TagType">The type of the tags that are returned by the provider.</typeparam>
    /// <typeparam name="ContextType">The type of the context based on which tags are generated.</typeparam>
    public interface ITagDataProvider<TagType, ContextType>
    {
        /// <summary>
        /// Retrieves a collection of tags based on the provided context.
        /// </summary>
        /// <param name="value">The event containing the context information used for generating tags.</param>
        /// <returns>An enumerable collection of tags of type <typeparamref name="TagType"/>.</returns>
        IEnumerable<TagType> GetTags(ObservableEvent<ContextType> value);
    }

}
