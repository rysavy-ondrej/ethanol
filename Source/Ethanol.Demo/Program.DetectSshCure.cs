using CsvHelper;
using Ethanol.Providers;
using Ethanol.Streaming;
using Microsoft.StreamProcessing;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Ethanol.Demo
{
    partial class Program
    {

        record DetectSshCureConfiguration(OutputFormat OutputFormat, bool ReadFromCsvInput, bool WriteIntermediateFiles, string IntermediateFilesPath);
      
        /// <summary>
        /// Detects TOR communication from context in the collection of <paramref name="sourceFiles"/>.
        /// </summary>
        /// <param name="sourceFiles">The observable collection of source files.</param>
        /// <param name="configuration">The configuration.</param>
        /// <returns>Task that completes when the processing is done.</returns>
        async Task DetectSshCure(IObservable<FileInfo> sourceFiles, DetectSshCureConfiguration configuration)
        {
            var cancellationToken = _cancellationTokenSource.Token;
            Config.ForceRowBasedExecution = true;

            var windowSize = TimeSpan.FromMinutes(15);
            var windowHop = TimeSpan.FromMinutes(5);
            var subject = new Subject<RawIpfixRecord>();
            var nfdump = new NfdumpExecutor();
            var loader = new CsvLoader<RawIpfixRecord>();

            loader.OnReadRecord += (object _, RawIpfixRecord value) => { subject.OnNext(value); };
            if (configuration.WriteIntermediateFiles)
            {
                var records = new List<RawIpfixRecord>();
                loader.OnStartLoading += (_, filename) => { records.Clear(); };
                loader.OnReadRecord += (_, record) => { records.Add(record); };
                loader.OnFinish += (_, filename) => { WriteAllRecords(Path.Combine(configuration.IntermediateFilesPath, $"{filename}.csv"), records); };
            }
            var flowStream = subject.GetWindowedEventStream(x=> DateTime.Parse(x.TimeStart).Ticks, windowSize, windowHop).Multicast(2);

            var portScanStream = flowStream[0]
                .Where(flow => flow.Protocol == "TCP" && flow.InPackets <= 2)
                
                .GroupApply(flow => new Flow("TCP", flow.SrcIp, 0, IPAddress.Any.ToString(), flow.DstPort),
                            group => group.Aggregate(window => window.CollectList(ipfix => new Flow(ipfix.Protocol, ipfix.SrcIp, ipfix.SrcPort, ipfix.DstIp, ipfix.DstPort))),
                            (key, value) => new { Key = key.Key, Value = value.Distinct().ToArray() })
                
                .Where(group => group.Value.Select(flow => flow.DstIp).Distinct().Count() > 50);

            var sshBruteForceStream = flowStream[1]
                .Where(flow => flow.Protocol == "TCP" && flow.InPackets >= 8 && flow.InPackets <= 14)
                
                .GroupApply(flow => new Flow("TCP", flow.SrcIp, 0, IPAddress.Any.ToString(), flow.DstPort),
                            group => group.Aggregate(window => window.CollectList(ipfix => new Flow(ipfix.Protocol, ipfix.SrcIp, ipfix.SrcPort, ipfix.DstIp, ipfix.DstPort))),
                            (key, value) => new { Key = key.Key, Value = value.Distinct().ToArray() })
                
                .Where(group => group.Value.Count() > 20);

            var sshScanTask = portScanStream.ToStreamEventObservable().ForEachAsync(streamEvent => PrintStreamEvent("port-scan", streamEvent), cancellationToken);
            var sshBruteForceTask = sshBruteForceStream.ToStreamEventObservable().ForEachAsync(streamEvent => PrintStreamEvent("ssh-brute", streamEvent), cancellationToken);
            var loaderTasks = sourceFiles
                .ForEachAsync(file => LoadRecordsFromFile(loader, nfdump, file, configuration.ReadFromCsvInput).Wait(), cancellationToken)
                .ContinueWith(_ => subject.OnCompleted());

            await Task.WhenAll(loaderTasks, sshScanTask, sshBruteForceTask);
        }
    }
}
