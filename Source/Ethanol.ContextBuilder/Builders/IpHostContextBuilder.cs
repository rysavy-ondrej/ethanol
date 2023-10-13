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
        public TimeSpan WindowSize { get; }
        public TimeSpan WindowHop { get; }
        public PipelineNodeType NodeType => PipelineNodeType.Transformer;

        public object SubscribedTo { get; private set; }

        private Subject<IpFlow> _ingressObservable;

        private Subject<ObservableEvent<IpHostContext>> _egressObservable;

        public class Configuration
        {
            [YamlMember(Alias = "window", Description = "The time span of window.")]
            public TimeSpan Window { get; set; } = TimeSpan.FromSeconds(60);

            [YamlMember(Alias = "hop", Description = "The time span of window hop.")]
            public TimeSpan Hop { get; set; } = TimeSpan.FromSeconds(30);
        }

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

            var windows = flowStream.WindowHop(WindowSize);
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

        public void OnCompleted()
        {
            _ingressObservable.OnCompleted();
        }

        public void OnError(Exception error)
        {
            _ingressObservable.OnError(error);
        }

        public void OnNext(IpFlow value)
        {
            _ingressObservable.OnNext(value);
        }

        public IDisposable Subscribe(IObserver<ObservableEvent<IpHostContext>> observer)
        {
            return _egressObservable.Subscribe(observer);
        }
    }
}
