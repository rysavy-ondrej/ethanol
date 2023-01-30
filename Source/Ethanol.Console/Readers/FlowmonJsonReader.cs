using AutoMapper;
using Ethanol.ContextBuilder.Context;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Ethanol.ContextBuilder.Readers
{
    public enum JsonFormat { Json, NdJson }
    class FlowmonJsonReader : InputDataReader<IpfixRecord>
    {
        private readonly TextReader _reader;
        private readonly IMapper mapper;
        private readonly Func<string> ReadLine;


        public static FlowmonJsonReader Create(IReadOnlyDictionary<string, string> arguments)
        {
            var reader = arguments.TryGetValue("file", out var inputFile) ? File.OpenText(inputFile) : System.Console.In;
            arguments.TryGetValue("format", out var format);
            return new FlowmonJsonReader(reader);
        }

        public FlowmonJsonReader(TextReader reader)
        {
            _reader = reader;
            ReadLine = () => ReadJsonRecord(reader);
            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<FlowmonexpEntry, IpfixRecord>()
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
                .ForMember(d => d.TlsClientVersion, o => o.MapFrom(s => s.TlsClientVersion))
                .ForMember(d => d.TlsJa3, o => o.MapFrom(s => s.TlsJa3Fingerprint))
                .ForMember(d => d.TlsServerCommonName, o => o.MapFrom(s => s.TlsSubjectCn.Replace("\0", "")))
                .ForMember(d => d.TlsServerName, o => o.MapFrom(s => s.TlsSni.Replace("\0", "")));
            });
            mapper = configuration.CreateMapper();
        }

        public bool TryReadNextEntry(out IpfixRecord ipfixRecord)
        {
            ipfixRecord = null;
            var line = ReadLine();
            if (line == null) return false;
            if (FlowmonexpEntry.TryDeserialize(line, out var entry))
            {
                ipfixRecord = mapper.Map<IpfixRecord>(entry);
                return true;
            }
            return false;
        }
        private string ReadNdJsonRecord(TextReader inputStream)
        {
            var line = inputStream.ReadLine();
            if (string.IsNullOrWhiteSpace(line))
            {
                return null;
            }
            else
            {
                return line;
            }
        }

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

        protected override void Open()
        {

        }

        protected override bool TryGetNextRecord(CancellationToken ct, out IpfixRecord record)
        {
            return TryReadNextEntry(out record);
        }

        protected override void Close()
        {
            _reader.Close();
        }
    }
}
