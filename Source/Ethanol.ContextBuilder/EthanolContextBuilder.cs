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
using System.Collections.Generic;
using System.Threading.Tasks.Dataflow;
using System.Diagnostics.Metrics;

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

        public static int BufferSize { get; set; } = 20000;

        static Meter s_meter = new Meter("Ethanol.ContextBuilder");

        public static async Task RunAsync(
            IDataReader<IpFlow>[] readers,
            IDataWriter<HostContext>[] writers,
            IEnricher<TimeRange<IpHostContext>, TimeRange<IpHostContextWithTags>> enricher,
            IRefiner<TimeRange<IpHostContextWithTags>, HostContext> refiner,
            TimeSpan windowSpan,
            IHostBasedFilter filter,
            ILogger? logger,
            CancellationToken cancellationToken)
        {
            int inputFlowsCounter = 0;
            int contextCreatedCounter = 0;
            int contextWrittenCounter = 0;
            int windowsClosedCounter = 0;
            int flowsAcceptedCounter = 0;
            int flowsDeniedCounter = 0;
            int flowsDroppedCounter = 0;
            int windowContextCreatedCounter = 0;

            logger?.LogInformation("Starting pipeline...");
            // CREATING BLOCKS:
            var inputBlock = new BufferBlock<IpFlow>(new DataflowBlockOptions { BoundedCapacity = BufferSize });

            var keyInputBlock = new TransformManyBlock<IpFlow, Timestamped<KeyValuePair<IPAddress, IpFlow>>>(f => BiflowKeySelector(f), new ExecutionDataflowBlockOptions { BoundedCapacity = BufferSize });

            var windowBlock = new WindowGroupByBlock<KeyValuePair<IPAddress, IpFlow>, IPAddress, IpFlow>(f => f.Key, x => x.Value, ()=> KeyValuePair.Create(IPAddress.Any, Enumerable.Empty<IpFlow>()), windowSpan, new ExecutionDataflowBlockOptions { BoundedCapacity = BufferSize });

            var contextBlock = new TransformBlock<Timestamped<IGrouping<IPAddress, IpFlow>>, TimeRange<IpHostContext>>(grp => BuildContext(grp), new ExecutionDataflowBlockOptions { BoundedCapacity = BufferSize });

            var enricherBlock = new TransformBlock<TimeRange<IpHostContext>, TimeRange<IpHostContextWithTags>>(ctx => enricher.Enrich(ctx), new ExecutionDataflowBlockOptions { BoundedCapacity = BufferSize });

            var refinerBlock = new TransformBlock<TimeRange<IpHostContextWithTags>, HostContext>(ctx => refiner.Refine(ctx), new ExecutionDataflowBlockOptions { BoundedCapacity = BufferSize, MaxDegreeOfParallelism = 4 });

            var writerBlock = new ActionBlock<HostContext>(ctx =>
            {
                foreach (var writer in writers)
                {
                    writer.OnNext(ctx);
                }
                contextWrittenCounter++;
                
            }, new ExecutionDataflowBlockOptions { BoundedCapacity = BufferSize });

            // LINKING:
            inputBlock.LinkTo(keyInputBlock, new DataflowLinkOptions { PropagateCompletion = true }); //, OnlyValidUnicastFlows);
            keyInputBlock.LinkTo(windowBlock, new DataflowLinkOptions { PropagateCompletion = true });
            windowBlock.LinkTo(contextBlock, new DataflowLinkOptions { PropagateCompletion = true });
            contextBlock.LinkTo(enricherBlock, new DataflowLinkOptions { PropagateCompletion = true });
            enricherBlock.LinkTo(refinerBlock, new DataflowLinkOptions { PropagateCompletion = true });
            refinerBlock.LinkTo(writerBlock, new DataflowLinkOptions { PropagateCompletion = true });

            s_meter.CreateObservableCounter<int>("ethanol.context_builder.flows_read", () => inputFlowsCounter , "flows", "Number of flows read from the input.");
            s_meter.CreateObservableCounter<int>("ethanol.context_builder.flows_accepted", () => flowsAcceptedCounter ,"flows", "Number of flows accepted by the flow filter.");
            s_meter.CreateObservableCounter<int>("ethanol.context_builder.flows_denied", () => flowsDeniedCounter, "flows", "Number of flows denied by the flow filter.");
            s_meter.CreateObservableCounter<int>("ethanol.context_builder.flows_dropped", () => flowsDroppedCounter ,"flows", "Number of flows droped by the flow sampler.");            
            s_meter.CreateObservableCounter<int>("ethanol.context_builder.contexts_written", () => contextWrittenCounter, "contexts", "Number of context written.");
            s_meter.CreateObservableCounter<int>("ethanol.context_builder.contexts_created", () => contextCreatedCounter , "contexts", "Number of contexts created.");

            s_meter.CreateObservableGauge<int>("ethanol.context_builder.total_contexts_created", () => contextCreatedCounter , "contexts", "Number of contexts created.");      
            s_meter.CreateObservableGauge<int>("ethanol.context_builder.total_flows_read", () => inputFlowsCounter, "flows", "Number of windows closed.");
            s_meter.CreateObservableGauge<int>("ethanol.context_builder.total_flows_accepted", () => inputFlowsCounter, "flows", "Number of windows closed.");
            s_meter.CreateObservableGauge<int>("ethanol.context_builder.total_flows_denied", () => flowsDeniedCounter, "flows", "Number of windows closed.");
            s_meter.CreateObservableGauge<int>("ethanol.context_builder.total_flows_dropped", () => flowsDroppedCounter, "flows", "Number of windows closed.");
            s_meter.CreateObservableGauge<int>("ethanol.context_builder.total_context_written", () => contextWrittenCounter, "contexts", "Number of windows closed.");
            s_meter.CreateObservableGauge<int>("ethanol.context_builder.total_windows_closed", () => windowsClosedCounter, "windows", "Number of windows closed.");
            s_meter.CreateObservableGauge<int>("ethanol.context_builder.actual_window_hosts", () => windowBlock.KeyCount, "hosts", "Number of hosts currently collected in the active window.");
            s_meter.CreateObservableGauge<int>("ethanol.context_builder.actual_window_flows", () => windowBlock.ValueCount, "flows", "Number of flows currently collected in the active window.");
            s_meter.CreateObservableGauge<int>("ethanol.context_builder.input_buffer", ()=> inputBlock.Count, "flows", "Number of flows in the input buffer.");
            s_meter.CreateObservableGauge<int>("ethanol.context_builder.writer_buffer",()=> writerBlock.InputCount, "contexts", "Number of contexts in the writer buffer.");
            s_meter.CreateObservableGauge<int>("ethanol.context_builder.builder_buffer", () => contextBlock.InputCount, "contexts", "Number of contexts in the context buffer.");
            s_meter.CreateObservableGauge<int>("ethanol.context_builder.enricher_buffer", () => refinerBlock.InputCount, "contexts", "Number of contexts in the enricher buffer.");
            s_meter.CreateObservableGauge<int>("ethanol.context_builder.refiner_buffer", () => refinerBlock.InputCount, "contexts", "Number of contexts in the refiner buffer.");
            using var d = readers.Merge().Subscribe(observer =>
            {
                inputBlock.Post(observer);
                inputFlowsCounter++;
            },
            () =>
            {
                inputBlock.Complete();
                logger?.LogInformation("Input completed.");
            });

            var _readerTasks = readers.Select(x =>
            {
                logger?.LogInformation($"Starting input reader {x}.");
                return Task
                    .Run(async () => await x.ReadAllAsync(cancellationToken))
                    .ContinueWith(t => logger?.LogInformation($"Input reader {x} completed."));
            }).ToArray();

            logger?.LogInformation("Wait for completion.");
            await writerBlock.Completion.ContinueWith(t =>
            {
                foreach (var writer in writers)
                {
                    writer.OnCompleted();
                }
            });
            logger?.LogInformation("Pipeline completed.");

            TimeRange<IpHostContext> BuildContext(Timestamped<IGrouping<IPAddress, IpFlow>> grp)
            {
                contextCreatedCounter++;
                windowContextCreatedCounter++;
                if (IPAddress.Any.Equals(grp.Value.Key))
                {
                     logger?.LogInformation($"Window [{grp.Timestamp}, {grp.Timestamp + windowSpan}) collected, processing {windowContextCreatedCounter} contexts.");
                    windowContextCreatedCounter = 0;
                    windowsClosedCounter++;
                }
                return new TimeRange<IpHostContext>(new IpHostContext { HostAddress = grp.Value.Key, Flows = grp.Value.ToArray() }, grp.Timestamp.Ticks, grp.Timestamp.Ticks + windowSpan.Ticks);
            }
            IEnumerable<Timestamped<KeyValuePair<IPAddress, IpFlow>>> BiflowKeySelector(IpFlow flow)
            {
                var accepted = false;
                var src = flow.SourceAddress;
                var dst = flow.DestinationAddress;

                if (dst != null && filter.Match(dst))
                {
                    accepted = true;
                    yield return Timestamped.Create(KeyValuePair.Create(dst, flow), flow.TimeStart);
                }
                if (src != null && filter.Match(src))
                {
                    accepted = true;
                    yield return Timestamped.Create(KeyValuePair.Create(src, flow), flow.TimeStart);
                }
                if (accepted)
                {
                    flowsAcceptedCounter++;
                }
                else
                {
                    flowsDeniedCounter++;
                }
            }
        }

        /// <summary>
        /// Represents an asynchronous operation that can return a value.
        /// </summary>
        /// <typeparam name="TResult">The type of the result produced by the task.</typeparam>
        public static async Task ReactiveRunAsync(
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
                .Do(_ => { if (builderStats != null) builderStats.ReaderThreadId = Thread.CurrentThread.ManagedThreadId; })
                .Do(x => { if (builderStats != null) builderStats.LoadedFlows++; })
                .Where(flow => { if (OnlyValidUnicastFlows(flow)) { return true; } else { if (builderStats != null) builderStats.InvalidFlows++; return false; } })
                .Where(flow => { if (FilterFlows(filter, flow)) { if (builderStats != null) builderStats.AcceptedFlows++; return true; } else { if (builderStats != null) builderStats.DroppedFlows++; return false; } })
                .Select(f => new Timestamped<IpFlow>(f, f.TimeStart))
                .Do(f => { if (builderStats != null) builderStats.CurrentTimestamp = f.Timestamp.DateTime; })

                .ObserveOn(ThreadPoolScheduler.Instance)
                .Do(_ => { if (builderStats != null) builderStats.WindowerThreadId = Thread.CurrentThread.ManagedThreadId; })
                .VirtualWindow(windowSpan, windowShift)
                .Where(window => window.Value != null)                  // remove empty windows from the processing pipeline
                .Do(x => { if (builderStats != null) builderStats.ContextsCreated++; })                
                .Do(_ => { if (builderStats != null) builderStats.ProcessorThreadId = Thread.CurrentThread.ManagedThreadId; })

                .ObserveOn(ThreadPoolScheduler.Instance)          // This causes that the following code is executed in other thread.    
                .SelectMany(window =>
                        {
                            //if (builderStats != null) builderStats.CurrentWindowStart = window.StartTime;
                            return window.Value!
                                //.ObserveOn(ThreadPoolScheduler.Instance)
                                .AggregateIpContexts(window.StartTime.Ticks, window.EndTime.Ticks)
                                //.Do(ctx => { if (builderStats != null) builderStats.ContextsActive++; })
                                .Do(_ => { if (builderStats != null) builderStats.BuilderThreadId = Thread.CurrentThread.ManagedThreadId; })                      
                                .Where(ctx => ContextFilter(filter, ctx))
                                .Do(ctx => { if (builderStats != null) builderStats.ContextAccepted++; })
                                
                                //.ObserveOn(ThreadPoolScheduler.Instance)          // This causes that the following code is executed in other thread.          
                                .Do(_ => { if (builderStats != null) builderStats.EnricherThreadId = Thread.CurrentThread.ManagedThreadId; })
                                .Enrich(enricher).Where(x => x != null).Select(x => x!)
                                .Do(ctx => { if (builderStats != null) builderStats.ContextEnriched++; })

                                //.ObserveOn(ThreadPoolScheduler.Instance)          // This causes that the following code is executed in other thread.
                                .Do(_ => { if (builderStats != null) builderStats.RefinerThreadId = Thread.CurrentThread.ManagedThreadId; })
                                .Refine(refiner).Where(x => x != null).Select(x => x!)
                                .Do(ctx => { if (builderStats != null) builderStats.ContextCompacted++; });
                                
                        }
                )
                .Do(f => { if (builderStats != null) builderStats.ContextsWritten++; })
                
                .ObserveOn(ThreadPoolScheduler.Instance)
                .Do(_ => { if (builderStats != null) builderStats.WriterThreadId = Thread.CurrentThread.ManagedThreadId; })
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
        private static bool FilterAddress(IHostBasedFilter filter, IPAddress? adr)
        {
            if (adr == null) return false;
            return filter.Match(adr);
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
            
            /// <summary>
            /// Flows that are not valid for the contexts.
            /// </summary>
            public int InvalidFlows { get; internal set; }
            
            public int AcceptedFlows { get; internal set; }
            /// <summary>
            /// Gets or sets the number of dropped flows becasue they did not pass the input filter.
            /// </summary>
            public long DroppedFlows { get; set; }

            /// <summary>Gets or sets the number of windows created by the system for managing flows.</summary>
            public int ContextsCreated { get; set; }

            /// <summary>Gets or sets the current timestamp within the system.</summary>
            public DateTime CurrentTimestamp { get; set; }

            /// <summary>Gets or sets the number of contexts that are currently open. These contexts cannot be build yet as they still gather flows.</summary>
            public int ContextsActive => ContextsCreated - ContextAccepted;

            /// <summary>
            /// Gets or sets the number of contexts that were accepted for further processing.
            /// </summary>
            public int ContextAccepted { get; set; }
            /// <summary>Gets or sets the number of contexts that have been written to an output or storage.</summary>

            public int ContextEnriched { get; internal set; }
            public int ContextCompacted { get; internal set; }
            public int ContextsWritten { get; set; }


            public int ReaderThreadId { get; internal set; }
            public int WindowerThreadId { get; internal set; }
            public int ProcessorThreadId { get; internal set; }
            public int BuilderThreadId { get; internal set; }
            public int EnricherThreadId { get; internal set; }
            public int RefinerThreadId { get; internal set; }
            public int WriterThreadId { get; internal set; }
        }
    }

    public class BufferMonitor
    {
    }
}