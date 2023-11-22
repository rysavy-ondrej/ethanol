using Ethanol.ContextBuilder.Aggregators;
using Ethanol.ContextBuilder.Context;
using Ethanol.ContextBuilder.Observable;
using Ethanol.ContextBuilder.Pipeline;
using Ethanol.ContextBuilder.Polishers;
using Ethanol.ContextBuilder.Readers;
using Ethanol.ContextBuilder.Writers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Provides static methods for building and orchestrating a data processing pipeline in the Ethanol context.
/// </summary>
public static class EthanolContextBuilder
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
        return IPAddress.TryParse(ipString, out var address) ? address : IPAddress.None;
    }

    /// <summary>
    /// Transforms a sequence of IpFlow events into a sequence of context objects based on IP addresses.
    /// </summary>
    /// <typeparam name="TContext">The type of context object to be produced.</typeparam>
    /// <param name="source">The source observable sequence of IpFlow events.</param>
    /// <param name="resultSelector">A function to transform grouped IpFlow events into a context object.</param>
    /// <returns>An observable sequence of context objects.</returns>

    public static IObservable<ObservableEvent<TContext>> IpHostContext<TContext>(this IObservable<ObservableEvent<IObservable<IpFlow>>> source,
        Func<KeyValuePair<IPAddress, IpFlow[]>, TContext> resultSelector)
    {
        // duplicates flows so that they can be assigned to both end points:
        KeyValuePair<IPAddress, IpFlow>[] GetKey(IpFlow flow)
        {
            return new[] { new KeyValuePair<IPAddress, IpFlow>(flow.SourceAddress, flow), new KeyValuePair<IPAddress, IpFlow>(flow.DestinationAddress, flow) };
        }
        return source.HostContext(GetKey, resultSelector);
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
        Func<KeyValuePair<TKey, TFlow[]>, TContext> resultSelector)
    {
        return source.SelectMany(window =>
            window.Payload
                .SelectMany(keySelector)
                .GroupByAggregate(k => k.Key, v => v.Value, g => new ObservableEvent<TContext>(resultSelector(g), window.StartTime, window.EndTime))
            );
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

        IDataWriter<ObservableEvent<IpTargetHostContext>>[] Writers,

        IObservableTransformer<ObservableEvent<IpHostContext>, ObservableEvent<IpHostContextWithTags>> Enricher,

        IObservableTransformer<ObservableEvent<IpHostContextWithTags>, ObservableEvent<IpTargetHostContext>> Refiner);


    /// <summary>
    /// Orchestrates the entire data processing pipeline using the provided builder modules.
    /// </summary>
    /// <param name="modules">The set of builder modules including readers, writers, enricher, and refiner.</param>
    /// <param name="windowSpan">The time span for the hopping window operation.</param>
    /// <param name="filter">A host-based filter to apply to the processing pipeline.</param>
    /// <returns>A task representing the asynchronous operation of the pipeline.</returns>
    public static async Task Run(BuilderModules modules, TimeSpan windowSpan, int inputOrderingQueueLength, IHostBasedFilter filter, IObserver<BuilderStatistics> progressReport = null)
    {

        int flowsLoadedCount = 0;
        int flowsConsumedCount = 0;
        int windowsCreatedCount = 0;
        int contextsCreatedCount = 0;
        int contextsWrittenCount = 0;

        DateTime? currentWindowStartTime = null;
        IEnumerable<ObservableEvent<IpHostContext>> AddPeriod(ObservableEvent<IpHostContext> evt)
        {
            if (currentWindowStartTime is null) currentWindowStartTime = evt.StartTime;

            // if context in the new window is observed, we generate the "window period":
            if (evt.StartTime > currentWindowStartTime) 
            {                
                yield return new ObservableEvent<IpHostContext>(new IpHostContext { HostAddress = IPAddress.Any, Flows = Array.Empty<IpFlow>() }, currentWindowStartTime.Value, currentWindowStartTime.Value + windowSpan);
                currentWindowStartTime = evt.StartTime;
            }

            yield return evt;
        }

        var sequencer = new SequencerTransformer<IpFlow>(inputOrderingQueueLength);
        var windowTransformer = new HoppingWindowAggregator<IpFlow>(windowSpan, null);

        BuilderStatistics CreateReport(long delta)
        {
            return new BuilderStatistics {
                LoadedFlows = flowsLoadedCount,
                ConsumedFlows = flowsConsumedCount,
                CreatedWindows = windowsCreatedCount,
                ContextsBuilt = contextsCreatedCount,
                ContextsWritten = contextsWrittenCount,
                CurrentWindowStart = windowTransformer.Statistics.CurrentWindowStart,
                CurrentTimestamp = windowTransformer.Statistics.CurrentTimestamp,
                DroppedFlowsInAllWindows = windowTransformer.Statistics.OutOfOrderFlowsTotal,
                DroppedFlowInCurrentWindow = windowTransformer.Statistics.OutOfOrderFlowsInCurrentWindow,
                FlowsInCurrentWindow = windowTransformer.Statistics.FlowsInCurrentWindow,
                TotalFlowsInAllWindows = windowTransformer.Statistics.FlowsTotal,
                MaxFlowBufferSize = sequencer.Statistics.MaxQueueLength,
                BufferedFlows = sequencer.Statistics.ActualQueueLength
            };
        }

        var progressReportSubscription = (progressReport is not null) ? Observable.Interval(TimeSpan.FromSeconds(10)).Select(CreateReport).Subscribe(progressReport) : null;

        var task = Produce(modules.Readers)

                 .Select(t => new ObservableEvent<IpFlow>(t, t.TimeStart, t.TimeStart + t.TimeDuration))

                 .Do(_ => flowsLoadedCount++)

                 .OrderSequence(sequencer)

                 .Do(_ => flowsConsumedCount++)

                 .HoppingWindow(windowTransformer)

                 .Do(_ => windowsCreatedCount++)

                 .IpHostContext(g => new IpHostContext { HostAddress = g.Key, Flows = g.Value })
                 
                 .Do(_ => contextsCreatedCount++)

                 .Where(filter.Evaluate)

                 .SelectMany(selector: AddPeriod)

                 .Transform(modules.Enricher)

                 .Transform(modules.Refiner)

                 .Do(_ => contextsWrittenCount++)

                 .Consume(modules.Writers);

        var _readerTasks = modules.Readers.Select(x => x.ReadAllAsync(CancellationToken.None)).ToArray();

        progressReportSubscription?.Dispose();
        await task;
    }

    /// <summary>
    /// Represents a progress report for the data processing pipeline.
    /// </summary>
    /// <remarks>
    /// This record provides a snapshot of the current state of the data processing pipeline, 
    /// giving insights into its overall progress and the volume of data it has handled. 
    /// The information encapsulated in this record can be used for monitoring, logging, 
    /// and diagnostic purposes to understand and optimize the pipeline's performance.
    /// </remarks>
    public record BuilderStatistics
    {
        public int LoadedFlows { get; set; }
        public int ConsumedFlows { get; set; }
        public int BufferedFlows { get; set; }
        public int MaxFlowBufferSize { get; set; }
        public int CreatedWindows { get; set; }
        public int ContextsBuilt { get; set; }
        public int ContextsWritten { get; set; }
        public DateTime CurrentWindowStart { get; set; }
        public DateTime CurrentTimestamp { get; set; }
        public int DroppedFlowsInAllWindows { get; set; }
        public int DroppedFlowInCurrentWindow { get; set; }
        public int FlowsInCurrentWindow { get; set; }
        public int TotalFlowsInAllWindows { get; set; }
    }
}
