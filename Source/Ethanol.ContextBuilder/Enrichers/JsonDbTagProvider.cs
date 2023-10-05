using JsonFlatFileDataStore;
using Microsoft.StreamProcessing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YamlDotNet.Core;

namespace Ethanol.ContextBuilder.Enrichers
{
    public class JsonDbTagProvider : ITagDataProvider<TagRecord>
    {
        private DataStore _store;
        private readonly IDocumentCollection<TagRecord> _collection;
        private readonly IEnumerable<TagRecord> _queryable;




        public JsonDbTagProvider(string jsonFile, string collectionName)
        {
            // Open database (create new if file doesn't exist)
            _store = new DataStore(jsonFile);

            // Get employee collection
            _collection = _store.GetCollection<TagRecord>(collectionName);
            _queryable = _collection.AsQueryable();
        }

        public IEnumerable<TagRecord> Get(string tagKey, DateTime start, DateTime end)
        {
            return _queryable.Where(x => x.Key.ToString() == tagKey && x.Validity.LowerBound <= start && x.Validity.UpperBound >= end);
        }

        public Task<IEnumerable<TagRecord>> GetAsync(string host, DateTime start, DateTime end)
        {
            return Task.FromResult(Get(host, start, end));
        }

        internal static ITagDataProvider<TagRecord> Create(IpHostContextEnricherPlugin.JsonConfiguration json)
        {
                return JsonDbTagProvider.LoadFromJson(json.Filename, json.Collection);
        }

        private static ITagDataProvider<TagRecord> LoadFromJson(string filename, string collection)
        {
            return new JsonDbTagProvider(filename, collection);
        }
    }
}
