﻿using AutoMapper;
using CsvHelper;
using Ethanol.ContextBuilder.Attributes;
using Ethanol.ContextBuilder.Context;
using Ethanol.ContextBuilder.Readers.DataObjects;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using YamlDotNet.Core.Tokens;

namespace Ethanol.ContextBuilder.Readers
{
    /// <summary>
    /// Reads export from flowmonexp5 in JSON format. This format is specific by 
    /// representing each flow as an individual JSON object. It is not NDJSON nor 
    /// properly formatted array of JSON objects.
    /// </summary>
    [Module(ModuleType.Reader, "NfdumpCsv")]
    class NfdumpReader : ReaderModule<IpfixObject>
    {
        private readonly CsvReader _csvReader;
        private readonly IMapper mapper;

        /// <summary>
        /// Creates a new reader for the given arguments.
        /// </summary>
        /// <param name="arguments">Collection of arguments used to create a reader.</param>
        /// <returns>A new <see cref="FlowmonJsonReader"/> object.</returns>
        public static NfdumpReader Create(IReadOnlyDictionary<string, string> arguments)
        {
            var reader = arguments.TryGetValue("file", out var inputFile) ? File.OpenText(inputFile) : System.Console.In;
            return new NfdumpReader(reader);
        }

        /// <summary>
        /// Initializes the reader with underlying <see cref="StreamReader"/>.
        /// </summary>
        /// <param name="reader">The text reader device (input file or standard input).</param>
        public NfdumpReader(TextReader reader)
        {
            _csvReader = new CsvReader(reader, CultureInfo.InvariantCulture);
            
        }

        /// <summary>
        /// Provides next record form the input or null.
        /// </summary>
        /// <param name="ipfixRecord">The record that was read or null.</param>
        /// <returns>true if recrod was read or null for EOF reached.</returns>
        public bool TryReadNextEntry(out IpfixObject ipfixRecord)
        {
            ipfixRecord = null;
            if (_csvReader.Read())
            {
                var record = _csvReader.GetRecord<NfdumpEntry>();
                if (record == null) return false;
                ipfixRecord = record.ToIpfix();
                return true;
            }
            return false;
        }

        /// <inheritdoc/>
        protected override void Open()
        {
            _csvReader.Read();
            _csvReader.ReadHeader();
        }
        /// <inheritdoc/>
        protected override bool TryGetNextRecord(CancellationToken ct, out IpfixObject record)
        {
            return TryReadNextEntry(out record);
        }
        /// <inheritdoc/>
        protected override void Close()
        {
            _csvReader.Dispose();
        }
    }
}
