using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;
using CsvHelper.TypeConversion;
using Ethanol.ContextBuilder.Enrichers.TagObjects;
using Npgsql;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Ethanol.ContextBuilder.Enrichers.TagSources
{
    /// <summary>
    /// Represents a source for fetching Netify tags from a CSV format.
    /// </summary>
    public class CsvNetifyTagSource
    {
        /// <summary>
        /// Represents the schema of a Netify application record within the CSV file.
        /// </summary>
        public class NetifyAppRecord
        {
            /// <summary>
            /// Gets or sets the unique identifier for the application.
            /// </summary>
            [Name("id")]
            public int Id { get; set; }

            /// <summary>
            /// Gets or sets the tag of the application.
            /// </summary>
            [Name("tag")]
            public string Tag { get; set; }

            /// <summary>
            /// Gets or sets the short name of the application.
            /// </summary>
            [Name("short_name")]
            public string ShortName { get; set; }

            /// <summary>
            /// Gets or sets the full name of the application.
            /// </summary>
            [Name("full_name")]
            public string FullName { get; set; }

            /// <summary>
            /// Gets or sets the description of the application.
            /// </summary>
            [Name("description")]
            public string Description { get; set; }

            /// <summary>
            /// Gets or sets the URL associated with the application.
            /// </summary>
            [Name("url")]
            public string Url { get; set; }

            /// <summary>
            /// Gets or sets the category of the application.
            /// </summary>
            [Name("category")]
            public string Category { get; set; }
        }
        /// <summary>
        /// Converts the given <paramref name="record"/> into a Netify tag.
        /// </summary>
        /// <param name="record">The Netify application record to convert.</param>
        /// <returns>The converted Netify tag.</returns>
        public static NetifyTag ConvertToTag(NetifyAppRecord record)
        {
            return new NetifyTag
            {
                Tag = record.Tag,
                ShortName = record.ShortName,
                FullName = record.FullName,
                Description = record.Description,
                Url = record.Url,
                Category = record.Category
            };
        }
        /// <summary>
        /// Loads application records from a CSV file and maps them to a dictionary by their IDs.
        /// </summary>
        /// <param name="appsFile">The path to the CSV file containing the application records.</param>
        /// <returns>A dictionary mapping application IDs to their corresponding records.</returns>
        public static IDictionary<int, NetifyAppRecord> LoadApplicationsFromFile(string appsFile)
        {
            // Load from Apps
            var reader = new StreamReader(appsFile);
            var config = new CsvConfiguration(System.Globalization.CultureInfo.InvariantCulture) { Delimiter = ";", BadDataFound = null };
            using var csv = new CsvReader(reader, config);
            return csv.GetRecords<NetifyAppRecord>().ToDictionary(x => x.Id);
        }

        /// <summary>
        /// Represents a record for Netify IP addresses within a CSV file.
        /// </summary>
        public class NetifyIpsRecord
        {
            /// <summary>
            /// Gets or sets the unique identifier for the IP record.
            /// </summary>
            [Name("id")]
            public int Id { get; set; }

            /// <summary>
            /// Gets or sets the IP address value.
            /// </summary>
            [Name("value")]
            public string Value { get; set; }

            /// <summary>
            /// Gets or sets the version of the IP (e.g., IPv4, IPv6).
            /// </summary>
            [Name("ip_version")]
            public string IpVersion { get; set; }

            /// <summary>
            /// Gets or sets the shared status of the IP. This is converted from potential empty or "none" string values to integers.
            /// </summary>
            [Name("shared")]
            [TypeConverter(typeof(EmptyStringToIntConverter))]
            public int Shared { get; set; }

            /// <summary>
            /// Gets or sets the application ID associated with the IP.
            /// </summary>
            [Name("app_id")]
            public int AppId { get; set; }

            /// <summary>
            /// Gets or sets the platform ID associated with the IP.
            /// </summary>
            [Name("platform_id")]
            public string PlatformId { get; set; }

            /// <summary>
            /// Gets or sets the ASN (Autonomous System Number) tag.
            /// </summary>
            [Name("asn_tag")]
            public string AsnTag { get; set; }

            /// <summary>
            /// Gets or sets the label for the ASN.
            /// </summary>
            [Name("asn_label")]
            public string AsnLabel { get; set; }
        }

        /// <summary>
        /// Loads Netify IP address records from a given CSV file.
        /// </summary>
        /// <param name="ipsFile">The path to the CSV file containing the IP address records.</param>
        /// <returns>An enumerable of Netify IP address records.</returns>
        public static IEnumerable<NetifyIpsRecord> LoadAddressesFromFile(string ipsFile)
        {
            // Load from Apps
            using (var reader = new StreamReader(ipsFile))
            {
                var config = new CsvConfiguration(System.Globalization.CultureInfo.InvariantCulture) { Delimiter = ";", BadDataFound = null };
                var csv = new CsvReader(reader, config);
                var ips = csv.GetRecords<NetifyIpsRecord>();
                foreach (var ip in ips) { yield return ip; }
            }
        }

        /// <summary>
        /// Represents a record for Netify domains within a CSV file.
        /// </summary>
        public class NetifyDomainRecord
        {
            /// <summary>
            /// Gets or sets the unique identifier for the domain record.
            /// </summary>
            [Name("id")]
            public int Id { get; set; }

            /// <summary>
            /// Gets or sets the domain value.
            /// </summary>
            [Name("value")]
            public string Value { get; set; }

            /// <summary>
            /// Gets or sets the application ID associated with the domain.
            /// </summary>
            [Name("app_id")]
            public int AppId { get; set; }

            /// <summary>
            /// Gets or sets the platform ID associated with the domain.
            /// </summary>
            [Name("platform_id")]
            public string PlatformId { get; set; }
        }

        /// <summary>
        /// Loads Netify domain records from a given CSV file.
        /// </summary>
        /// <param name="inputFile">The path to the CSV file containing the domain records.</param>
        /// <returns>An enumerable of Netify domain records.</returns>
        public static IEnumerable<NetifyDomainRecord> LoadDomainsFromFile(string inputFile)
        {
            // Load from file
            using (var reader = new StreamReader(inputFile))
            {
                var config = new CsvConfiguration(System.Globalization.CultureInfo.InvariantCulture) { Delimiter = ";", BadDataFound = null };
                var csv = new CsvReader(reader, config);
                var records = csv.GetRecords<NetifyDomainRecord>();
                foreach (var record in records) { yield return record; }
            }
        }

    }
}
