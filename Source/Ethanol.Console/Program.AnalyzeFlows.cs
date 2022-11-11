using Ethanol.Console.DataObjects;
using Ethanol.Console.Readers;
using Ethanol.Streaming;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Ethanol.Console
{
    partial class Program
    {
        readonly IDeserializer yamlDeserializer = new DeserializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).Build();

        private Task CheckHostContextAsync(TextReader inputStream, DataFileFormat inputFormat, DataFileFormat outputFormat)
        {
            foreach (var record in LoadContextRecords(inputStream))
            {
                System.Console.WriteLine();
                System.Console.WriteLine($"[{record.ValidTime}] checking network activity of host {record.Event}.");
                DetectAzorult(record.Payload);
            }
            return Task.CompletedTask;
        }

        private bool DetectAzorult(HostContext<NetworkActivity> record)
        {
            if (
                (record.Value.Http?.Any(x => x.Url.Contains("whatismyipaddress.com")) ?? false)
                &&
                (record.Value.Http?.Any(x => x.Url.Contains("22ssh.com.com")) ?? false)
                ||
                (
                (record.Value.Dns?.Any(d => d.DomainNane.Contains("ipify.org")) ?? false)
                &&
                (record.Value.Dns?.Any(d => d.DomainNane.Contains("duckdns.org")) ?? false)
                )
            )
            {
                System.Console.WriteLine($"! Malware of the 'azorult' family has been detected:");
                System.Console.WriteLine($"    Infected host: {record.HostKey}");
                System.Console.WriteLine($"    This is a Trojan and keylogger that is used to retrieve private information such as passwords and login credentials."); 
                System.Console.WriteLine($"    It is an advanced malware that features strong anti-evasion functions.");
                System.Console.WriteLine($"    See: https://bazaar.abuse.ch/sample/37d8e1ce3b6e6488942717aa78cb54785edc985143bcc8d9ba9f42d73a3dbd7a/");
                return true;
            }
            return false;
        }

        private IEnumerable<EthanolEvent<HostContext<NetworkActivity>>> LoadContextRecords(TextReader inputStream)
        { 
            while (true)
            {
                var singleRecord = ReadYamlRecord(inputStream);
                if (singleRecord == null) yield break;
                var record = yamlDeserializer.Deserialize<EthanolEvent<HostContext<NetworkActivity>>>(singleRecord);
                yield return record;
            }
        }

        private string ReadYamlRecord(TextReader inputStream)
        {
            StringBuilder sb = new StringBuilder();
            while (true)
            {
                var line = inputStream.ReadLine();
                if (line == null) return null;
                sb.AppendLine(line);
                if (line.Trim() == "...") return sb.ToString();
            }
        }
    }

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

            var proxyObserver = new Subject<SocketEntry>();
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

        private IpfixRecord UpdateWith(IpfixRecord left, IGrouping<FlowKey, SocketEntry> socketRecords)
        {
            if (socketRecords == null) return left;
            var newIpfix = (IpfixRecord)left.Clone();
            newIpfix.ProcessName = socketRecords.FirstOrDefault()?.ProcessName;
            return newIpfix;
        }
        private static Task CompleteEmptyObservableTask<T>(IObserver<T> observer)
        {
            observer.OnCompleted();
            return Task.CompletedTask;
        }

        private async Task BuildHostCentricContext(IIpfixRecordReader inputStream, DataFileFormat outputFormat)
        {           
            var ethanol = new EthanolEnvironment();
            var ipfixStream = new IpfixObservableStream(TimeSpan.FromMinutes(15), TimeSpan.FromMinutes(5));
            var exitStream = ethanol.ContextBuilder.BuildHostContext(ipfixStream);
            var exitObservable = new ObservableEgressStream<HostContext<NetworkActivity>>(exitStream);
            var consumerTask = exitObservable.ForEachAsync(obj =>
            {
                if (outputFormat == DataFileFormat.Yaml)
                {
                    PrintStreamEventYaml(obj.Payload.HostKey.ToString(), obj);
                }
                else
                {
                    PrintStreamEventJson(obj.Payload.HostKey.ToString(), obj);
                }
            });

            while (inputStream.TryReadNextEntry(out var ipfixRecord))
            {
                ipfixStream.OnNext(ipfixRecord);
            }

            ipfixStream.OnCompleted();
            await Task.WhenAll(consumerTask);

        }

        /// <summary>
        /// Reads input from the provided reader and builds the flow context.
        /// </summary>
        private async Task BuildFlowContext(IIpfixRecordReader inputStream, DataFileFormat outputFormat)
        { 
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

            while (inputStream.TryReadNextEntry(out var ipfixRecord))
            {
                ipfixStream.OnNext(ipfixRecord);
            }
            ipfixStream.OnCompleted();
            await Task.WhenAll(consumerTask);
        }
    }
}
