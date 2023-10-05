using CsvHelper.Configuration;
using CsvHelper;
using Npgsql;
using System;
using System.Collections.Generic;
using System.IO;
using CsvHelper.Configuration.Attributes;
using CsvHelper.TypeConversion;
using System.Linq;

namespace Ethanol.ContextBuilder.Enrichers
{
    public class NetifyCsvSource
    { 
        public class EmptyStringToIntConverter : Int32Converter
        {
            public override object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
            {
                if (string.IsNullOrWhiteSpace(text) || String.Equals(text, "none", StringComparison.InvariantCultureIgnoreCase))
                {
                    return 0;
                }
                return base.ConvertFromString(text, row, memberMapData);
            }
        }


        public class NetifyAppRecord
        {
            [Name("id")]
            public int Id { get; set; }

            [Name("tag")]
            public string Tag { get; set; }

            [Name("short_name")]
            public string ShortName { get; set; }

            [Name("full_name")]
            public string FullName { get; set; }

            [Name("description")]
            public string Description { get; set; }

            [Name("url")]
            public string Url { get; set; }

            [Name("category")]
            public string Category { get; set; }
        }

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


        public static IDictionary<int, NetifyAppRecord> LoadApplicationsFromFile(string appsFile)
            {
                // Load from Apps
                var reader = new StreamReader(appsFile);
                var config = new CsvConfiguration(System.Globalization.CultureInfo.InvariantCulture) { Delimiter = ";", BadDataFound = null };
                using var csv = new CsvReader(reader, config);
                return csv.GetRecords<NetifyAppRecord>().ToDictionary(x=>x.Id);
            }
        
        internal static int ApplicationsBulkInsert(NpgsqlConnection connection, string inputFile, string tableName)
        {
            var recordCount = 0;
            using (var reader = new StreamReader(inputFile))
            {
                var config = new CsvConfiguration(System.Globalization.CultureInfo.InvariantCulture) { Delimiter = ";", BadDataFound = null };
                var csv = new CsvReader(reader, config);
                var appsList = csv.GetRecords<NetifyAppRecord>();

                using (var writer = connection.BeginBinaryImport($"COPY {tableName} (id, tag, short_name, full_name, description, url, category) FROM STDIN (FORMAT BINARY)"))
                {
                    foreach (var app in appsList)
                    {
                        recordCount++;

                        writer.StartRow();
                        writer.Write(app.Id, NpgsqlTypes.NpgsqlDbType.Integer);
                        writer.Write(app.Tag, NpgsqlTypes.NpgsqlDbType.Text);
                        writer.Write(app.ShortName, NpgsqlTypes.NpgsqlDbType.Text);
                        writer.Write(app.FullName, NpgsqlTypes.NpgsqlDbType.Text);
                        writer.Write(app.Description, NpgsqlTypes.NpgsqlDbType.Text);
                        writer.Write(app.Url, NpgsqlTypes.NpgsqlDbType.Text);
                        writer.Write(app.Category, NpgsqlTypes.NpgsqlDbType.Text);
                    }

                    writer.Complete();
                }
            }
            return recordCount;
        }

        public class NetifyIpsRecord
        {
            [Name("id")]
            public int Id { get; set; }

            [Name("value")]
            public string Value { get; set; }

            [Name("ip_version")]
            public string IpVersion { get; set; }

            [Name("shared")]
            [TypeConverter(typeof(EmptyStringToIntConverter))]
            public int Shared { get; set; }

            [Name("app_id")]
            public int AppId { get; set; }

            [Name("platform_id")]
            public string PlatformId { get; set; }

            [Name("asn_tag")]
            public string AsnTag { get; set; }

            [Name("asn_label")]
            public string AsnLabel { get; set; }
        }
            internal static IEnumerable<NetifyIpsRecord> LoadAddressesFromFile(string ipsFile)
            {
                // Load from Apps
                using (var reader = new StreamReader(ipsFile))
                {
                    var config = new CsvConfiguration(System.Globalization.CultureInfo.InvariantCulture) { Delimiter = ";", BadDataFound = null };
                    var csv = new CsvReader(reader, config);
                    var ips = csv.GetRecords<NetifyIpsRecord>();
                    foreach(var  ip in ips) { yield return ip; }
                }
            }
        
