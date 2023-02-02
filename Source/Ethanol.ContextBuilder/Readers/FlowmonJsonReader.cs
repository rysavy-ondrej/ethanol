using AutoMapper;
using Ethanol.ContextBuilder.Context;
using Ethanol.ContextBuilder.Plugins.Attributes;
using Ethanol.ContextBuilder.Readers.DataObjects;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading;
using YamlDotNet.Serialization;

namespace Ethanol.ContextBuilder.Readers
{
    /// <summary>
    /// Reads export from flowmonexp5 in JSON format. This format is specific by 
    /// representing each flow as an individual JSON object. It is not NDJSON nor 
    /// properly formatted array of JSON objects.
    /// </summary>
    [Plugin(PluginType.Reader, "FlowmonJson", "Reads JSON file with IPFIX data produced by flowmonexp5 tool.")]
    class FlowmonJsonReader : FlowReader<IpfixObject>
    {
        private readonly TextReader _reader;
        private readonly IMapper mapper;


        public class Configuration
        {
            [YamlMember(Alias ="file", Description ="The file name with JSON data to read.")]
            public string FileName { get; set; }
        }

        /// <summary>
        /// Creates a new reader for the given arguments.
        /// </summary>
        /// <param name="arguments">Collection of arguments used to create a reader.</param>
        /// <returns>A new <see cref="FlowmonJsonReader"/> object.</returns>
        [PluginCreate]
        public static FlowmonJsonReader Create(Configuration configuration)
        {
            var reader = configuration.FileName != null ? File.OpenText(configuration.FileName) : System.Console.In;
            return new FlowmonJsonReader(reader);
        }

        /// <summary>
        /// Initializes the reader with underlying <see cref="TextReader"/>.
        /// </summary>
        /// <param name="reader">The text reader device (input file or standard input).</param>
        public FlowmonJsonReader(TextReader reader)
        {
            _reader = reader;
            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<FlowmonexpEntry, IpfixObject>()
                .ForMember(d => d.Bytes, o => o.MapFrom(s => s.Bytes))
                .ForMember(d => d.DestinationIpAddress, o => o.MapFrom(s => s.L3Ipv4Dst))
                .ForMember(d => d.DestinationPort, o => o.MapFrom(s => s.L4PortDst))
                .ForMember(d => d.DnsQueryName, o => o.MapFrom(s => s.InveaDnsQname.Replace("\0", "")))
                .ForMember(d => d.DnsResponseData, o => o.MapFrom(s => s.InveaDnsCrrRdata.Replace("\0", "")))
                .ForMember(d => d.HttpHost, o => o.MapFrom(s => s.HttpRequestHost.Replace("\0", "")))
                .ForMember(d => d.HttpMethod, o => o.MapFrom(s => s.HttpMethodMask.ToString()))
                .ForMember(d => d.HttpResponse, o => o.MapFrom(s => s.HttpResponseStatusCode.ToString()))
                .ForMember(d => d.HttpUrl, o => o.MapFrom(s => s.HttpRequestUrl.ToString()))
                .ForMember(d => d.Packets, o => o.MapFrom(s => s.Packets))
                .ForMember(d => d.Protocol, o => o.MapFrom(s => (ProtocolType)s.L4Proto))
                .ForMember(d => d.Nbar, o => o.MapFrom(s => s.NbarName))
                .ForMember(d => d.SourceIpAddress, o => o.MapFrom(s => s.L3Ipv4Src))
                .ForMember(d => d.SourceTransportPort, o => o.MapFrom(s => s.L4PortSrc))
                .ForMember(d => d.TimeStart, o => o.MapFrom(s => s.StartNsec))
                .ForMember(d => d.TimeDuration, o => o.MapFrom(s => s.EndNsec - s.StartNsec))
                .ForMember(d => d.TlsVersion, o => o.MapFrom(s => s.TlsClientVersion))
                .ForMember(d => d.TlsJa3, o => o.MapFrom(s => s.TlsJa3Fingerprint))
                .ForMember(d => d.TlsServerCommonName, o => o.MapFrom(s => s.TlsSubjectCn.Replace("\0", "")))
                .ForMember(d => d.TlsServerName, o => o.MapFrom(s => s.TlsSni.Replace("\0", "")));
            });
            mapper = configuration.CreateMapper();
        }

        /// <summary>
        /// Provides next record form the input or null.
        /// </summary>
        /// <param name="ipfixRecord">The record that was read or null.</param>
        /// <returns>true if recrod was read or null for EOF reached.</returns>
        public bool TryReadNextEntry(out IpfixObject ipfixRecord)
        {
            ipfixRecord = null;
            var line = ReadJsonRecord(_reader);
            if (line == null) return false;
            if (FlowmonexpEntry.TryDeserialize(line, out var entry))
            {
                ipfixRecord = mapper.Map<IpfixObject>(entry);
                return true;
            }
            return false;
        }

    /// <summary>
    /// Reads input record from Flowmon's specific JSON.
    /// </summary>
    private string ReadJsonRecord(TextReader inputStream)
        { 
            var buffer = new StringBuilder();
            while (true)
            {
                var line = inputStream.ReadLine();
                buffer.AppendLine(line);
                if (line == null)
                {
                    break;
                }

                if (line.Trim() == "}")
                {
                    break;
                }
            }
            var record = buffer.ToString().Trim();
            if (string.IsNullOrWhiteSpace(record))
            {
                return null;
            }
            else
            {
                return record;
            }
        }

        /// <inheritdoc/>
        protected override void Open()
        {

        }
        /// <inheritdoc/>
        protected override bool TryGetNextRecord(CancellationToken ct, out IpfixObject record)
        {
            return TryReadNextEntry(out record);
        }
        /// <inheritdoc/>
        protected override void Close()
        {
            _reader.Close();
        }
    }
}
