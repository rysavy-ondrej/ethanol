using Ethanol.ContextBuilder.Context;
using JsonFlatFileDataStore;
using Microsoft.StreamProcessing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ethanol.ContextBuilder.Enrichers
{
    /// <summary>
    /// Provides tag data retrieval capabilities from a JSON-based database.
    /// This class implements the <see cref="ITagDataProvider{T}"/> interface and allows
    /// querying of tag data based on specified criteria.
    /// </summary>
    public class JsonDbTagProvider : ITagDataProvider<TagObject>
    {
        private DataStore _store;
        private readonly IDocumentCollection<TagObject> _collection;
        private readonly IEnumerable<TagObject> _queryable;

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonDbTagProvider"/> class.
        /// </summary>
        /// <param name="jsonFile">Path to the JSON database file.</param>
        /// <param name="collectionName">The name of the collection in the JSON database where the tags are stored.</param>
        public JsonDbTagProvider(string jsonFile, string collectionName)
        {
            // Open database (create new if file doesn't exist)
            _store = new DataStore(jsonFile);

            // Get tag collection from database
            _collection = _store.GetCollection<TagObject>(collectionName);
            _queryable = _collection.AsQueryable();
        }

        /// <summary>
        /// Retrieves tags that match the given key and fall within the specified time range.
        /// </summary>
        /// <param name="tagKey">The key associated with the tag to be retrieved.</param>
        /// <param name="start">The starting date of the range.</param>
        /// <param name="end">The ending date of the range.</param>
        /// <returns>A collection of tags that match the specified criteria.</returns>
        public IEnumerable<TagObject> Get(string tagKey, DateTime start, DateTime end)
        {
            return _queryable.Where(x => x.Key.ToString() == tagKey && x.StartTime <= start && x.EndTime >= end);
        }

        /// <summary>
        /// Asynchronously retrieves tags that match the given key and fall within the specified time range.
        /// </summary>
        /// <param name="host">The key associated with the tag to be retrieved.</param>
        /// <param name="start">The starting date of the range.</param>
        /// <param name="end">The ending date of the range.</param>
        /// <returns>A task representing the asynchronous operation, which upon completion returns a collection of tags that match the specified criteria.</returns>
        public Task<IEnumerable<TagObject>> GetAsync(string host, DateTime start, DateTime end)
        {
            return Task.FromResult(Get(host, start, end));
        }

        /// <summary>
        /// Creates a new instance of the <see cref="JsonDbTagProvider"/> based on the given JSON configuration.
        /// </summary>
        /// <param name="json">The JSON configuration that contains information for creating the provider.</param>
        /// <returns>A new instance of the <see cref="JsonDbTagProvider"/>.</returns>
        internal static ITagDataProvider<TagObject> Create(EnricherConfiguration.JsonConfiguration json)
        {
            return JsonDbTagProvider.LoadFromJson(json.Filename, json.Collection);
        }

        /// <summary>
        /// Loads tag data provider from a specified JSON file and collection.
        /// </summary>
        /// <param name="filename">The path to the JSON database file.</param>
        /// <param name="collection">The name of the collection in the JSON database where the tags are stored.</param>
        /// <returns>A new instance of the <see cref="JsonDbTagProvider"/> initialized with the given parameters.</returns>
        private static ITagDataProvider<TagObject> LoadFromJson(string filename, string collection)
        {
            return new JsonDbTagProvider(filename, collection);
        }
    }

}
