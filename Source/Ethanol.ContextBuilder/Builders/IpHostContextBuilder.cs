using Ethanol.ContextBuilder.Context;
using Ethanol.ContextBuilder.Observable;
using Ethanol.ContextBuilder.Pipeline;
using Ethanol.ContextBuilder.Plugins.Attributes;
using Microsoft.StreamProcessing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using YamlDotNet.Serialization;

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
    /// <seealso cref="PluginAttribute" />
    [Plugin(PluginCategory.Builder, "IpHostContext", "Builds the context for IP hosts identified in the source IPFIX observable.")]
    public class IpHostContextBuilder : IObservableTransformer<IpFlow, ObservableEvent<IpHostContext>>, IPipelineNode
    {
        /// <summary>
        /// Gets the size of the window for aggregating IP flows.
        /// </summary>
        public TimeSpan WindowSize { get; }
        /// <summary>
        /// Gets the time interval to move the window forward in the IP flow stream.
        /// </summary>
        public TimeSpan WindowHop { get; }
        /// <summary>
        /// Gets the type of node this instance represents in a data processing pipeline.
        /// </summary>
        public PipelineNodeType NodeType => PipelineNodeType.Transformer;
        /// <summary>
        /// Gets the entity to which this instance is subscribed.
        /// </summary>
        public object SubscribedTo { get; private set; }

        private Subject<IpFlow> _ingressObservable;

        private Subject<ObservableEvent<IpHostContext>> _egressObservable;

        /// <summary>
        /// Represents the configuration options for the IpHostContextBuilder, including window size and hop duration.
        /// </summary>
        public class Configuration
        {
            [YamlMember(Alias = "window", Description = "The time span of window.")]
            public TimeSpan Window { get; set; } = TimeSpan.FromSeconds(60);

            [YamlMember(Alias = "hop", Description = "The time span of window hop.")]
            public TimeSpan Hop { get; set; } = TimeSpan.FromSeconds(30);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IpHostContextBuilder"/> class, specifying the window size and hop duration.
        /// </summary>
        /// <param name="windowSize">The size of the time window for aggregation.</param>
        /// <param name="windowHop">The interval for moving the time window forward.</param>
        public IpHostContextBuilder(TimeSpan windowSize, TimeSpan windowHop)
        {
            WindowSize = windowSize;
            WindowHop = windowHop;

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
        /// Creates an instance of the <see cref="IpHostContextBuilder"/> based on the provided configuration.
        /// </summary>
        /// <param name="configuration">The configuration options for the builder.</param>
        /// <returns>A new instance of the <see cref="IpHostContextBuilder"/>.</returns>
        [PluginCreate]
        internal static IObservableTransformer<IpFlow, object> Create(Configuration configuration)
        {
            return new IpHostContextBuilder(configuration.Window, configuration.Hop);
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
                var fhost = window.Payload.SelectMany(flow => 
                    (new[] 
                    { 
                        new KeyValuePair<string, IpFlow>(flow.SourceAddress.ToString(), flow), 
                        new KeyValuePair<string, IpFlow>(flow.DestinationAddress.ToString(), flow) 
                    }).ToObservable());
                return new ObservableEvent<IObservable<IGroupedObservable<string, IpFlow>>>(
                    fhost.GroupBy(flow => flow.Key, flow => flow.Value), 
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
