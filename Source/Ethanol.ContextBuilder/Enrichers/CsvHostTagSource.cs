using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using CsvHelper.Configuration.Attributes;
using Microsoft.Extensions.Logging;
using static Ethanol.ContextBuilder.Enrichers.CsvNetifySource;

namespace Ethanol.ContextBuilder.Enrichers
{
    class CsvHostTagSource
    {
        static ILogger _logger = LogManager.GetCurrentClassLogger();
        internal record CsvHostTagRecord(
            [Index(0)] string KeyType,
            [Index(1)] string KeyValue,
            [Index(2)] string Source,
            [Index(3)] DateTime StartTime,
            [Index(4)] DateTime EndTime,
            [Index(5)] double Reliability,
            [Index(6)] string Module,
            [Index(7)] string Data
        );

        public static IEnumerable<TagRecord> LoadFromFile(string filename)
        {
            using var reader = new StreamReader(File.OpenRead(filename));
            var rows = ParseCsv(reader).Select(row => ConvertToTag(row));
            foreach(var row in rows)
            {
                yield return row;
            }
        }

        private static TagRecord ConvertToTag(CsvHostTagRecord row)
        {
            var record = new TagRecord
            {
                Key = row.KeyValue?.Trim('"') ?? String.Empty,
                Type = row.Source,
                StartTime = row.StartTime, 
                EndTime = row.EndTime,
                Reliability = row.Reliability,
                Value = row.Data
            };
            record.Details = row;
            return record;
        }

        /// <summary>
        /// Parses the input data using custom CSV parser. The custom parser is needed as source CSV is not properly quoted.
        /// </summary>
        /// <param name="reader">The source reader.</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        static IEnumerable<CsvHostTagRecord> ParseCsv(TextReader reader)
        {
            string line;

            while ((line = reader.ReadLine()) != null)
            {
                var parts = line.Split(',', 8);
                // shorter lines are silently ignored...
                if (parts.Length != 8)
                {
                    _logger.LogWarning($"Invalid line: {line}");
                   continue;
                }

                var record = new CsvHostTagRecord(
                    KeyType: parts[0],
                    KeyValue: parts[1],
                    Source: parts[2],
                    StartTime: DateTime.Parse(parts[3]),
                    EndTime: DateTime.Parse(parts[4]),
                    Reliability: double.Parse(parts[5]),
                    Module: parts[6],
                    Data: parts[7]
                );

                yield return record;
            }
        }
    }
}
