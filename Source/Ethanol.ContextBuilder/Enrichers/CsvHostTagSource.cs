using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using CsvHelper.Configuration;
using CsvHelper;
using CsvHelper.Configuration.Attributes;
using static Ethanol.ContextBuilder.Enrichers.CsvNetifySource;

namespace Ethanol.ContextBuilder.Enrichers
{
    public record FlowTag(
        DateTime StartTime,
        DateTime EndTime,
        string Protocol,
        string LocalAddress,
        int LocalPort,
        string RemoteAddress,
        int RemotePort,
        string ProcessName
    );

    class CsvFlowTagSource
    {
        static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        public static IEnumerable<TagRecord> LoadFromFile(string filename)
        {
            var reader = new StreamReader(filename);
            var config = new CsvConfiguration(System.Globalization.CultureInfo.InvariantCulture) { Delimiter = ",", BadDataFound = null };
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
                Validity = new NpgsqlTypes.NpgsqlRange<DateTime>(row.StartTime, row.EndTime),
                Reliability = 1.0,
                Value = row.ProcessName
            };
            record.SetDetails(row);
            return record;
        }
    }
    class CsvHostTagSource
    {
        static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();
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
                Validity = new NpgsqlTypes.NpgsqlRange<DateTime>(row.StartTime, row.EndTime),
                Reliability = row.Reliability,
                Value = row.Data
            };
            record.SetDetails(row);
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
                    _logger.Warn($"Invalid line: {line}");
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
