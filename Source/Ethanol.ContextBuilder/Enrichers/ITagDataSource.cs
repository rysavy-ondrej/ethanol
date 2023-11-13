using Ethanol.ContextBuilder.Context;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ethanol.ContextBuilder.Enrichers
{
    /// <summary>
    /// Defines a data source for retrieving <see cref="TagDataType"/> based on specified criteria.
    /// This interface provides methods for synchronous and asynchronous retrieval of tag data.
    /// </summary>
    public interface ITagDataSource<TagDataType>
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
        /// Retrieves a collection of <see cref="TagDataType"/> that matches the given tag key, 
        /// tag type, and falls within the specified time range.
        /// </summary>
        /// <param name="tagKey">The key or identifier of the tag to be retrieved.</param>
        /// <param name="tagType">The type of the tag to be retrieved.</param>
        /// <param name="start">The starting point of the date range for which tags are to be retrieved.</param>
        /// <param name="end">The ending point of the date range for which tags are to be retrieved.</param>
        /// <returns>A collection of <see cref="TagDataType"/> that matches the given criteria.</returns>
        IEnumerable<TagDataType> Get(string tagKey, string tagType, DateTime start, DateTime end);


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

        /// <summary>
        /// Asynchronously retrieves a collection of <see cref="TagDataType"/> that matches the given key,
        /// tag type, and falls within the specified time range.
        /// </summary>
        /// <param name="key">The key or identifier of the tag to be retrieved.</param>
        /// <param name="tagType">The type of the tag to be retrieved.</param>
        /// <param name="start">The starting point of the date range for which tags are to be retrieved.</param>
        /// <param name="end">The ending point of the date range for which tags are to be retrieved.</param>
        /// <returns>A task that represents the asynchronous operation. The value of the TResult parameter contains 
        /// a collection of <see cref="TagDataType"/> that matches the given criteria.</returns>
        Task<IEnumerable<TagDataType>> GetAsync(string key, string tagType, DateTime start, DateTime end);


        /// <summary>
        /// Retrieves a collection of <see cref="TagObject"/> that corresponds to the given remote hosts,
        /// tag type, and falls within the specified time range.
        /// </summary>
        /// <param name="remoteHosts">The collection of IP addresses representing the remote hosts for which the tags are to be retrieved.</param>
        /// <param name="tagType">The type of the tag to be used for filtering.</param>
        /// <param name="start">The starting point of the date range for which tags are to be retrieved.</param>
        /// <param name="end">The ending point of the date range for which tags are to be retrieved.</param>
        /// <returns>A collection of <see cref="TagObject"/> that matches the given criteria.</returns>
        IEnumerable<TagDataType> GetMany(IEnumerable<string> keys, string tagType, DateTime start, DateTime end);

    }
}