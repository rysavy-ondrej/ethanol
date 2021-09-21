using Ethanol.Providers;
using Microsoft.StreamProcessing;
using System;
using System.Collections.Generic;
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
        /// <summary>
        /// YAML serializer used to produce output.
        /// </summary>
        readonly ISerializer yamlSerializer = new SerializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).DisableAliases().Build();
        record DetectTorConfiguration(double DomainNameEntropy, OutputFormat OutputFormat);
      
        async Task DetectTor(IObservable<FileInfo> sourceFiles, DetectTorConfiguration configuration)
        {
            var cancellationToken = _cancellationTokenSource.Token;
            Config.ForceRowBasedExecution = true;

            var windowSize = TimeSpan.FromMinutes(15);
            var windowHop = TimeSpan.FromMinutes(5);

            var nfdump = new NfDumpExec();
            var loader = new CsvLoader<RawIpfixRecord>();            
            var subject = new Subject<RawIpfixRecord>();           

            loader.OnReadRecord += (object _, RawIpfixRecord value) => { subject.OnNext(value); };
            
            var flowStream = GetEventStreamFromObservable(subject, x=> DateTime.Parse(x.ts).Ticks, windowSize, windowHop);
            var contextStream = BuildTlsContext(flowStream, new BuildFlowContextConfiguration());
            var torFlowsStream = contextStream.Where(f => f.Context.ClientFlows.Any(e => String.IsNullOrWhiteSpace(e.DomainName) && e.CommonName == "N/A" && e.ServerNameEntropy > configuration.DomainNameEntropy && e.Flow.DstPt >  443));

            async Task LoadRecordsFromFile(FileInfo fileInfo)
            {
                loader.FlowCount = 0;
                await nfdump.ProcessInputAsync(fileInfo.FullName, "ipv4", async reader => await loader.Load(reader));
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
        /// Produces an output for the single stream output.
        /// </summary>
        /// <typeparam name="T">The type of context.</typeparam>
        /// <param name="obj">Flow and context.</param>
        void PrintTorFlowRecords<T>(StreamEvent<T> obj)
        {
            if (obj.IsInterval || obj.IsEnd)
            {
                var evt = new { Event = "tor-flow-detected", ValidTime = new { Start = new DateTime(obj.StartTime), End = new DateTime(obj.EndTime) }, Payload = obj.Payload };
                Console.WriteLine( yamlSerializer.Serialize(evt));
            }
        }

        /// <summary>
        /// Gets the stream from the given observable. It uses <paramref name="getStartTime"/> to retrieve 
        /// start time timestamp of observable records to produce stream events. It also performs 
        /// hopping window time adjustement on all ingested events.
        /// </summary>
        /// <typeparam name="T">The type of event payloads.</typeparam>
        /// <param name="observable">The input observable.</param>
        /// <param name="getStartTime">The function to get start time of a record.</param>
        /// <param name="windowSize">Size of the hopping window.</param>
        /// <param name="windowHop">Hop size in ticks.</param>
        /// <returns>A stream of events with defined start times adjusted to hopping windows.</returns>
        private IStreamable<Empty, T> GetEventStreamFromObservable<T>(IObservable<T> observable, Func<T,long> getStartTime, TimeSpan windowSize, TimeSpan windowHop)
        {
            bool ValidateTimestamp((T Record,long StartTime) record) =>
                record.StartTime > DateTime.MinValue.Ticks && record.StartTime < DateTime.MaxValue.Ticks;            

            var source = observable
                .Select(x=> (Record:x,StartTime:getStartTime(x)))
                .Where(ValidateTimestamp)
                .Select(x => StreamEvent.CreatePoint(x.StartTime, x.Record));
            return source.ToStreamable(disorderPolicy: DisorderPolicy.Adjust(TimeSpan.FromMinutes(5).Ticks), FlushPolicy.FlushOnBatchBoundary).HoppingWindowLifetime(windowSize.Ticks, windowHop.Ticks);
        }
        /// <summary>
        /// Computes an entropy of the given string.
        /// </summary>
        /// <param name="message">A string to compute entropy for.</param>
        /// <returns>A flow value representing Shannon's entropy for the given string.</returns>
        public double ComputeEntropy(string message)
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
        public double[] ComputeDnsEntropy(string domain)
        {
            if (string.IsNullOrWhiteSpace(domain)) return new double[] { 0.0 };
            var parts = domain.Split('.');
            return parts.Select(ComputeEntropy).ToArray();
        }
    }
}
