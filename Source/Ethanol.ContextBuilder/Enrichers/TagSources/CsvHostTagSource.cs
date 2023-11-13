using CsvHelper.Configuration.Attributes;
using Ethanol.ContextBuilder.Context;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Ethanol.ContextBuilder.Enrichers.TagSources
{
    /// <summary>
    /// Provides functionality to load host tags from a CSV file.
    /// </summary>
    public class CsvHostTagSource
    {
        /// <summary>
        /// Represents a single record in the host tags CSV file.
        /// </summary>
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

        /// <summary>
        /// Loads host tags from the specified CSV file.
        /// </summary>
        /// <param name="filename">The name of the CSV file containing host tags.</param>
        /// <returns>An enumerable of <see cref="TagObject"/> instances representing the host tags loaded from the file.</returns>
        public static IEnumerable<TagObject> LoadFromFile(string filename)
        {
            using var reader = new StreamReader(File.OpenRead(filename));
            var rows = ParseCsv(reader).Select(row => ConvertToTag(row));
            foreach (var row in rows)
            {
                yield return row;
            }
        }

        /// <summary>
        /// Converts a <see cref="CsvHostTagRecord"/> instance to a <see cref="TagObject"/> instance.
        /// </summary>
        /// <param name="row">The <see cref="CsvHostTagRecord"/> instance to convert.</param>
        /// <returns>A <see cref="TagObject"/> representation of the provided host tag.</returns>
        private static TagObject ConvertToTag(CsvHostTagRecord row)
        {
            var record = new TagObject
            {
                Key = row.KeyValue?.Trim('"') ?? string.Empty,
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
        /// Parses the input data using a custom CSV parser. The custom parser is utilized because the source CSV might not be properly quoted.
        /// </summary>
        /// <param name="reader">The source reader providing the CSV content.</param>
        /// <returns>An enumerable of <see cref="CsvHostTagRecord"/> instances parsed from the reader.</returns>
        /// <exception cref="Exception">Thrown when the parsing encounters unexpected errors.</exception>
        static IEnumerable<CsvHostTagRecord> ParseCsv(TextReader reader)
        {
            string line;

            while ((line = reader.ReadLine()) != null)
            {
                var parts = line.Split(',', 8);
                // shorter lines are silently ignored...
                if (parts.Length != 8)
                {
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
