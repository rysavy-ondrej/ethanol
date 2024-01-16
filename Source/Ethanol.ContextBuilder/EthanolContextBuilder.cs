using Ethanol.ContextBuilder.Enrichers;
using Ethanol.ContextBuilder.Filters;
using Ethanol.ContextBuilder.Refiners;
using Ethanol.ContextBuilder.Readers;
using Ethanol.ContextBuilder.Writers;
using Ethanol.DataObjects;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reactive;
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
        static Meter s_meter = new Meter("Ethanol.ContextBuilder");

        /// <summary>
        /// Gets or sets the buffer size for block in the processing pipeline. The default value is 20000.
        /// It also represents the maximum number of collected hosts in the window.
        /// </summary>
        public static int BufferSize { get; set; } = 20000;

        /// <summary>
        /// Gets or sets the batch size for processing.
        /// The pipeline processes contexts in batched. This property specifies the maximum number of contexts in a batch. The value can be used 
        /// to tune performance. The default value is 128.
        /// </summary>
        public static int BatchSize { get; set; } = 128;

        /// <summary>
        /// Runs the asynchronous pipeline.
        /// </summary>
        /// <param name="readers">The array of data readers.</param>
        /// <param name="writers">The array of data writers.</param>
        /// <param name="enricher">The enricher for adding tags to the context.</param>
        /// <param name="refiner">The refiner for refining the context.</param>
        /// <param name="windowSpan">The time span for each window.</param>
        /// <param name="filter">The host-based filter for accepting or denying flows.</param>
        /// <param name="logger">The optional logger for logging information.</param>
        /// <param name="cancellationToken">The cancellation token for cancelling the operation.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
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

            logger?.LogInformation("Starting pipeline...");
            // CREATING BLOCKS:
            var inputBlock = new BufferBlock<IpFlow>(new DataflowBlockOptions { BoundedCapacity = BufferSize });

            var keyInputBlock = new TransformManyBlock<IpFlow, Timestamped<KeyValuePair<IPAddress, IpFlow>>>(f => BiflowKeySelector(f), new ExecutionDataflowBlockOptions { BoundedCapacity = BufferSize });

            var windowBlock = new WindowGroupByBlock<KeyValuePair<IPAddress, IpFlow>, IPAddress, IpFlow>(f => f.Key, x => x.Value, windowSpan, BatchSize, new ExecutionDataflowBlockOptions { BoundedCapacity = BufferSize });

            var contextBlock = new TransformBlock<Batch<IGrouping<IPAddress, IpFlow>>, Batch<IpHostContext>>(grp => BuildContext(grp), new ExecutionDataflowBlockOptions { BoundedCapacity = BufferSize });

            var enricherBlock = new TransformBlock<Batch<IpHostContext>, Batch<IpHostContextWithTags>>(batch => EnrichContext(batch), new ExecutionDataflowBlockOptions { BoundedCapacity = BufferSize });

            var refinerBlock = new TransformBlock<Batch<IpHostContextWithTags>, Batch<HostContext>>(batch => RefineContext(batch), new ExecutionDataflowBlockOptions { BoundedCapacity = BufferSize, MaxDegreeOfParallelism = 4 });

            var writerBlock = new ActionBlock<Batch<HostContext>>(batch =>
            {
                foreach (var writer in writers)
                {
                    writer.OnNextBatch(batch.Items);
                    if (batch.Last)
                    {
                        writer.OnWindowClosed(batch.TickStart, batch.TickStart + batch.Duration);
                    }
                }
                contextWrittenCounter += batch.Items.Length;
                if (batch.Last)
                {
                    windowsClosedCounter++;
                }
            }, new ExecutionDataflowBlockOptions { BoundedCapacity = BufferSize });

            // LINKING:
            inputBlock.LinkTo(keyInputBlock, new DataflowLinkOptions { PropagateCompletion = true }); //, OnlyValidUnicastFlows);
            keyInputBlock.LinkTo(windowBlock, new DataflowLinkOptions { PropagateCompletion = true });
            windowBlock.LinkTo(contextBlock, new DataflowLinkOptions { PropagateCompletion = true });
            contextBlock.LinkTo(enricherBlock, new DataflowLinkOptions { PropagateCompletion = true });
            enricherBlock.LinkTo(refinerBlock, new DataflowLinkOptions { PropagateCompletion = true });
            refinerBlock.LinkTo(writerBlock, new DataflowLinkOptions { PropagateCompletion = true });

            s_meter.CreateObservableCounter<int>("ethanol.context_builder.flows_read", () => inputFlowsCounter, "flows", "Number of flows read from the input.");
            s_meter.CreateObservableCounter<int>("ethanol.context_builder.flows_accepted", () => flowsAcceptedCounter, "flows", "Number of flows accepted by the flow filter.");
            s_meter.CreateObservableCounter<int>("ethanol.context_builder.flows_denied", () => flowsDeniedCounter, "flows", "Number of flows denied by the flow filter.");
            s_meter.CreateObservableCounter<int>("ethanol.context_builder.flows_dropped", () => flowsDroppedCounter, "flows", "Number of flows droped by the flow sampler.");
            s_meter.CreateObservableCounter<int>("ethanol.context_builder.contexts_written", () => contextWrittenCounter, "contexts", "Number of context written.");
            s_meter.CreateObservableCounter<int>("ethanol.context_builder.contexts_created", () => contextCreatedCounter, "contexts", "Number of contexts created.");

            s_meter.CreateObservableGauge<int>("ethanol.context_builder.total_contexts_created", () => contextCreatedCounter, "contexts", "Number of contexts created.");
            s_meter.CreateObservableGauge<int>("ethanol.context_builder.total_contexts_written", () => contextWrittenCounter, "contexts", "Number of context written.");
            s_meter.CreateObservableGauge<int>("ethanol.context_builder.total_flows_read", () => inputFlowsCounter, "flows", "Number of windows closed.");
            s_meter.CreateObservableGauge<int>("ethanol.context_builder.total_flows_accepted", () => inputFlowsCounter, "flows", "Number of windows closed.");
            s_meter.CreateObservableGauge<int>("ethanol.context_builder.total_flows_denied", () => flowsDeniedCounter, "flows", "Number of windows closed.");
            s_meter.CreateObservableGauge<int>("ethanol.context_builder.total_flows_dropped", () => flowsDroppedCounter, "flows", "Number of windows closed.");

            s_meter.CreateObservableGauge<int>("ethanol.context_builder.total_windows_closed", () => windowsClosedCounter, "windows", "Number of windows closed.");
            s_meter.CreateObservableGauge<int>("ethanol.context_builder.actual_window_hosts", () => windowBlock.KeyCount, "hosts", "Number of hosts currently collected in the active window.");
            s_meter.CreateObservableGauge<int>("ethanol.context_builder.actual_window_flows", () => windowBlock.ValueCount, "flows", "Number of flows currently collected in the active window.");
            s_meter.CreateObservableGauge<int>("ethanol.context_builder.next_window_in", TimeUntilNextWindow, "seconds", "Number of seconds until the next window is created.");
            s_meter.CreateObservableGauge<int>("ethanol.context_builder.input_buffer", () => inputBlock.Count, "flows", "Number of flows in the input buffer.");
            s_meter.CreateObservableGauge<int>("ethanol.context_builder.writer_buffer", () => writerBlock.InputCount * BatchSize, "contexts", "Number of contexts in the writer buffer.");
            s_meter.CreateObservableGauge<int>("ethanol.context_builder.builder_buffer", () => contextBlock.InputCount * BatchSize, "contexts", "Number of contexts in the context buffer.");
            s_meter.CreateObservableGauge<int>("ethanol.context_builder.enricher_buffer", () => refinerBlock.InputCount * BatchSize, "contexts", "Number of contexts in the enricher buffer.");
            s_meter.CreateObservableGauge<int>("ethanol.context_builder.refiner_buffer", () => refinerBlock.InputCount * BatchSize, "contexts", "Number of contexts in the refiner buffer.");

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

            Batch<IpHostContext> BuildContext(Batch<IGrouping<IPAddress, IpFlow>> batch)
            {

                var contextBatch = new Batch<IpHostContext>(batch.Items.Select(x => new IpHostContext { HostAddress = x.Key, Flows = x.ToArray() }).ToArray(), batch.TickStart, batch.Duration, batch.Last);
                contextCreatedCounter += batch.Items.Length;
                return contextBatch;
            }
            Batch<IpHostContextWithTags> EnrichContext(Batch<IpHostContext> batch)
            {
                var items = batch.Items.Select(x => enricher.Enrich(new TimeRange<IpHostContext>(x, batch.TickStart, batch.TickStart + batch.Duration))).Where(x => x != null && x.Value != null).Select(x => x!.Value!);
                return new Batch<IpHostContextWithTags>(items.ToArray(), batch.TickStart, batch.Duration, batch.Last);
            }
            Batch<HostContext> RefineContext(Batch<IpHostContextWithTags> batch)
            {
                var items = batch.Items.Select(x => refiner.Refine(new TimeRange<IpHostContextWithTags>(x, batch.TickStart, batch.TickStart + batch.Duration))).Where(x => x != null).Select(x => x!);
                return new Batch<HostContext>(items.ToArray(), batch.TickStart, batch.Duration, batch.Last);
            }

            IEnumerable<Timestamped<KeyValuePair<IPAddress, IpFlow>>> BiflowKeySelector(IpFlow flow)
            {
                if (OnlyValidUnicastFlows(flow) == false)
                {
                    flowsDroppedCounter++;
                    yield break;
                }

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
            int TimeUntilNextWindow()
            {
                return (int)((windowBlock.NextWindowStart - windowBlock.CurrentTime)?.TotalSeconds ?? 0);
            }
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
    }
}