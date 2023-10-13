using System;
using System.Collections.Generic;
using System.IO;
using CsvHelper.Configuration;
using CsvHelper;
using Microsoft.Extensions.Logging;
using Ethanol.ContextBuilder.Context;

namespace Ethanol.ContextBuilder.Enrichers
{
    class CsvFlowTagSource
    {
        static ILogger _logger = LogManager.GetCurrentClassLogger();

        public static IEnumerable<TagObject> LoadFromFile(string filename)
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

        private static TagObject ConvertToTag(FlowTag row)
        {
            var record = new TagObject
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
