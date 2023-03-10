using JsonFlatFileDataStore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ethanol.ContextBuilder.Enrichers
{
    public class JsonDbHostTagProvider : IHostDataProvider<HostTag>
    {
        private DataStore _store;
        private readonly IDocumentCollection<HostTag> _collection;
        private readonly IEnumerable<HostTag> _queryable;

        public JsonDbHostTagProvider(string jsonFile, string collectionName)
        {
            // Open database (create new if file doesn't exist)
            _store = new DataStore(jsonFile);

            // Get employee collection
            _collection = _store.GetCollection<HostTag>(collectionName);
            _queryable = _collection.AsQueryable();
        }

        public IEnumerable<HostTag> Get(string host, DateTime start, DateTime end)
        {
            return _queryable.Where(x => x.HostAddress.ToString() == host && x.StartTime <= start && x.EndTime >= end);
        }

        public Task<IEnumerable<HostTag>> GetAsync(string host, DateTime start, DateTime end)
        {
            return Task.FromResult(Get(host, start, end));
        }

        internal static IHostDataProvider<HostTag> Create(IpHostContextEnricherPlugin.JsonConfiguration json)
        {
            return new JsonDbHostTagProvider(json.Filename, json.Collection);
        }
    }
}
