using CsvHelper.Configuration.Attributes;
using System;

namespace Ethanol.ContextBuilder.Readers.DataObjects
{
    /// <summary>
    /// Represents a single host extension record provided by Smart ADS. 
    /// <para/>
    /// It supports reading/writing in CSV, JSON and YAML formats.
    /// </summary>
    internal class HostTagEntry
    {
        /// <summary>
        /// The type of the key
        /// </summary>
        [Index(0)]
        public string KeyType { get; set; }
        [Index(1)]
        public string KeyValue { get; set; }
        [Index(2)]
        public string SourceType { get; set; }
        [Index(3)]
        public DateTime ValidFrom { get; set; }
        [Index(4)]
        public DateTime ValidTo { get; set; }
        [Index(5)]
        public double Reliability { get; set; }
        [Index(6)]
        public string SourceModule { get; set; }
        [Index(7)]
        public string ValueText { get; set; }
    }
}
