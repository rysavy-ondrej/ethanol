using System.Collections.Generic;
using System.Linq;

namespace Ethanol.ContextBuilder.Context
{
    /// <summary>
    /// Provides extension methods for collections of <see cref="TagObject"/> to enable easier filtering based on the properties of the objects.
    /// </summary>
    public static class TagObjectExtensions
    {
        /// <summary>
        /// Filters a collection of <see cref="TagObject"/> based on a specific tag type.
        /// </summary>
        /// <param name="collection">The collection of <see cref="TagObject"/> to filter.</param>
        /// <param name="TagType">The type of the tag by which to filter.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> containing only those <see cref="TagObject"/> with the specified tag type.</returns>
        public static IEnumerable<TagObject> WhereType(this IEnumerable<TagObject> collection, string TagType)
        {
            return collection.Where(t => t.Type == TagType);
        }

        /// <summary>
        /// Filters a collection of <see cref="TagObject"/> based on a specific tag key.
        /// </summary>
        /// <param name="collection">The collection of <see cref="TagObject"/> to filter.</param>
        /// <param name="tagKey">The key of the tag by which to filter.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> containing only those <see cref="TagObject"/> with the specified tag key.</returns>
        public static IEnumerable<TagObject> WhereKey(this IEnumerable<TagObject> collection, string tagKey)
        {
            return collection.Where(t => t.Key == tagKey);
        }

        /// <summary>
        /// Filters a collection of <see cref="TagObject"/> based on both a specific tag type and tag key.
        /// </summary>
        /// <param name="collection">The collection of <see cref="TagObject"/> to filter.</param>
        /// <param name="tagType">The type of the tag by which to filter.</param>
        /// <param name="tagKey">The key of the tag by which to filter.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> containing only those <see cref="TagObject"/> with the specified tag type and tag key.</returns>
        public static IEnumerable<TagObject> WhereKey(this IEnumerable<TagObject> collection, string tagType, string tagKey)
        {
            return collection.Where(t => t.Type == tagType && t.Key == tagKey);
        }
    }

}
