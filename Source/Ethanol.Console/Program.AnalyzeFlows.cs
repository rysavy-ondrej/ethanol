using AutoMapper;
using Ethanol.Streaming;
using Microsoft.StreamProcessing;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;

namespace Ethanol.Console
{

    partial class Program
    {
        /// <summary>
        /// Compute the context in the collection of <paramref name="sourceFlowFiles"/> and writes it to the dump files.
        /// </summary>
        /// <param name="sourceFlowFiles">The observable collection of source files.</param>
        /// <param name="configuration">The configuration.</param>
        /// <returns>Task that completes when the processing is done.</returns>
        Task BuildContextFromFiles(IObservable<FileInfo> sourceFlowFiles, IObservable<FileInfo> sourceDumpFiles,DataFileFormat inputFormat, DataFileFormat outputFormat)
        {
            var sw = new Stopwatch();
            sw.Start();
            var totalOutputFlows = 0;
            var ethanol = new EthanolEnvironment();
            var entryObserver = new IpfixObservableStream(TimeSpan.FromMinutes(15), TimeSpan.FromMinutes(5));
            var socketObserver = new SocketObservableStream(TimeSpan.FromMinutes(15), TimeSpan.FromMinutes(5));
            /*
            var ipfixStream = entryObserver.Join(socketObserver,
                left => left.FlowKey,
                right => right.FlowKey,
                (left, right) => UpdateWith(left, right.ProcessName)); 
            */
            var ipfixStream = entryObserver.EnrichFrom(socketObserver,
                left => left.FlowKey,
                right => right.FlowKey,
                (left, right) => UpdateWith(left, right));

            var exitStream = ethanol.ContextBuilder.BuildTlsContext(ipfixStream);
            var exitObservable = new ObservableEgressStream<ContextFlow<TlsContext>>(exitStream);
            var consumerTask = exitObservable.ForEachAsync(obj =>
            {
                totalOutputFlows++;
                if (outputFormat == DataFileFormat.Yaml)
                {
                    PrintStreamEventYaml(obj.Payload.FlowKey.ToString(), obj);
                }
                else
                {
                    PrintStreamEventJson(obj.Payload.FlowKey.ToString(), obj);
                }
            });

            var flowProducerTask = inputFormat == DataFileFormat.Csv
                ? ethanol.DataLoader.LoadFromCsvFiles(sourceFlowFiles, entryObserver, _cancellationTokenSource.Token).ContinueWith(_=> entryObserver.OnCompleted())
                : ethanol.DataLoader.LoadFromNfdFiles(sourceFlowFiles, entryObserver, _cancellationTokenSource.Token);

            var proxyObserver = new Subject<SocketRecord>();
            proxyObserver.Do(DumpsItems).Subscribe(socketObserver);

            var dumpProducerTask = sourceDumpFiles != null
                ? ethanol.DataLoader.LoadFromCsvFiles(sourceDumpFiles, proxyObserver, _cancellationTokenSource.Token).ContinueWith(_ => proxyObserver.OnCompleted())
                : CompleteEmptyObservableTask(socketObserver);
 
            return Task.WhenAll(consumerTask, flowProducerTask, dumpProducerTask).ContinueWith(t => System.Console.Error.WriteLine($"Flows:{totalOutputFlows} [{sw.Elapsed}]"));         
        }

        private void DumpsItems<T>(T obj)
        {
            File.AppendAllText($"{typeof(T)}.dump", yamlSerializer.Serialize(obj));
        }

        private IpfixRecord UpdateWith(IpfixRecord left, IGrouping<FlowKey, SocketRecord> sockRecords)
        {
            if (sockRecords == null) return left;
            var newIpfix = (IpfixRecord)left.Clone();
            newIpfix.ProcessName = sockRecords.FirstOrDefault()?.ProcessName;
            return newIpfix;
        }
        private static Task CompleteEmptyObservableTask<T>(IObserver<T> observer)
        {
            observer.OnCompleted();
            return Task.CompletedTask;
        }

        /// <summary>
        /// Reads input from the provided stream formatted as JSON produced by ipfixcol2.
        /// </summary>
        private async Task BuildContextFromIpfixcolJson(TextReader inputStream, DataFileFormat outputFormat)
        {

            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<IpfixEntry, IpfixRecord>()
                .ForMember(d => d.Bytes, o => o.MapFrom(s => s.IanaOctetDeltaCount))
                .ForMember(d => d.DestinationIpAddress, o => o.MapFrom(s => s.IanaDestinationIPv4Address))
                .ForMember(d => d.DestinationPort, o => o.MapFrom(s => s.IanaDestinationTransportPort))
                .ForMember(d => d.DnsQueryName, o => o.MapFrom(s => s.FlowmonDnsQname.Replace("\0","")))
                .ForMember(d => d.DnsResponseData, o => o.MapFrom(s => s.FlowmonDnsCrrRdata.Replace("\0", "")))
                .ForMember(d => d.HttpHost, o => o.MapFrom(s => s.FlowmonHttpHost.Replace("\0", "")))
                .ForMember(d => d.Packets, o => o.MapFrom(s => s.IanaPacketDeltaCount))
                .ForMember(d => d.Protocol, o => o.MapFrom(s => s.IanaProtocolIdentifier))
                .ForMember(d => d.SourceIpAddress, o => o.MapFrom(s => s.IanaSourceIPv4Address))
                .ForMember(d => d.SourceTransportPort, o => o.MapFrom(s => s.IanaSourceTransportPort))
                .ForMember(d => d.TimeStart, o => o.MapFrom(s => s.IanaFlowStartMilliseconds))
                .ForMember(d => d.TimeDuration, o => o.MapFrom(s => s.IanaFlowEndMilliseconds - s.IanaFlowStartMilliseconds))
                .ForMember(d => d.TlsClientVersion, o => o.MapFrom(s => s.FlowmonTlsClientVersion))
                .ForMember(d => d.TlsJa3, o => o.MapFrom(s => s.FlowmonTlsJa3Fingerprint))
                .ForMember(d => d.TlsServerCommonName, o => o.MapFrom(s => s.FlowmonTlsSubjectCn.Replace("\0", "")))
                .ForMember(d => d.TlsServerName, o => o.MapFrom(s => s.FlowmonTlsSni.Replace("\0", "")));
            });
            var mapper = configuration.CreateMapper();
            var ethanol = new EthanolEnvironment();
            var ipfixStream = new IpfixObservableStream(TimeSpan.FromMinutes(15), TimeSpan.FromMinutes(5));
            var exitStream = ethanol.ContextBuilder.BuildTlsContext(ipfixStream);
            var exitObservable = new ObservableEgressStream<ContextFlow<TlsContext>>(exitStream);
            var consumerTask = exitObservable.ForEachAsync(obj =>
            {
                if (outputFormat == DataFileFormat.Yaml)
                {
                    PrintStreamEventYaml(obj.Payload.FlowKey.ToString(), obj);
                }
                else
                {
                    PrintStreamEventJson(obj.Payload.FlowKey.ToString(), obj);
                }
            });

            while (true)
            {
                var line = await inputStream.ReadLineAsync();
                if (line == null) break;
                if (IpfixEntry.TryDeserialize(line, out var ipfixEntry))
                {
                    var ipfixRecord = mapper.Map<IpfixRecord>(ipfixEntry);
                    ipfixStream.OnNext(ipfixRecord);
                }
            }
            ipfixStream.OnCompleted();
            await Task.WhenAll(consumerTask);
        }


    }
}
