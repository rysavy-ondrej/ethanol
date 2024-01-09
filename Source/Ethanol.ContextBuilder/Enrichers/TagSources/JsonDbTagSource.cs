using Ethanol.DataObjects;
using Ethanol.ContextBuilder.Enrichers;
using JsonFlatFileDataStore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


/// <summary>
/// Provides tag data retrieval capabilities from a JSON-based database.
/// This class implements the <see cref="ITagDataSource{T}"/> interface and allows
/// querying of tag data based on specified criteria.
/// </summary>
public class JsonDbTagSource : ITagDataSource<TagObject>
{
    private DataStore _store;
    private readonly IDocumentCollection<TagObject> _collection;
    private readonly IEnumerable<TagObject> _queryable;

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonDbTagSource"/> class.
    /// </summary>
    /// <param name="jsonFile">Path to the JSON database file.</param>
    /// <param name="collectionName">The name of the collection in the JSON database where the tags are stored.</param>
    public JsonDbTagSource(string jsonFile, string collectionName)
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
        return _queryable.Where(x => tagKey.Equals(x.Key?.ToString()) && x.StartTime <= start && x.EndTime >= end);
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
    /// Loads tag data provider from a specified JSON file and collection.
    /// </summary>
    /// <param name="filename">The path to the JSON database file.</param>
    /// <param name="collection">The name of the collection in the JSON database where the tags are stored.</param>
    /// <returns>A new instance of the <see cref="JsonDbTagSource"/> initialized with the given parameters.</returns>
    private static ITagDataSource<TagObject> LoadFromJson(string filename, string collection)
    {
        return new JsonDbTagSource(filename, collection);
    }

    /// <summary>
    /// Retrieves a collection of <see cref="TagObject"/> that matches the specified tag key, tag type, 
    /// and falls within the given time range.
    /// </summary>
    /// <param name="tagKey">The key or identifier of the tag to be retrieved.</param>
    /// <param name="tagType">The type of the tag to be retrieved.</param>
    /// <param name="start">The starting point of the date range for which tags are to be retrieved.</param>
    /// <param name="end">The ending point of the date range for which tags are to be retrieved.</param>
    /// <returns>A collection of <see cref="TagObject"/> that matches the provided criteria.</returns>
    public IEnumerable<TagObject> Get(string tagKey, string tagType, DateTime start, DateTime end)
    {
        return _queryable.Where(x => tagKey.Equals(x.Key?.ToString()) && x.Type == tagType && x.StartTime <= start && x.EndTime >= end);
    }

    /// <summary>
    /// Asynchronously retrieves a collection of <see cref="TagObject"/> that matches the specified key, tag type,
    /// and falls within the given time range.
    /// </summary>
    /// <param name="key">The key or identifier of the tag to be retrieved.</param>
    /// <param name="tagType">The type of the tag to be retrieved.</param>
    /// <param name="start">The starting point of the date range for which tags are to be retrieved.</param>
    /// <param name="end">The ending point of the date range for which tags are to be retrieved.</param>
    /// <returns>A task that represents the asynchronous operation. The value of the TResult parameter contains 
    /// a collection of <see cref="TagObject"/> that matches the given criteria.</returns>
    public Task<IEnumerable<TagObject>> GetAsync(string key, string tagType, DateTime start, DateTime end)
    {
        return Task.FromResult(Get(key, tagType, start, end));
    }

    public IEnumerable<TagObject> GetMany(IEnumerable<string> keys, string tagType, DateTime start, DateTime end)
    {
        throw new NotImplementedException();
    }
}
