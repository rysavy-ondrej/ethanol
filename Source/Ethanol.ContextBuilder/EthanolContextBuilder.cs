using Ethanol.ContextBuilder.Aggregators;
using Ethanol.ContextBuilder.Context;
using Ethanol.ContextBuilder.Enrichers;
using Ethanol.ContextBuilder.Observable;
using Ethanol.ContextBuilder.Pipeline;
using Ethanol.ContextBuilder.Polishers;
using Ethanol.ContextBuilder.Readers;
using Ethanol.ContextBuilder.Writers;
using Ethanol.DataObjects;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
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
    public static partial class EthanolContextBuilder
    {
        /// <summary>
        /// Merges data from multiple data readers into a single observable sequence.
        /// </summary>
        /// <typeparam name="TFlow">The type of data produced by the readers.</typeparam>
        /// <param name="readers">An array of data readers.</param>
        /// <returns>An observable sequence containing data from all provided readers.</returns>
        public static IObservable<TFlow> Produce<TFlow>(params IDataReader<TFlow>[] readers)
        {
            return readers.Merge();
        }

        /// <summary>
        /// Orders the sequence of observable events by applying a sequencer transformation.
        /// </summary>
        /// <typeparam name="TPayload">The type of payload in the observable event.</typeparam>
        /// <param name="observable">The observable sequence to be ordered.</param>
        /// <param name="sequenceLength">The length of the sequence for ordering.</param>
        /// <returns>An ordered observable sequence of events.</returns>
        public static IObservable<ObservableEvent<TPayload>> OrderSequence<TPayload>(this IObservable<ObservableEvent<TPayload>> observable, IObservableTransformer<ObservableEvent<TPayload>, ObservableEvent<TPayload>> sequencer)
        {

            observable.Subscribe(sequencer);
            return sequencer;
        }

        /// <summary>
        /// Converts a string representation of an IP address to an IPAddress object, or returns IPAddress.None if conversion fails.
        /// </summary>
        /// <param name="ipString">The string representation of the IP address.</param>
        /// <returns>An IPAddress object or IPAddress.None.</returns>
        public static IPAddress GetAddressOrDefault(this string ipString)
        {
            if (ipString == null) return IPAddress.None;
            return IPAddress.TryParse(ipString, out var address) ? address : IPAddress.None;
        }

        /// <summary>
        /// Transforms a sequence of IpFlow events into a sequence of context objects based on IP addresses.
        /// </summary>
        /// <typeparam name="TContext">The type of context object to be produced.</typeparam>
        /// <param name="source">The source observable sequence of IpFlow events.</param>
        /// <param name="resultSelector">A function to transform grouped IpFlow events into a context object.</param>
        /// <returns>An observable sequence of context objects.</returns>
        public static IObservable<ObservableEvent<TContext>> HostContext<TContext>(this IObservable<ObservableEvent<IObservable<IpFlow>>> source,
            Func<KeyValuePair<IPAddress, IpFlow[]>, TContext> resultSelector,
            Func<TContext>? periodFunc = null
            )
        {
            if (source is null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (resultSelector is null)
            {
                throw new ArgumentNullException(nameof(resultSelector));
            }
            // duplicates flows so that they can be assigned to both end points:
            KeyValuePair<IPAddress, IpFlow>[] GetKey(IpFlow flow)
            {
                return new[] { new KeyValuePair<IPAddress, IpFlow>(flow.SourceAddress ?? IPAddress.None, flow), new KeyValuePair<IPAddress, IpFlow>(flow.DestinationAddress ?? IPAddress.None, flow) };
            }
            return source.HostContext(GetKey, resultSelector, periodFunc);
        }

        /// <summary>
        /// General method to transform a sequence of flows into a sequence of context objects based on a key selector and result selector.
        /// </summary>
        /// <typeparam name="TFlow">The type of the flow in the observable sequence.</typeparam>
        /// <typeparam name="TKey">The type of the key used for grouping flows.</typeparam>
        /// <typeparam name="TContext">The type of context object to be produced.</typeparam>
        /// <param name="source">The source observable sequence.</param>
        /// <param name="keySelector">A function to select the key for grouping flows.</param>
        /// <param name="resultSelector">A function to transform grouped flows into a context object.</param>
        /// <returns>An observable sequence of context objects.</returns>
        public static IObservable<ObservableEvent<TContext>> HostContext<TFlow, TKey, TContext>(this IObservable<ObservableEvent<IObservable<TFlow>>> source,
            Func<TFlow, IEnumerable<KeyValuePair<TKey, TFlow>>> keySelector,
            Func<KeyValuePair<TKey, TFlow[]>, TContext> resultSelector,
            Func<TContext>? periodFunc
            )
        {
            if (source is null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (keySelector is null)
            {
                throw new ArgumentNullException(nameof(keySelector));
            }
            if (resultSelector is null)
            {
                throw new ArgumentNullException(nameof(resultSelector));
            }
            if (periodFunc is not null)
            {
                return source.SelectMany(window =>
                    (window.Payload ?? Array.Empty<TFlow>().ToObservable())
                        .SelectMany(keySelector)
                        .GroupByAggregate(k => k.Key, v => v.Value, g => new ObservableEvent<TContext>(resultSelector(g), window.StartTime, window.EndTime))
                        .Append(new ObservableEvent<TContext>(periodFunc(), window.StartTime, window.EndTime))
                    );
            }
            else
            {
                return source.SelectMany(window =>
                    (window.Payload ?? Array.Empty<TFlow>().ToObservable())
                        .SelectMany(keySelector)
                        .GroupByAggregate(k => k.Key, v => v.Value, g => new ObservableEvent<TContext>(resultSelector(g), window.StartTime, window.EndTime))
                    );
            }
        }

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
        /// Applies a transformation to an observable sequence using the specified transformer.
        /// </summary>
        /// <typeparam name="TSource">The type of elements in the source sequence.</typeparam>
        /// <typeparam name="TResult">The type of elements in the result sequence.</typeparam>
        /// <param name="source">The source observable sequence to transform.</param>
        /// <param name="transformer">The transformer to apply to the source sequence.</param>
        /// <returns>An observable sequence containing the transformed elements.</returns>
        public static IObservable<ObservableEvent<TResult>> Transform<TSource, TResult>(this IObservable<ObservableEvent<TSource>> source, IObservableTransformer<ObservableEvent<TSource>, ObservableEvent<TResult>> transformer)
        {
            if (source is null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (transformer is null)
            {
                throw new ArgumentNullException(nameof(transformer));
            }

            source.Subscribe(transformer);
            return transformer;
        }
        /// <summary>
        /// Transforms the elements of an observable sequence using the specified transformer.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <typeparam name="TResult">The type of the elements in the transformed sequence.</typeparam>
        /// <param name="source">The source observable sequence.</param>
        /// <param name="transformer">The transformer to apply to each element of the source sequence.</param>
        /// <returns>An observable sequence that contains the transformed elements.</returns>
        public static IObservable<TResult> Transform<TSource, TResult>(this IObservable<TSource> source, IObservableTransformer<TSource, TResult> transformer)
        {
            if (source is null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (transformer is null)
            {
                throw new ArgumentNullException(nameof(transformer));
            }

            source.Subscribe(transformer);
            return transformer;
        }

        /// <summary>
        /// Represents a set of builder modules consisting of readers, writers, an enricher, and a refiner.
        /// </summary>
        /// <param name="Readers">An array of data readers for IpFlow.</param>
        /// <param name="Writers">An array of data writers for ObservableEvent of IpTargetHostContext.</param>
        /// <param name="Enricher">A transformer to enrich IpHostContext with tags.</param>
        /// <param name="Refiner">A transformer to refine IpHostContextWithTags into IpTargetHostContext.</param>
        public record BuilderModules(
            IDataReader<IpFlow>[] Readers,
            IDataWriter<HostContext>[] Writers,
            IObservableTransformer<ObservableEvent<IpHostContext>, ObservableEvent<IpHostContextWithTags>> Enricher,
            IObservableTransformer<ObservableEvent<IpHostContextWithTags>, HostContext> Refiner);



        /// <summary>
        /// Represents an asynchronous operation that can return a value.
        /// </summary>
        /// <typeparam name="TResult">The type of the result produced by the task.</typeparam>
        public static async Task RunAsync(
            IDataReader<IpFlow>[] readers,
            IDataWriter<HostContext>[] writers,
            IEnricher<ObservableEvent<IpHostContext>, ObservableEvent<IpHostContextWithTags>> enricher,
            IRefiner<ObservableEvent<IpHostContextWithTags>, HostContext> refiner,
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
                .Where(window => window.Payload!=null)                  // remove empty windows from the processing pipeline
                .Do(x=> { if (builderStats != null) builderStats.CreatedWindows++; })
                .SelectMany(window =>
                        {
                            if (builderStats != null) builderStats.CurrentWindowStart = window.StartTime;
                            return window.Payload
                                .AggregateIpContexts(window.StartTime.Ticks, window.EndTime.Ticks)
                                .Do(ctx => { if (builderStats != null) builderStats.ContextsBuilt++; })
                                .Where(ctx => ContextFilter(filter, ctx))
                                .Do(ctx => { if (builderStats != null) builderStats.ContextAccepted++; })
                                .ObserveOn(NewThreadScheduler.Default)          // This causes that the following code is executed in other thread.
                                .Enrich(enricher)
                                .Polish(refiner);
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
        /// Orchestrates the entire data processing pipeline using the provided builder modules.
        /// This method sets up and executes the pipeline, handling data through various stages
        /// like reading, writing, enriching, and refining based on the provided modules.
        /// </summary>
        /// <param name="modules">
        /// The set of builder modules including readers, writers, enricher, and refiner.
        /// These modules collectively define the behavior of the data processing pipeline.
        /// </param>
        /// <param name="windowSpan">
        /// The time span for the hopping window operation. This defines the window size for processing data batches.
        /// </param>
        /// <param name="inputOrderingQueueLength">
        /// The length of the queue for ordering input data. This parameter helps manage the order in which data is processed.
        /// </param>
        /// <param name="filter">
        /// A host-based filter to apply to the processing pipeline. This filter allows for selective processing of data based on certain criteria.
        /// </param>
        /// <param name="builderStatsObserver">
        /// An optional observer for builder statistics. If provided, the observer will receive updates on various metrics and statistics of the pipeline's operation.
        /// This parameter is optional and can be null.
        /// </param>
        /// <returns>
        /// A task representing the asynchronous operation of the pipeline. This task can be awaited to ensure the pipeline's completion.
        /// </returns>
        public static async Task<BuilderStatistics> Run(
            BuilderModules modules,
            TimeSpan windowSpan,
            int inputOrderingQueueLength,
            IHostBasedFilter filter,
            CancellationToken cancellationToken,
            IObserver<BuilderStatistics>? builderStatsObserver,
            ILogger? logger)
        {
            int flowsLoadedCount = 0;
            int flowsConsumedCount = 0;
            int windowsCreatedCount = 0;
            int contextsCreatedCount = 0;
            int contextsWrittenCount = 0;


            var sequencer = new SequencerTransformer<IpFlow>(inputOrderingQueueLength);
            var windowTransformer = new HoppingWindowAggregator<IpFlow>(windowSpan, null);

            BuilderStatistics CreateReport(long delta)
            {
                return new BuilderStatistics
                {
                    LoadedFlows = flowsLoadedCount,
                    ConsumedFlows = flowsConsumedCount,
                    CreatedWindows = windowsCreatedCount,
                    ContextsBuilt = contextsCreatedCount,
                    ContextsWritten = contextsWrittenCount,
                    CurrentWindowStart = windowTransformer.Counters.TryGetValue("CurrentWindowStart", out var value) ? new DateTime((long)value) : DateTime.MinValue,
                    CurrentTimestamp = windowTransformer.Counters.TryGetValue("CurrentTimestamp", out value) ? new DateTime((long)value) : DateTime.MinValue,
                };
            }

            var progressReportSubscription = builderStatsObserver is not null ? System.Reactive.Linq.Observable.Interval(TimeSpan.FromSeconds(10)).Select(CreateReport).Subscribe(builderStatsObserver) : null;

            var pipelineTask = Produce(modules.Readers)

                     .Do(_ => flowsLoadedCount++)

                     .Where(OnlyValidUnicastFlows).Where(flow => FilterFlows(filter, flow))

                     .Select(t => new ObservableEvent<IpFlow>(t, t.TimeStart, t.TimeStart + t.TimeDuration))

                     .Do(_ => flowsConsumedCount++)

                     .OrderSequence(sequencer)

                     .HoppingWindow(windowTransformer)

                     .Do(_ => windowsCreatedCount++)

                     .HostContext(g => new IpHostContext { HostAddress = g.Key, Flows = g.Value }, () => new IpHostContext { HostAddress = IPAddress.Any, Flows = Array.Empty<IpFlow>() })

                     .Do(_ => contextsCreatedCount++)

                     .Where(ctx => ContextFilter(filter, ctx))

                     .Transform(modules.Enricher)

                     .Transform(modules.Refiner)

                     .Do(_ => contextsWrittenCount++)

                     .Consume(modules.Writers);

            // for multiple readers, we need to run them in parallel on the background:
            var _readerTasks = modules.Readers.Select(x =>
            {
                logger?.LogInformation($"Starting input reader {x}.");
                return Task.Run(async () => await x.ReadAllAsync(cancellationToken));
            }).ToArray();

            // wait to completion of the main pipeline:
            await pipelineTask;
            logger?.LogInformation("Pipeline task completed.");

            // release progress reporting when we done:
            progressReportSubscription?.Dispose();

            // readers should be done as well either completed or cancelled:
            try
            {
                await Task.WhenAll(_readerTasks);
            }
            catch (OperationCanceledException)
            {
                logger?.LogInformation("Pipeline operation was cancelled.");
            }

            return CreateReport(0);
        }

        /// <summary>
        /// Filters the observable event based on the provided filter.
        /// </summary>
        /// <param name="filter">The host-based filter to apply.</param>
        /// <param name="f">The observable event containing the IP host context.</param>
        /// <returns>True if the context passes the filter; otherwise, false.</returns>
        private static bool ContextFilter(IHostBasedFilter filter, ObservableEvent<IpHostContext>? f)
        {
            if (f == null) return false;
            if (IPAddress.Any.Equals(f.Payload?.HostAddress)) return true;
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