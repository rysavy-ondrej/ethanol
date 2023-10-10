using Ethanol.ContextBuilder.Context;
using JsonFlatFileDataStore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Threading.Tasks;
using System.Collections;

namespace Ethanol.ContextBuilder.Enrichers
{
    record JsonDbFlowTagRecord(DateTime StartTime, DateTime EndTime, string LocalAddress, ushort LocalPort, string RemoteAddress, ushort RemotePort, string ProcessName);
    public class JsonDbFlowTagProvider : IHostDataProvider<FlowTag>
    {
        private DataStore _store;
        private readonly IDocumentCollection<JsonDbFlowTagRecord> _collection;
        private readonly IEnumerable<JsonDbFlowTagRecord> _queryable;

        public JsonDbFlowTagProvider(string jsonFile, string collectionName)
        {
            _store = new DataStore(jsonFile);

            _collection = _store.GetCollection<JsonDbFlowTagRecord>(collectionName);
            _queryable = _collection.AsQueryable();
        }

        public IEnumerable<FlowTag> Get(string host, DateTime start, DateTime end)
        {
            var result = _queryable.Where(x => x.LocalAddress == host && start <= x.EndTime && end >= x.StartTime).Select(GetFlowTag).ToList();
            return result;
        }

        private FlowTag GetFlowTag(JsonDbFlowTagRecord record)
        {
            return new FlowTag(record.StartTime, record.EndTime,
                                  (TryConvert.ToIPAddress(record.LocalAddress, out var localAddress) ? localAddress : IPAddress.None).ToString(),
                                  TryConvert.ToUInt16(record.LocalPort, out var localPort) ? localPort : (ushort)0,
                                  (TryConvert.ToIPAddress(record.RemoteAddress, out var remoteAddress) ? remoteAddress : IPAddress.None).ToString(),
                                  TryConvert.ToUInt16(record.RemotePort, out var remotePort) ? remotePort : (ushort)0,
                                  record.ProcessName
                                  );
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