        internal static int AddressesBulkInsert(NpgsqlConnection connection, string inputFile, string tableName)
        {
            var recordCount = 0;
            using (var reader = new StreamReader(inputFile))
            {
                var config = new CsvConfiguration(System.Globalization.CultureInfo.InvariantCulture) { Delimiter = ";", BadDataFound = null };
                var csv = new CsvReader(reader, config);
                var appsList = csv.GetRecords<NetifyIpsRecord>();

                using (var writer = connection.BeginBinaryImport($"COPY {tableName}  (id, value, ip_version, shared, app_id, platform_id, asn_tag, asn_label) FROM STDIN (FORMAT BINARY)"))
                {
                    foreach (var app in appsList)
                    {
                        recordCount++;

                        writer.StartRow();
                        writer.Write(app.Id, NpgsqlTypes.NpgsqlDbType.Integer);
                        writer.Write(app.Value, NpgsqlTypes.NpgsqlDbType.Text);
                        writer.Write(app.IpVersion, NpgsqlTypes.NpgsqlDbType.Text);
                        writer.Write(app.Shared, NpgsqlTypes.NpgsqlDbType.Integer);
                        writer.Write(app.AppId, NpgsqlTypes.NpgsqlDbType.Integer);
                        writer.Write(app.PlatformId, NpgsqlTypes.NpgsqlDbType.Text);
                        writer.Write(app.AsnTag, NpgsqlTypes.NpgsqlDbType.Text);
                        writer.Write(app.AsnLabel, NpgsqlTypes.NpgsqlDbType.Text);
                    }

                    writer.Complete();
                }
            }
            return recordCount;
        }


        public class NetifyDomainRecord
        {
            [Name("id")]
            public int Id { get; set; }

            [Name("value")]
            public string Value { get; set; }

            [Name("app_id")]
            public int AppId { get; set; }

            [Name("platform_id")]
            public string PlatformId { get; set; }

        }
        internal static IEnumerable<NetifyDomainRecord> LoadDomainsFromFile(string inputFile)
        {
            // Load from Apps
            using (var reader = new StreamReader(inputFile))
            {
                var config = new CsvConfiguration(System.Globalization.CultureInfo.InvariantCulture) { Delimiter = ";", BadDataFound = null };
                var csv = new CsvReader(reader, config);
                var records = csv.GetRecords<NetifyDomainRecord>();
                foreach (var record in records) { yield return record; }


            }
        }

        public static int DomainsBulkInsert(NpgsqlConnection connection, string inputFile, IDictionary<int, NetifyAppRecord> applications, string tableName)
        {
            var recordCount = 0;
            using (var reader = new StreamReader(inputFile))
            {
                var config = new CsvConfiguration(System.Globalization.CultureInfo.InvariantCulture) { Delimiter = ";", BadDataFound = null };
                var csv = new CsvReader(reader, config);
                var appsList = csv.GetRecords<NetifyDomainRecord>();

                using (var writer = connection.BeginBinaryImport($"COPY {tableName} (id, value, app_id, platform_id) FROM STDIN (FORMAT BINARY)"))
                {
                    foreach (var app in appsList)
                    {
                        recordCount++;

                        writer.StartRow();
                        writer.Write(app.Id, NpgsqlTypes.NpgsqlDbType.Integer);
                        writer.Write(app.Value, NpgsqlTypes.NpgsqlDbType.Text);
                        writer.Write(app.AppId, NpgsqlTypes.NpgsqlDbType.Integer);
                        writer.Write(app.PlatformId, NpgsqlTypes.NpgsqlDbType.Text);
                    }

                    writer.Complete();
                }
            }
            return recordCount;
        }
    }
}
