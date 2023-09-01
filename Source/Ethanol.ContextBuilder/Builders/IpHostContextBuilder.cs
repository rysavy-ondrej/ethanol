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
    // TODO: Generate "compact" context using the following records:
    // Host { ha = 192.168.111.32}: 
    //   Flow { pr = UDP, sa = 192.168.111.32, sp = 54698, da = 192.168.111.1, dp = 53, ts = 11/9/2021 6:00:02 AM, te = 11/9/2021 6:00:02 AM, pkt = 1, byt = 77 }
    //   Flow { pr = UDP, sa = 192.168.111.32, sp = 50994, da = 192.168.111.1, dp = 53, ts = 11/9/2021 6:00:22 AM, te = 11/9/2021 6:00:22 AM, pkt = 1, byt = 69 }
    //   Flow { pr = ARP, sa = 192.168.111.1, sp = 0, da = 192.168.111.32, dp = 0, ts = 11/9/2021 6:00:02 AM, te = 11/9/ 2021 6:00:02 AM, pkt = 1, byt = 28 }
    //   WebReq { hurl = www.google.com }
    //   DnsMap { dn = www.google.com, ip = 134.25.53.5 }
    //   TlsCon { ... }
    //
    public record DnsMap(string dn, string ip);
    public record WebReq(string hurl);
    public record TlsCon(string ver, string sni, string scn, string icn, string ja3);
    public record Netflow(string pr, string sa, ushort sp, string da, ushort dp, DateTime ts, DateTime te, int pkt, int byt);
    
    
    
    public class IpHostContext : IpHostContext<Empty>
    {
    }
    
    public class IpHostContext<TagType>
    { 
        public IPAddress HostAddress { get; init; }
        public IpFlow[] Flows { get; init; }
        public TagType Tags { get; init; }
        /// <summary>
        /// Gets flows of type <typeparamref name="TFlow"/> using <paramref name="select"/> function. 
        /// </summary>
        /// <typeparam name="TResult">The type of result.</typeparam>
        /// <typeparam name="TFlow">The type of flows to retrieve.</typeparam>
        /// <param name="select">The result mapping function.</param>
        /// <returns>Get the enumerablw of flow object created using <paramref name="select"/> funciton.</returns>
        public IEnumerable<TResult> GetFlowsAs<TResult, TFlow>(Func<TFlow, TResult> select) where TFlow : IpFlow
            => Flows.Where(f => f is TFlow).Select(f => select(f as TFlow));
    }
    /// <summary>
    /// Builds the context for Ip hosts identified in the source IPFIX stream.
    /// <para/>
    /// This implementation directly use Observable contrary to <see cref="HostContextBuilder"/> which is based on Streamable.
    /// </summary>
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
