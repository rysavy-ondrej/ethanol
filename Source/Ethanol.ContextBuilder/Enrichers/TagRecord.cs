using CsvHelper;
using Npgsql;
using NpgsqlTypes;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Security.Cryptography;
using System.Text.Json;
using YamlDotNet.Core.Tokens;

namespace Ethanol.ContextBuilder.Enrichers
{
    /// <summary>
    /// Represents a record for a Tag in the database.
    /// </summary>
    public class TagRecord
    {
        /// <summary>
        /// Gets or sets the type of the tag.
        /// </summary>
        [Column("type")]
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the key associated with the tag.
        /// </summary>
        [Column("key")]
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets the value associated with the tag.
        /// </summary>
        [Column("value")]
        public string Value { get; set; }

        /// <summary>
        /// Gets or sets the reliability factor of the tag.
        /// </summary>
        [Column("reliability")]
        public double Reliability { get; set; }

        /// <summary>
        /// Gets or sets the validity range for the tag.
        /// </summary>
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        /// <summary>
        /// Gets or sets the detailed information for the tag in JSON format.
        /// </summary>
        [Column("details")]
        public dynamic Details { get; set; }

        /// <summary>
        /// Reads a TagRecord from the provided NpgsqlDataReader.
        /// </summary>
        /// <param name="reader">The NpgsqlDataReader containing the tag data.</param>
        /// <returns>The TagRecord extracted from the reader.</returns>
        internal static TagRecord Read(NpgsqlDataReader reader)
        {
            var validity = reader.GetFieldValue<NpgsqlRange<DateTime>>("validity");
            return new TagRecord
            {
                Type = reader.GetString("type"),
                Key = reader.GetString("key"),
                Value = reader.GetString("value"),
                Reliability = reader.GetFloat("reliability"),
                StartTime = validity.LowerBound,
                EndTime =validity.UpperBound,
                Details = JsonSerializer.Deserialize<dynamic>(reader.GetString("details"))
            };
        }

        internal T GetDetails<T>()
        {
            if (typeof(T) == Details.GetType())
            {
                return (T)Details;
            }
            else
            {
                var json = JsonSerializer.Serialize(Details);
                return JsonSerializer.Deserialize<T>(json);
            } 
        }

        /// <summary>
        /// Represents the SQL columns and their types for the TagRecord.
        /// </summary>
        public static (string, string)[] SqlColumns =>
            new (string, string)[]
            {
            ("type", $"VARCHAR({TypeLength}) NOT NULL"),
            ("key", $"VARCHAR({KeyLength}) NOT NULL"),
            ("value", $"VARCHAR({ValueLength})"),
            ("reliability", "REAL"),
            ("validity", "TSRANGE"),
            ("details", "JSON")
            };
        public static int TypeLength = 64;
        public static int KeyLength = 128;
        public static int ValueLength = 512;
    }
}
