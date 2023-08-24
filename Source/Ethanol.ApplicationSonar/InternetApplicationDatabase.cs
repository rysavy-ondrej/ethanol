// This is simple:
// Read input JSON, perform searching for IPs, kewords in domain names, etc, and produce the output...
//
// The Netify is provided as input XXX file and represented as in-memory database.
// The app fingeprint is also loaded from XXX file and represented as in-memory database NMemory (???)
// 
using NLog;
using NMemory;
using NMemory.Indexes;
using NMemory.Tables;
using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ethanol.ApplicationSonar
{
    /// <summary>
    /// Represents a database for managing internet application records.
    /// </summary>
    public partial class InternetApplicationDatabase
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private Database _database;
        private Table<InternetApplicationRecord, int> _applicationTable;
        private Table<AddressIndicatorRecord, int> _addressTable;
        private IIndex<AddressIndicatorRecord, string?> _addressTableIndexOnAddress;
        private IIndex<AddressIndicatorRecord, int> _addressTableIndexOnApplication;
        private Relation<InternetApplicationRecord, int, AddressIndicatorRecord, int> _addressToApplicationRelation;

        public InternetApplicationDatabase()
        {
            this._database = new Database();
            this._applicationTable = _database.Tables.Create<InternetApplicationRecord, int>(item => item.Id);
            this._addressTable = _database.Tables.Create<AddressIndicatorRecord, int>(item => item.Id);

            _addressTableIndexOnAddress = _addressTable.CreateIndex(new RedBlackTreeIndexFactory(), p => p.Address);
            _addressTableIndexOnApplication = _addressTable.CreateIndex(new RedBlackTreeIndexFactory(), p => p.ApplicationId);

            //_addressToApplicationRelation = _database.Tables.CreateRelation(_applicationTable.PrimaryKeyIndex, _addressTableIndexOnApplication, x => x, x => x, new RelationOptions());
        }

        /// <summary>
        /// Creates the databse with two tables IPS and APS based on the URI of the provided files.
        /// </summary>
        /// <param name="addressIndicatorFileUri">The URI of the Internet address indicators file.</param>
        /// <param name="applicationIndexFileUri">The URI of the Internet application index file.</param>
        /// <returns>The database providing search capabilities on IP indicators for identification of known Internet applications.</returns>
        /// <exception cref="FileNotFoundException">Raised if either of files cannot be found.</exception>
        internal static async Task<InternetApplicationDatabase> LoadFromAsync(Uri addressIndicatorFileUri, Uri applicationIndexFileUri)
        {
            if (addressIndicatorFileUri is null) throw new ArgumentNullException(nameof(addressIndicatorFileUri));
            if (applicationIndexFileUri is null) throw new ArgumentNullException(nameof(applicationIndexFileUri));
           
            var db = new InternetApplicationDatabase();

            var ipsRecords = await FetchRecordsAsync<AddressIndicatorRecord>(addressIndicatorFileUri);
            await db.LoadIpsAsync(ipsRecords);
            ipsRecords.Dispose();

            var apsRecords = await FetchRecordsAsync<InternetApplicationRecord>(applicationIndexFileUri);
            await db.LoadApsAsync(apsRecords);
            apsRecords.Dispose();

            return db;
        }

        private  async Task LoadApsAsync(IAsyncEnumerable<InternetApplicationRecord?> apsRecords)
        {
            var count = 0;
            await foreach (var item in apsRecords)
            {
                if (item != null)
                {
                    _applicationTable.Insert(item);

                    if (++count % 1000 == 0) _logger.Info($"{count} APS record inserted.");
                }
            }
        }

        private  async Task LoadIpsAsync(IAsyncEnumerable<AddressIndicatorRecord?> ipsRecords)
        {
            var count = 0;
            await foreach (var item in ipsRecords)
            {
                if (item != null)
                {
                    _addressTable.Insert(item);
                    if (++count % 100000 == 0) _logger.Info($"{count} IPS record inserted.");
                }
            }
        }

        
        private static async Task<DisposableAsyncEnumerable<T?>> FetchRecordsAsync<T>(Uri uri, CancellationToken ct = default)
        {
            var jsonOptions = new JsonSerializerOptions();
            jsonOptions.Converters.Add(new IntFromFloatJsonConverter());
            if (uri.Scheme == Uri.UriSchemeFile)
            {
                // Accessing a local file
                string filePath = uri.LocalPath;
                _logger.Info($"Accessing a local file {filePath}");
                var stream = File.OpenRead(filePath);
                return new DisposableAsyncEnumerable<T?>(JsonSerializer.DeserializeAsyncEnumerable<T>(stream, jsonOptions, ct), stream);
            }
            // Check if the URI scheme is "http" or "https"
            else if (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)
            {
                // Accessing an HTTP document
                string url = uri.ToString();
                _logger.Info($"Accessing an HTTP(S) document {url}");
                using var client = new HttpClient();
                var stream = await client.GetStreamAsync(url);
                return new DisposableAsyncEnumerable<T?>(JsonSerializer.DeserializeAsyncEnumerable<T>(stream, jsonOptions, ct), stream, client);
            }
            throw new FileNotFoundException("The file is found at the specified URI.", uri.ToString());
        }

        internal ICollection<InternetApplicationDescription> GetApplications(string remoteAddress, out int shared)
        {
            shared = 0;
            var result = new List<InternetApplicationDescription>();
            var addressRecord = _addressTable.Where(x => x.Address == remoteAddress);
            foreach(var adr in addressRecord) 
            {
                shared = Math.Max(shared, adr.Shared ?? 0);
                var app = _applicationTable.Where(x => x.Id == adr.ApplicationId).FirstOrDefault();
                if (app != null)
                {
                    result.Add(new InternetApplicationDescription { ApplicationAddress = adr.Address, ApplicationTag = app.Tag, ApplicationName = app.ShortName, ApplicationUrl = app.Url, Category = app.Category });
                }
            }
            return result;
        }



        /// <summary>
        /// Gets the count of address records in the database.
        /// </summary>
        public long AddressCount => _addressTable.Count;

        /// <summary>
        /// Gets the count of application records in the database.
        /// </summary>
        public long ApplicationCount => _applicationTable.Count;


        /// <summary>
        /// Represents a single Internet Application Record.
        /// </summary>
        /// <remarks>
        /// The JSON has the following format:
        /// {"id": 48.0, "tag": "app.qq", "short_name": "Tencent QQ", "full_name": "Tencent QQ Instant Messenger", "description": "Tencent QQ, popularly known as QQ, is an instant messaging software service based in China.  Nearly 1 billion active accounts use QQ for chat, gaming, music, shopping and more.", "url": "http://www.qq.com", "category": "Messaging"}
        /// </remarks>
        public record InternetApplicationRecord
        {
            [JsonPropertyName("id")]
            public int Id { get; set; }
            [JsonPropertyName("tag")]
            public string? Tag { get; set; }
            [JsonPropertyName("short_name")]
            public string? ShortName { get; set; }
            [JsonPropertyName("full_name")]
            public string? FullName { get; set; }
            [JsonPropertyName("description")]
            public string? Description { get; set; }
            [JsonPropertyName("url")]
            public string? Url { get; set; }
            [JsonPropertyName("category")]
            public string? Category { get; set; }
        }
        /// <summary>
        /// Represents a single Address Indicator Record.
        /// </summary>
        /// <remarks>
        /// The JSON has the following format:
        /// {"id": 1.0, "value": "1.62.64.112", "ip_version": 4.0, "shared": 90.0, "app_id": 48.0, "platform_id": 11396.0, "asn_tag": "AS4837", "asn_label": "China Unicom", "asn_route": "1.56.0.0/13", "asn_entity_id": 11059.0}
        /// </remarks>
        public record AddressIndicatorRecord
        {
            [JsonPropertyName("id")]
            public int Id { get; set; }
            [JsonPropertyName("value")]
            public string? Address { get; set; }
            [JsonPropertyName("shared")]
            public int? Shared { get; set; }
            [JsonPropertyName("app_id")]
            public int ApplicationId { get; set; }
            [JsonPropertyName("asn_tag")]
            public string? AsnTag { get; set; }
            [JsonPropertyName("asn_label")]
            public string? AsnLabel { get; set; }
            [JsonPropertyName("asn_route")]
            public string? AsnRoute { get; set; }
            [JsonPropertyName("asn_entity_id")]
            public int? AsnId { get; set; }
        }
        /// <summary>
        /// Custom JSON converter for converting JSON float values to integers.
        /// Implements the <see cref="JsonConverter{T}"/> interface for <see cref="int"/> type.
        /// </summary>
        public class IntFromFloatJsonConverter : JsonConverter<int>
        {
            public override int Read(
                ref Utf8JsonReader reader,
                Type typeToConvert,
                JsonSerializerOptions options) =>
                    Convert.ToInt32(reader.GetSingle());

            public override void Write(
                Utf8JsonWriter writer,
                int value,
                JsonSerializerOptions options) =>
                    writer.WriteNumberValue(Convert.ToSingle(value));
        }
    }

    /// <summary>
    /// Represents an internet application description.
    /// </summary>
    public record InternetApplicationDescription
    {
        /// <summary>
        /// Gets or sets the address of the application.
        /// </summary>
        public string ApplicationAddress { get; set; }

        /// <summary>
        /// Gets or sets the tag associated with the application.
        /// </summary>
        public string ApplicationTag { get; set; }

        /// <summary>
        /// Gets or sets the name of the application.
        /// </summary>
        public string ApplicationName { get; set; }

        /// <summary>
        /// Gets or sets the URL of the application.
        /// </summary>
        public string ApplicationUrl { get; set; }

        /// <summary>
        /// Gets or sets the category of the application.
        /// </summary>
        public string Category { get; set; }
    }

}