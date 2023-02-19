using Ethanol.ContextBuilder.Context;
using Ethanol.ContextBuilder.Observable;
using Ethanol.ContextBuilder.Plugins.Attributes;
using Ethanol.Streaming;
using Microsoft.Extensions.Hosting;
using Microsoft.StreamProcessing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace Ethanol.ContextBuilder.Builders
{

    public record IpHostContext(IPAddress HostAddress, IpFlow[] Flows)
    {
        IEnumerable<TlsConnection> TlsConnections = Flows.Where(f => f is TlsFlow).Select(f => TlsConnection.Create(f as TlsFlow));

        IEnumerable<DnsResolution> DomainResolutions = Flows.Where(f => f is DnsFlow).Select(f => DnsResolution.Create(f as DnsFlow));
    }
    /// <summary>
    /// Builds the context for Ip hosts identified in the source IPFIX stream.
    /// </summary>
    [Plugin(PluginType.Builder, "IpHostContext", "Builds the context for IP hosts identified in the source IPFIX observable.")]
    public class IpHostContextBuilder : IContextBuilder<IpFlow, ObservableEvent<IpHostContext>>
    {
        public TimeSpan WindowSize { get; }
        public TimeSpan WindowHop { get; }
        
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
        /// Consumes the computed context and publis it through output observable as <see cref="IpHostContext"/>.
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
                        () => _egressObservable.OnNext(new ObservableEvent<IpHostContext>(new IpHostContext(IPAddress.Parse(host.Key), flowList.ToArray()), window.StartTime, window.EndTime))
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
        internal static IContextBuilder<IpFlow, object> Create(Configuration configuration)
        {
            return new IpHostContextBuilder(configuration.Window, configuration.Hop);
        }



        /// <summary>
        /// Collects flows to hosts within each window.
        /// </summary>
        /// <param name="source">The source observable of <see cref="IpFlow"/> objects.</param>
        /// <returns>The observable of windows within flows are grouped to hosts.</returns>
        private IObservable<ObservableEvent<IObservable<IGroupedObservable<string, IpFlow>>>> BuildHostFlowContext(IObservable<IpFlow> source)
        {
            var flowStream = source.Select(x => new ObservableEvent<IpFlow>(x, x.TimeStart, x.TimeStart + x.TimeDuration));
            var windows = flowStream.WindowHop(WindowSize);
            return windows.Select(window =>
            {
                var fhost = window.Payload.SelectMany(flow => (new[] { new KeyValuePair<string, IpFlow>(flow.SourceAddress.ToString(), flow), new KeyValuePair<string, IpFlow>(flow.DestinationAddress.ToString(), flow) }).ToObservable());
                return new ObservableEvent<IObservable<IGroupedObservable<string, IpFlow>>>(fhost.GroupBy(flow => flow.Key, flow => flow.Value), window.StartTime, window.EndTime);
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
