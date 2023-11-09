using Ethanol.ContextBuilder.Context;
using Ethanol.ContextBuilder.Observable;
using Ethanol.ContextBuilder.Pipeline;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;

namespace Ethanol.ContextBuilder.Builders
{

    
    /// <summary>
    /// Builds the contextual information for IP hosts identified from the source IPFIX stream.
    /// </summary>
    /// <remarks>
    /// The <c>IpHostContextBuilder</c> class is an essential component that transforms raw IP flows 
    /// from IPFIX streams into a structured and contextual representation specific to IP hosts.
    ///
    /// The builder leverages the Observable pattern to continuously ingest input flows and process 
    /// them in real-time. This design allows for more scalable and efficient handling of large-scale 
    /// IPFIX data streams.
    ///
    /// The transformation process involves:
    /// - Wrapping each IP flow with metadata about its start and end times.
    /// - Segmenting the flow stream into defined time windows.
    /// - Grouping the flows by their source and destination addresses.
    ///
    /// The resulting observable provides a detailed view of grouped IP flows for every IP host within 
    /// the specified time windows.
    /// </remarks>
    /// <seealso cref="IObservableTransformer{TInput,TOutput}" />
    /// <seealso cref="IPipelineNode" />
    public class IpHostContextBuilder : IObservableTransformer<IpFlow, ObservableEvent<IpHostContext>>
    {
        /// <summary>
        /// Gets the size of the window for aggregating IP flows.
        /// </summary>
        public TimeSpan WindowSize { get; }
        /// <summary>
        /// Gets the time interval to move the window forward in the IP flow stream.
        /// </summary>
        public TimeSpan WindowHop { get; }
        public HostBasedFilter AddressFilter { get; private set; }

        /// <summary>
        /// Gets the type of node this instance represents in a data processing pipeline.
        /// </summary>
        public PipelineNodeType NodeType => PipelineNodeType.Transformer;
        /// <summary>
        /// Gets the entity to which this instance is subscribed.
        /// </summary>
        public object SubscribedTo { get; private set; }

        public Task Completed => _tcs.Task;

        private Subject<IpFlow> _ingressObservable;

        private Subject<ObservableEvent<IpHostContext>> _egressObservable;

        private TaskCompletionSource _tcs = new TaskCompletionSource();

        /// <summary>
        /// Initializes a new instance of the <see cref="IpHostContextBuilder"/> class, specifying the window size and hop duration.
        /// </summary>
        /// <param name="windowSize">The size of the time window for aggregation.</param>
        /// <param name="windowHop">The interval for moving the time window forward.</param>
        public IpHostContextBuilder(TimeSpan windowSize, TimeSpan windowHop, HostBasedFilter filter)
        {
            WindowSize = windowSize;
            WindowHop = windowHop;
            AddressFilter = filter;

            _ingressObservable = new Subject<IpFlow>();
            _egressObservable = new Subject<ObservableEvent<IpHostContext>>();

            // hardwire the pipeline...
            PublishContext(BuildHostFlowContext(_ingressObservable));
        }

        /// <summary>
        /// Consumes the computed context and publish it through output observable as <see cref="IpHostContext"/>.
        /// <para/>
        /// In fact it implements SelectMany on observable of windows. The result
        /// is an observable of context objects embedded within <see cref="ObservableEvent{TPayload}"/>. 
        /// </summary>
        /// <param name="observable">The input observable.</param>
        private void PublishContext(IObservable<ObservableEvent<IObservable<IGroupedObservable<string, IpFlow>>>> observable)
        {
            observable.Subscribe(
                // on next:
                window => window.Payload.Subscribe(
                    host =>
                    {
                        var flowList = new List<IpFlow>();
                        host.Subscribe(
                        // on next:
                        flow => flowList.Add(flow),
                        // on error:
                        error => { },
                        // on completed:
                        () => _egressObservable.OnNext(new ObservableEvent<IpHostContext>(new IpHostContext { HostAddress = IPAddress.Parse(host.Key), Flows = flowList.ToArray() }, window.StartTime, window.EndTime))
                        );
                    }
                ),
                // on error:
                _egressObservable.OnError,
                // on completed:
                _egressObservable.OnCompleted
            );
        }

        /// <summary>
        /// Builds the context for IP host flows by segmenting and grouping flows based on their IP addresses.
        /// </summary>
        /// <param name="source">The source observable of raw IP flows.</param>
        /// <returns>An observable of grouped IP flows, contextualized by IP host within specified time windows.</returns>
        private IObservable<ObservableEvent<IObservable<IGroupedObservable<string, IpFlow>>>> BuildHostFlowContext(IObservable<IpFlow> source)
        {
            var flowStream = source.Select(x => 
                new ObservableEvent<IpFlow>(x, x.TimeStart, x.TimeStart + x.TimeDuration));

            var windows = flowStream.HoppingWindow(WindowSize);
            return windows.Select(window =>
            {
                var flowEndpoints = window.Payload.SelectMany(flow => 
                    (new[] 
                    { 
                        new KeyValuePair<string, IpFlow>(flow.SourceAddress.ToString(), flow), 
                        new KeyValuePair<string, IpFlow>(flow.DestinationAddress.ToString(), flow) 
                    }).ToObservable());

                var filtered = flowEndpoints.Where(h => IPAddress.TryParse(h.Key, out var hostAdr) && AddressFilter.Match(hostAdr));

                return new ObservableEvent<IObservable<IGroupedObservable<string, IpFlow>>>(
                    filtered.GroupBy(flow => flow.Key, flow => flow.Value),
                    window.StartTime,
                    window.EndTime);
            });
        }

        /// <summary>
        /// Invoked when the observable source has completed producing data.
        /// </summary>
        public void OnCompleted()
        {
            _ingressObservable.OnCompleted();
            _tcs.SetResult();
        }

        /// <summary>
        /// Invoked when an error has occurred within the observable source.
        /// </summary>
        /// <param name="error">The exception that caused the error.</param>
        public void OnError(Exception error)
        {
            _ingressObservable.OnError(error);
        }

        /// <summary>
        /// Invoked when the observable source produces a new data item.
        /// </summary>
        /// <param name="value">The new data item.</param>
        public void OnNext(IpFlow value)
        {
            _ingressObservable.OnNext(value);
        }

        /// <summary>
        /// Subscribes an observer to the egress observable of the builder to receive aggregated IP host contexts.
        /// </summary>
        /// <param name="observer">The observer to subscribe.</param>
        /// <returns>A disposable object representing the subscription.</returns>
        public IDisposable Subscribe(IObserver<ObservableEvent<IpHostContext>> observer)
        {
            return _egressObservable.Subscribe(observer);
        }
    }
}
