using JsonFlatFileDataStore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Ethanol.ContextBuilder.Enrichers
{
    public class JsonDbNetifyTagProvider : IHostDataProvider<NetifyTag>
    {
        static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();
        private DataStore _store;
        private readonly IDocumentCollection<ApsRecord> _appsCollection;
        private readonly IDocumentCollection<IpsRecord> _ipsCollection;
        private readonly IEnumerable<ApsRecord> _appsQueryable;
        private readonly IEnumerable<IpsRecord> _ipsQueryable;

        public JsonDbNetifyTagProvider(string jsonFile, string appsCollectionName, string ipsCollectionName)
        {
            var fileInfo = new FileInfo(jsonFile);
            _logger.Info($"Loading data store from json file: {fileInfo.FullName}, {fileInfo.Length} bytes.");
            // Open database (create new if file doesn't exist)
            _store = new DataStore(jsonFile);

            // Get employee collection
            _appsCollection = _store.GetCollection<ApsRecord>(appsCollectionName);
            _appsQueryable = _appsCollection.AsQueryable();

            _ipsCollection = _store.GetCollection<IpsRecord>(ipsCollectionName);
            _ipsQueryable = _ipsCollection.AsQueryable();
            
            _logger.Info($"Loaded, Apps={_appsCollection.Count} entries, Ips={_ipsCollection.Count} entries.");
        }

        public IEnumerable<NetifyTag> Get(string host, DateTime start, DateTime end)
        {
            return _ipsQueryable.Where(x => x.value == host).Join(_appsQueryable, x => x.app_id, y => y.id, 
                (x, y) => new NetifyTag { Tag = y.tag, ShortName = y.short_name, FullName = y.full_name, Description = y.description, Url = y.url, Category = y.category })
                .Distinct();
        }

        public Task<IEnumerable<NetifyTag>> GetAsync(string host, DateTime start, DateTime end)
        {
            return Task.FromResult(Get(host, start, end));
        }

        internal static IHostDataProvider<NetifyTag> Create(IpHostContextEnricherPlugin.JsonConfiguration json)
        {
            var collections = json.Collection?.Split(',', ';');
            var apsCollection = (collections.Length > 0 && !String.IsNullOrWhiteSpace(collections[0])) ? collections[0] : "aps";
            var ipsCollection = (collections.Length > 1 && !String.IsNullOrWhiteSpace(collections[1])) ? collections[1] : "ips" ;
            return new JsonDbNetifyTagProvider(json.Filename, apsCollection, ipsCollection);
        }
    }

    public record ApsRecord
    {
        public int id { get; set; }
        public string tag { get; set; }
        public string short_name { get; set; }
        public string full_name { get; set; }
        public string description { get; set; }
        public string url { get; set; }
        public string category { get; set; }
    }
    public record IpsRecord
    {
        public int id { get; set; }
        public string value { get; set; }
        public int? shared { get; set; }
        public int app_id { get; set; }
        public string asn_tag { get; set; }
        public string asn_label { get; set; }
        public string asn_route { get; set; }
    }
}
