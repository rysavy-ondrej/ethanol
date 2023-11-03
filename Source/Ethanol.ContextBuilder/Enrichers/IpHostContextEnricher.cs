using Ethanol.ContextBuilder.Context;
using Ethanol.ContextBuilder.Enrichers.TagObjects;
using Ethanol.ContextBuilder.Enrichers.TagProviders;
using Ethanol.ContextBuilder.Observable;
using Ethanol.ContextBuilder.Pipeline;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reactive.Subjects;
using System.Threading.Tasks;

namespace Ethanol.ContextBuilder.Enrichers
{
    /// <summary>
    /// Enriches the context of IP hosts by applying additional data from provided data sources.
    /// </summary>
    public class IpHostContextEnricher : IObservableTransformer<ObservableEvent<IpHostContext>, ObservableEvent<RawHostContext>>, IPipelineNode
    {
        static long cacheSize = 1024;
        static TimeSpan cacheExpiration = TimeSpan.FromMinutes(15);

        static ILogger _logger = LogManager.GetCurrentClassLogger();
        /// Represents a subject that can both observe items of type <see cref="ObservableEvent<RawHostContext>"/> 
        /// as well as produce them. This is often used to represent both the source and terminator of an observable sequence.
        private readonly Subject<ObservableEvent<RawHostContext>> _subject;

        /// Provides a mechanism for querying tag data. It can be used to retrieve or fetch tags associated with certain data or context.
        private readonly ITagDataProvider<TagObject> _tagQueryable;

        /// Represents a mechanism for signaling the completion of some asynchronous operation. 
        /// This provides a way to manually control the lifetime of a Task, signaling its completion.
        private TaskCompletionSource _tcs = new TaskCompletionSource();

        /// <summary>
        /// Initializes a new instance of the <see cref="IpHostContextEnricher"/> class.
        /// </summary>
        /// <param name="tagQueryable">Provides querying capabilities for tags.</param>
        public IpHostContextEnricher(ITagDataProvider<TagObject> tagQueryable)
        {
            _subject = new Subject<ObservableEvent<RawHostContext>>();
            _tagQueryable = new CachedTagDataProvider<TagObject>(tagQueryable, cacheSize,cacheExpiration);
        }

        /// <summary>
        /// Gets the type of the pipeline node, which in this case is Transformer.
        /// </summary>
        public PipelineNodeType NodeType => PipelineNodeType.Transformer;

        public Task Completed => _tcs.Task;

        /// <summary>
        /// Signals that the enrichment process has completed.
        /// </summary>
        public void OnCompleted()
        {
            _subject.OnCompleted();
            _tcs.SetResult();
        }
        /// <summary>
        /// Notifies observers that an exception has occurred.
        /// </summary>
        public void OnError(Exception error)
        {
            _subject.OnError(error);
        }
        /// <summary>
        /// Enriches the provided <see cref="IpHostContext"/> and notifies observers of the enriched context.
        /// </summary>
        public void OnNext(ObservableEvent<IpHostContext> value)
        {
            try
            {
                var tags = GetTags(value);
                _subject.OnNext(new ObservableEvent<RawHostContext>(new RawHostContext { HostAddress = value.Payload.HostAddress, Flows = value.Payload.Flows, Tags = tags.ToArray() }, value.StartTime, value.EndTime));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in context enrichment.", value);
                _subject.OnError(ex);
            }
        }

        /// <summary>
        /// Retrieves the tags associated with the provided <see cref="IpHostContext"/>.
        /// </summary>
        private IEnumerable<TagObject> GetTags(ObservableEvent<IpHostContext> value)
        {
            var host = value.Payload.HostAddress;
            var start = value.StartTime;
            var end = value.EndTime;
            var flows = value.Payload.Flows;
            var remoteHosts = flows.Select(x => x.GetRemoteAddress(host)).Distinct();
            if (_tagQueryable != null)
            {
                return GetRemoteTags(remoteHosts, start, end);
            }
            return Array.Empty<TagObject>();
        }
        /// <summary>
        /// Gets tags associated with the local host for a specified time range.
        /// </summary>
        private IEnumerable<TagObject> GetLocalTags(IPAddress host, DateTime start, DateTime end)
        {
            return _tagQueryable?.Get(host.ToString(), start, end) ?? Enumerable.Empty<TagObject>();
        }

        /// <summary>
        /// Retrieves tags associated with remote hosts for a specified time range.
        /// </summary>
        /// <remarks>
        /// This method takes an array of IpFlow objects and two DateTime values, and returns a dictionary
        /// where the key is a string representation of a remote address (from the IpFlow array), and the
        /// value is an array of NetifyApplication objects that match the given remote address and time range.
        /// If there are no matching NetifyApplication objects, an empty array is returned.
        /// </remarks>
        /// <param name="flows">The array of IpFlow objects.</param>
        /// <param name="start">The start time of the time range.</param>
        /// <param name="end">The end time of the time range.</param>
        /// <returns>A dictionary where the key is a string representation of a remote address (from the IpFlow array), and the
        /// value is an array of NetifyApplication objects that match the given remote address and time range.
        /// </returns>
        private IEnumerable<TagObject> GetRemoteTags(IEnumerable<IPAddress> remoteHosts, DateTime start, DateTime end)
        {                
            //return remoteHosts.SelectMany(addr => _tagQueryable?.Get(addr.ToString(), nameof(NetifyTag), start, end) ?? Enumerable.Empty<TagObject>());
            return _tagQueryable.GetMany(remoteHosts.Select(x=>x.ToString()), nameof(NetifyTag), start, end) ?? Enumerable.Empty<TagObject>();
        }

        /// <summary>
        /// Gets tags associated with specific IP flows for a given time range.
        /// </summary>
        private IEnumerable<TagObject> GetFlowTags(IEnumerable<IpFlow> flows, DateTime start, DateTime end)
        {
            return flows.SelectMany(flow => _tagQueryable?.Get(flow.FlowKey.ToString(), start, end) ?? Enumerable.Empty<TagObject>());
        }

        /// <summary>
        /// Subscribes an observer to the enriched context.
        /// </summary>
        /// <param name="observer">The observer that will receive notifications.</param>
        /// <returns>Disposable object representing the subscription.</returns>
        public IDisposable Subscribe(IObserver<ObservableEvent<RawHostContext>> observer)
        {
            return _subject.Subscribe(observer);
        }
    }
}
