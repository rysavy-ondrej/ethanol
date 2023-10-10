using System;
using System.Collections.Generic;
using System.IO;
using CsvHelper.Configuration;
using CsvHelper;

namespace Ethanol.ContextBuilder.Enrichers
{
    class CsvFlowTagSource
    {
        static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        public static IEnumerable<TagRecord> LoadFromFile(string filename)
        {
            var reader = new StreamReader(filename);
            var config = new CsvConfiguration(System.Globalization.CultureInfo.InvariantCulture) { Delimiter = ",", BadDataFound = null, MissingFieldFound = null };
            using var csv = new CsvReader(reader, config);
            var records = csv.GetRecords<FlowTag>();
            foreach(var  record in records)
            {
                yield return ConvertToTag(record);
            }
        }

        private static TagRecord ConvertToTag(FlowTag row)
        {
            var record = new TagRecord
            {
                Key = $"Tcp@{row.LocalAddress}:{row.LocalPort}-{row.RemoteAddress}:{row.RemotePort}",
                Type = nameof(FlowTag),
                StartTime = row.StartTime, 
                EndTime = row.EndTime,
                Reliability = 1.0,
                Value = row.ProcessName
            };
            record.Details = row;
            return record;
        }
    }
}
