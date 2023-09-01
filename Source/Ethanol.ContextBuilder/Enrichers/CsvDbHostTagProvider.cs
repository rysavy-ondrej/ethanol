using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.StreamProcessing;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Ethanol.ContextBuilder.Enrichers
{
    public class CsvDbHostTagProvider : IHostDataProvider<HostTag>
    {
        private Dictionary<string, List<HostTag>> _data;

        public CsvDbHostTagProvider(IEnumerable<HostTag> enumerable)
        {
            _data = BuildDictionary(enumerable);         
        }
        public static Dictionary<string, List<HostTag>> BuildDictionary(IEnumerable<HostTag> hostTags)
        {
            var dict = new Dictionary<string, List<HostTag>>();

            foreach (var tag in hostTags)
            {
                if (!dict.ContainsKey(tag.HostAddress))
                {
                    dict[tag.HostAddress] = new List<HostTag>();
                }

                dict[tag.HostAddress].Add(tag);
            }

            return dict;
        }

        /// <summary>
        /// Enables to load data from CSV instead of JSON file.
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static CsvDbHostTagProvider LoadFromCsv(string filename)
        {
            // Load from CSV here:
            // ip	147.229.176.80	os_by_tcpip	2021-11-09T03:35:58.783846	2021-11-09T03:35:58.783846	1.0	os_by_tcpip@collector-enta	Windows
            var records = CsvHostTag.Load(filename);
            return new CsvDbHostTagProvider(records.Select(x=>new HostTag(x.StartTime,x.EndTime, x.KeyValue, x.SourceValue, x.Reliability, x.Value)));
        }
        public static bool DoIntervalsIntersect(DateTime start1, DateTime end1, DateTime start2, DateTime end2)
        {
            return !(end1 < start2 || end2 < start1);
        }

        public IEnumerable<HostTag> Get(string host, DateTime start, DateTime end)
        {
            var candidates = _data.GetValueOrDefault(host) ?? Enumerable.Empty<HostTag>();
            return candidates.Where(x => DoIntervalsIntersect(start, end, x.StartTime, x.EndTime));
        }

        public Task<IEnumerable<HostTag>> GetAsync(string host, DateTime start, DateTime end)
        {
            return Task.FromResult(Get(host, start, end));
        }
        internal static IHostDataProvider<HostTag> Create(IpHostContextEnricherPlugin.CsvSourceConfiguration config)
        {
            return LoadFromCsv(config.Filename);
        }

        class CsvHostTag
        {
            public string KeyType { get; set; }
            public string KeyValue { get; set; }
            public string SourceValue { get; set; }
            public DateTime StartTime { get; set; }
            public DateTime EndTime { get; set; }
            public double Reliability { get; set; }
            public string SourceModule { get; set; }
            public string Value { get; set; }

            public static IEnumerable<CsvHostTag> Load(string filename)
            {
                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = false, 
                    BadDataFound = null,
                };
                using (var reader = new StreamReader(filename))
                using (var csv = new CsvReader(reader, config))
                {
                    var records = csv.GetRecords<CsvHostTag>();

                    foreach (var record in records)
                    {
                        yield return record;
                    }
                }
            }
        }
    }
}
