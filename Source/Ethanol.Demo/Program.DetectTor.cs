using CsvHelper;
using Ethanol.Providers;
using Ethanol.Streaming;
using Microsoft.StreamProcessing;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Ethanol.Demo
{
    partial class Program
    {

        record DetectTorConfiguration(double DomainNameEntropy, OutputFormat OutputFormat, bool ReadFromCsvInput, bool WriteIntermediateFiles, string IntermediateFilesPath);
      
        /// <summary>
        /// Detects TOR communication from context in the collection of <paramref name="sourceFiles"/>.
        /// </summary>
        /// <param name="sourceFiles">The observable collection of source files.</param>
        /// <param name="configuration">The configuration.</param>
        /// <returns>Task that completes when the processing is done.</returns>
        async Task DetectTor(IObservable<FileInfo> sourceFiles, DetectTorConfiguration configuration)
        {
            var cancellationToken = _cancellationTokenSource.Token;
            Config.ForceRowBasedExecution = true;

            var windowSize = TimeSpan.FromMinutes(15);
            var windowHop = TimeSpan.FromMinutes(5);

            // used only if reading from nfdump source files
            var nfdump = new NfdumpExecutor();
            var loader = new CsvLoader<RawIpfixRecord>();            
            var subject = new Subject<RawIpfixRecord>();           

            loader.OnReadRecord += (object _, RawIpfixRecord value) => { subject.OnNext(value); };
            if (configuration.WriteIntermediateFiles)
            {
                var records = new List<RawIpfixRecord>();
                loader.OnStartLoading += (_, filename) => { records.Clear(); };
                loader.OnReadRecord += (_, record) => { records.Add(record); };
                loader.OnFinish += (_, filename) => { WriteAllRecords(Path.Combine(configuration.IntermediateFilesPath, $"{filename}.csv"), records); };
            }
            var flowStream = subject.GetWindowedEventStream(x=> DateTime.Parse(x.TimeStart).Ticks, windowSize, windowHop);
            var contextStream = BuildTlsContext(flowStream, new BuildFlowContextConfiguration());
            
            // simple TOR detection rule:
            // If any flow in the context is TLS with randomly generated server name, without common name and service port > 443:
            var torFlowsStream = contextStream.Where(
                ctxFlow => ctxFlow.Context.ClientFlows.Any(fact => 
                       string.IsNullOrWhiteSpace(fact.DomainName) 
                    && fact.TlsServerCommonName == "N/A" 
                    && fact.ServerNameEntropy > configuration.DomainNameEntropy 
                    && fact.Flow.DstPt >  443));

            // consumer:
            var consumer = torFlowsStream.ToStreamEventObservable().ForEachAsync(e => PrintStreamEvent("tor-flow-detected",e), cancellationToken);
            
            // producer:
            var producer = sourceFiles
                .ForEachAsync(f => LoadRecordsFromFile(loader, nfdump, f, configuration.ReadFromCsvInput).Wait(), cancellationToken)
                .ContinueWith(_ => subject.OnCompleted());
            await Task.WhenAll(producer, consumer);
        }

        /// <summary>
        /// Writes all records to CSV file.
        /// </summary>
        /// <param name="filename">Target filename of the CSV file to be produced.</param>
        /// <param name="records">A list of records to be written to the output file</param>
        private void WriteAllRecords<T>(string filename, IEnumerable<T> records)
        {
            using (var writer = new StreamWriter(filename))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(records);
            }
        }
    }
}
