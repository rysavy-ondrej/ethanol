using Ethanol.ContextBuilder.Enrichers;
using Ethanol.ContextBuilder.Filters;
using Ethanol.ContextBuilder.Refiners;
using Ethanol.ContextBuilder.Reactive;
using Ethanol.ContextBuilder.Readers;
using Ethanol.ContextBuilder.Writers;
using Ethanol.DataObjects;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Ethanol.ContextBuilder
{
    /// <summary>
    /// Provides static methods for building and orchestrating a data processing pipeline in the Ethanol context.
    /// </summary>
    public static class EthanolContextBuilder
    {
        /// <summary>
        /// Subscribes to an observable source and writes the emitted items to the specified data writers.
        /// </summary>
        /// <typeparam name="T">The type of data being consumed.</typeparam>
        /// <param name="source">The observable source to subscribe to.</param>
        /// <param name="writers">An array of data writers to write the data to.</param>
        /// <returns>A task representing the asynchronous operation of writing data.</returns>
        public static Task Consume<T>(this IObservable<T> source, params IDataWriter<T>[] writers)
        {
            if (source is null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (writers is null || writers.Length == 0)
            {
                throw new ArgumentException("At least one write has to be provided.", nameof(writers));
            }

            var connectable = source.Publish();
            foreach (var writer in writers)
            {
                connectable.Subscribe(writer);
            }
            connectable.Connect();
            return Task.WhenAll(writers.Select(x => x.Completed));
        }

        /// <summary>
        /// Represents an asynchronous operation that can return a value.
        /// </summary>
        /// <typeparam name="TResult">The type of the result produced by the task.</typeparam>
        public static async Task RunAsync(
            IDataReader<IpFlow>[] readers,
            IDataWriter<HostContext>[] writers,
            IEnricher<TimeRange<IpHostContext>, TimeRange<IpHostContextWithTags>> enricher,
            IRefiner<TimeRange<IpHostContextWithTags>, HostContext> refiner,
            TimeSpan windowSpan,
            TimeSpan windowShift,
            IHostBasedFilter filter,
            ILogger? logger,
            BuilderStatistics? builderStats,
            CancellationToken cancellationToken)
        {
            var pipelineTask = readers                
                .Merge()
                .Do(x=> { if (builderStats != null) builderStats.LoadedFlows++; })
                .Where(OnlyValidUnicastFlows).Where(flow => FilterFlows(filter, flow))
                .Do(x=> { if (builderStats != null) builderStats.ConsumedFlows++; })
                .Select(f => new Timestamped<IpFlow>(f, f.TimeStart))
                .Do(f => { if (builderStats != null) builderStats.CurrentTimestamp = f.Timestamp.DateTime; })
                .VirtualWindow(windowSpan, windowShift)
                .Where(window => window.Value!=null)                  // remove empty windows from the processing pipeline
                .Do(x=> { if (builderStats != null) builderStats.CreatedWindows++; })
                .SelectMany(window =>
                        {
                            if (builderStats != null) builderStats.CurrentWindowStart = window.StartTime;
                            return window.Value!
                                .AggregateIpContexts(window.StartTime.Ticks, window.EndTime.Ticks)
                                .Do(ctx => { if (builderStats != null) builderStats.ContextsBuilt++; })
                                .Where(ctx => ContextFilter(filter, ctx))
                                .Do(ctx => { if (builderStats != null) builderStats.ContextAccepted++; })
                                .ObserveOn(NewThreadScheduler.Default)          // This causes that the following code is executed in other thread.
                                .Enrich(enricher).Where(x=>x!=null).Select(x=>x!) 
                                .Refine(refiner).Where(x=>x!=null).Select(x=>x!);
                        }
                ).Do(f => { if (builderStats != null) builderStats.ContextsWritten++; })
                .Consume(writers);

            // for multiple readers, we need to run them in parallel on the background:
            var _readerTasks = readers.Select(x =>
            {
                logger?.LogInformation($"Starting input reader {x}.");
                return Task
                    .Run(async () => await x.ReadAllAsync(cancellationToken))
                    .ContinueWith(t => logger?.LogInformation($"Input reader {x} completed."));
            }).ToArray();

            // wait to completion of the main pipeline:
            await pipelineTask;
            logger?.LogInformation("Pipeline task completed.");

            // readers should be done as well either completed or cancelled:
            try
            {
                logger?.LogInformation("Waiting to readers...");
                await Task.WhenAll(_readerTasks);
                logger?.LogInformation("Readers completed.");
            }
            catch (OperationCanceledException)
            {
                logger?.LogInformation("Pipeline operation was cancelled.");
            }
        }

        /// <summary>
        /// Filters the observable event based on the provided filter.
        /// </summary>
        /// <param name="filter">The host-based filter to apply.</param>
        /// <param name="f">The observable event containing the IP host context.</param>
        /// <returns>True if the context passes the filter; otherwise, false.</returns>
        private static bool ContextFilter(IHostBasedFilter filter, TimeRange<IpHostContext>? f)
        {
            if (f == null) return false;
            if (IPAddress.Any.Equals(f.Value?.HostAddress)) return true;
            return filter.Evaluate(f);
        }
        /// <summary>
        /// Filters the given IP flow based on the provided host-based filter.
        /// </summary>
        /// <param name="filter">The host-based filter to apply.</param>
        /// <param name="flow">The IP flow to filter.</param>
        /// <returns><c>true</c> if the flow matches the filter; otherwise, <c>false</c>.</returns>
        private static bool FilterFlows(IHostBasedFilter filter, IpFlow? flow)
        {
            if (flow == null || flow.SourceAddress == null || flow.DestinationAddress == null) return false;
            return filter.Match(flow.SourceAddress) || filter.Match(flow.DestinationAddress);
        }

        /// <summary>
        /// Determines whether the given IP flow represents a valid unicast flow.
        /// </summary>
        /// <param name="flow">The IP flow to check.</param>
        /// <returns><c>true</c> if the IP flow is a valid unicast flow; otherwise, <c>false</c>.</returns>
        private static bool OnlyValidUnicastFlows(IpFlow? flow)
        {
            if (flow == null) return false;
            if (flow.FlowKey == null) return false;
            if (flow.FlowKey.SourceAddress == null) return false;
            if (flow.FlowKey.DestinationAddress == null) return false;

            if (flow.FlowKey.SourceAddress.AddressFamily == AddressFamily.InterNetwork && flow.FlowKey.DestinationAddress.AddressFamily == AddressFamily.InterNetwork)
            {
                return
                    flow.FlowKey.SourceAddress.Equals(IPAddress.Any) == false && flow.FlowKey.DestinationAddress.Equals(IPAddress.Any) == false
                && flow.FlowKey.SourceAddress.Equals(IPAddress.Broadcast) == false && flow.FlowKey.DestinationAddress.Equals(IPAddress.Broadcast) == false
                && flow.FlowKey.SourceAddress.Equals(IPAddress.Loopback) == false && flow.FlowKey.DestinationAddress.Equals(IPAddress.Loopback) == false;
            }
            if (flow.FlowKey.SourceAddress.AddressFamily == AddressFamily.InterNetworkV6 && flow.FlowKey.DestinationAddress.AddressFamily == AddressFamily.InterNetworkV6)
            {
                return
                    flow.FlowKey.SourceAddress.Equals(IPAddress.IPv6Any) == false && flow.FlowKey.DestinationAddress.Equals(IPAddress.IPv6Any) == false
                && flow.FlowKey.SourceAddress.Equals(IPAddress.IPv6Loopback) == false && flow.FlowKey.DestinationAddress.Equals(IPAddress.IPv6Loopback) == false
                && flow.FlowKey.SourceAddress.Equals(IPAddress.IPv6None) == false && flow.FlowKey.DestinationAddress.Equals(IPAddress.IPv6None) == false;
            }
            return false;
        }

        /// <summary>
        /// Represents statistics related to the management and processing of flows in a builder system.
        /// This record tracks metrics like loaded, consumed, and buffered flows, as well as statistics
        /// related to window creation and flow handling.
        /// </summary>
        public record BuilderStatistics
        {
            /// <summary>Gets or sets the number of flows that have been loaded into the system.</summary>
            public long LoadedFlows { get; set; }

            /// <summary>Gets or sets the number of flows that have been consumed by the system.</summary>
            public long ConsumedFlows { get; set; }

            /// <summary>
            /// Gets or sets the number of dropped flows becasue they did not pass the input filter.
            /// </summary>
            public long DroppedFlows => LoadedFlows - ConsumedFlows;

            /// <summary>Gets or sets the number of windows created by the system for managing flows.</summary>
            public int CreatedWindows { get; set; }

            /// <summary>Gets or sets the number of contexts that have been built in the system.</summary>
            public int ContextsBuilt { get; set; }

            /// <summary>
            /// Gets or sets the number of contexts that passsed the context filter.
            /// </summary>
            public int ContextAccepted { get; set; }

            /// <summary>Gets or sets the number of contexts that have been written to an output or storage.</summary>
            public int ContextsWritten { get; set; }

            /// <summary>Gets or sets the start time of the current window for flow processing.</summary>
            public DateTime CurrentWindowStart { get; set; }

            /// <summary>Gets or sets the current timestamp within the system.</summary>
            public DateTime CurrentTimestamp { get; set; }
        }
    }
}