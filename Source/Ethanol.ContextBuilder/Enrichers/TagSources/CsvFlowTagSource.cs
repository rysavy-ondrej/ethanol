using CsvHelper;
using CsvHelper.Configuration;
using Ethanol.ContextBuilder.Context;
using Ethanol.ContextBuilder.Enrichers.TagObjects;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.IO;

namespace Ethanol.ContextBuilder.Enrichers.TagSources
{
    /// <summary>
    /// Provides functionality to load flow tags from a CSV file.
    /// </summary>
    public class CsvFlowTagSource
    {
        /// <summary>
        /// Loads flow tags from the specified CSV file.
        /// </summary>
        /// <param name="filename">The name of the CSV file containing flow tags.</param>
        /// <returns>An enumerable of <see cref="TagObject"/> instances representing the flow tags loaded from the file.</returns>
        public static IEnumerable<TagObject> LoadFromFile(string filename)
        {
            var reader = new StreamReader(filename);
            var config = new CsvConfiguration(System.Globalization.CultureInfo.InvariantCulture) { Delimiter = ",", BadDataFound = null, MissingFieldFound = null };
            using var csv = new CsvReader(reader, config);
            var records = csv.GetRecords<TcpFlowTag>();
            foreach (var record in records)
            {
                yield return ConvertToTag(record);
            }
        }

        /// <summary>
        /// Converts a <see cref="TcpFlowTag"/> instance to a <see cref="TagObject"/> instance.
        /// </summary>
        /// <param name="row">The <see cref="TcpFlowTag"/> instance to convert.</param>
        /// <returns>A <see cref="TagObject"/> representation of the provided flow tag.</returns>
        private static TagObject ConvertToTag(TcpFlowTag row)
        {
            var record = new TagObject
            {
                Key = $"Tcp@{row.LocalAddress}:{row.LocalPort}-{row.RemoteAddress}:{row.RemotePort}",
                Type = nameof(TcpFlowTag),
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
