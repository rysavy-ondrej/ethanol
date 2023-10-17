using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ethanol.ContextBuilder.Enrichers.TagProviders
{
    /// <summary>
    /// Defines a provider for retrieving <see cref="TagDataType"/> based on specified criteria.
    /// This interface provides methods for synchronous and asynchronous retrieval of tag data.
    /// </summary>
    public interface ITagDataProvider<TagDataType>
    {
        /// <summary>
        /// Retrieves a collection of <see cref="TagDataType"/> that matches the given tag key 
        /// and falls within the specified time range.
        /// </summary>
        /// <param name="tagKey">The key or identifier of the tag to be retrieved.</param>
        /// <param name="start">The starting point of the date range for which tags are to be retrieved.</param>
        /// <param name="end">The ending point of the date range for which tags are to be retrieved.</param>
        /// <returns>A collection of <see cref="TagDataType"/> that matches the given criteria.</returns>
        IEnumerable<TagDataType> Get(string tagKey, DateTime start, DateTime end);

        /// <summary>
        /// Asynchronously retrieves a collection of <see cref="TagDataType"/> that matches the given tag key 
        /// and falls within the specified time range.
        /// </summary>
        /// <param name="key">The key or identifier of the tag to be retrieved.</param>
        /// <param name="start">The starting point of the date range for which tags are to be retrieved.</param>
        /// <param name="end">The ending point of the date range for which tags are to be retrieved.</param>
        /// <returns>A task representing the asynchronous operation, which upon completion 
        /// will return a collection of <see cref="TagDataType"/> that matches the given criteria.</returns>
        Task<IEnumerable<TagDataType>> GetAsync(string key, DateTime start, DateTime end);
    }

}