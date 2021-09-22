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
            var nfdump = new NfDumpExec();
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
            var torFlowsStream = contextStream.Where(f => f.Context.ClientFlows.Any(e => string.IsNullOrWhiteSpace(e.DomainName) && e.TlsServerCommonName == "N/A" && e.ServerNameEntropy > configuration.DomainNameEntropy && e.Flow.DstPt >  443));

            async Task LoadRecordsFromFile(FileInfo fileInfo)
            {
                loader.FlowCount = 0;
                if (configuration.ReadFromCsvInput)
                {
                    await loader.Load(fileInfo.Name, fileInfo.OpenRead());
                }
                else
                {
                    await nfdump.ProcessInputAsync(fileInfo.FullName, "ipv4", async reader => await loader.Load(fileInfo.Name, reader));
                }
                var evt = new { Event = "dump-file-loaded", Time = DateTime.Now, Source = fileInfo.Name, FlowCount = loader.FlowCount };
                Console.WriteLine(yamlSerializer.Serialize(evt));
            }

            // consumer:
            var consumer = torFlowsStream.ToStreamEventObservable().ForEachAsync(PrintTorFlowRecords, cancellationToken);
            
            // producer:
            await sourceFiles
                .ForEachAsync(f => LoadRecordsFromFile(f).Wait(), cancellationToken)
                .ContinueWith(_ => subject.OnCompleted());
            Task.WaitAll(new[] { consumer }, cancellationToken);
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

        /// <summary>
        /// Produces an output for the single stream output.
        /// </summary>
        /// <typeparam name="T">The type of context.</typeparam>
        /// <param name="obj">Flow and context.</param>
        private void PrintTorFlowRecords<T>(StreamEvent<T> obj)
        {
            if (obj.IsInterval || obj.IsEnd)
            {
                var evt = new { Event = "tor-flow-detected", ValidTime = new { Start = new DateTime(obj.StartTime), End = new DateTime(obj.EndTime) }, Payload = obj.Payload };
                Console.WriteLine( yamlSerializer.Serialize(evt));
            }
        }
        /// <summary>
        /// Computes an entropy of the given string.
        /// </summary>
        /// <param name="message">A string to compute entropy for.</param>
        /// <returns>A flow value representing Shannon's entropy for the given string.</returns>
        private double ComputeEntropy(string message)
        {
            if (message == null) return 0;
            Dictionary<char, int> K = message.GroupBy(c => c).ToDictionary(g => g.Key, g => g.Count());
            double entropyValue = 0;
            foreach (var character in K)
            {
                double PR = character.Value / (double)message.Length;
                entropyValue -= PR * Math.Log(PR, 2);
            }
            return entropyValue;
        }
        /// <summary>
        /// Computes entropy for individual parts of the domain name.
        /// </summary>
        /// <param name="domain">The domain name.</param>
        /// <returns>An array of entropy values for each domain name.</returns>
        private double[] ComputeDnsEntropy(string domain)
        {
            if (string.IsNullOrWhiteSpace(domain)) return new double[] { 0.0 };
            var parts = domain.Split('.');
            return parts.Select(ComputeEntropy).ToArray();
        }
    }
}
