using JsonFlatFileDataStore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ethanol.ContextBuilder.Enrichers
{
    public class JsonDbFlowTagProvider : IHostDataProvider<FlowTag>
    {
        private DataStore _store;
        private readonly IDocumentCollection<FlowTag> _collection;
        private readonly IEnumerable<FlowTag> _queryable;

        public JsonDbFlowTagProvider(string jsonFile, string collectionName)
        {
            // Open database (create new if file doesn't exist)
            _store = new DataStore(jsonFile);

            // Get employee collection
            _collection = _store.GetCollection<FlowTag>(collectionName);
            _queryable = _collection.AsQueryable();
        }

        public IEnumerable<FlowTag> Get(string host, DateTime start, DateTime end)
        {
            var result = _queryable.Where(x => x.LocalAddress.ToString() == host && start <= x.EndTime && end >= x.StartTime).ToList();
            return result;
        }

        public Task<IEnumerable<FlowTag>> GetAsync(string host, DateTime start, DateTime end)
        {
            return Task.FromResult(Get(host, start, end));
        }

        internal static IHostDataProvider<FlowTag> Create(IpHostContextEnricherPlugin.JsonConfiguration json)
        {
            return new JsonDbFlowTagProvider(json.Filename, json.Collection);
        }
    }
}
