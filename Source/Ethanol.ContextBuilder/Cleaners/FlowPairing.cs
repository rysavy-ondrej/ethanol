using AutoMapper;
using Ethanol.ContextBuilder.Context;
using Ethanol.ContextBuilder.Pipeline;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;

namespace Ethanol.ContextBuilder.Cleaners
{
    /// <summary>
    /// Pair single flows to biflows. 
    /// <para/> 
    /// Use the timeout option to control how long to wait to reverse flow before the single flow is published.
    /// </summary>
    public class FlowPairing : IObservableTransformer<IpFlow, IpFlow>, IPipelineNode
    {
        private readonly Subject<IpFlow> _flowSubject;
        private readonly TimeSpan _flowTimeout;
        private readonly Cache<FlowKey, IpFlow> _flowDictionary;
        DateTime _currentTime = DateTime.MinValue;

        public FlowPairing(TimeSpan flowTimeout)
        {
            _flowSubject = new Subject<IpFlow>();
            _flowTimeout = flowTimeout;
            _flowDictionary = new Cache<FlowKey, IpFlow>();
        }

        public PipelineNodeType NodeType => PipelineNodeType.Transformer;

        public void OnCompleted()
        {
            foreach (var flow in _flowDictionary.CleanupExpiredItems(DateTime.MaxValue))
            {
                _flowSubject.OnNext(flow);
            }
            _flowSubject.OnCompleted();
        }

        public void OnError(Exception error)
        {
            _flowSubject.OnError(error);
        }


        public void OnNext(IpFlow flow)
        {
            var reverseFlow = _flowDictionary.GetAndRemove(flow.FlowKey.GetReverseFlowKey());
            if (reverseFlow != null)
            {
                var reqFlow = flow.FlowType == FlowType.RequestFlow ? flow : reverseFlow;
                var resFlow = flow.FlowType == FlowType.ResponseFlow ? flow : reverseFlow;

                var biflow = PairFlows(reqFlow, resFlow);
                _flowSubject.OnNext(biflow);
            }
            else
            {
                _flowDictionary.Set(flow.FlowKey, flow, flow.TimeStart + _flowTimeout);
            }
            if (flow.TimeStart > _currentTime) _currentTime = flow.TimeStart;
            foreach(var expiredFlow in _flowDictionary.CleanupExpiredItems(_currentTime))
            {
                _flowSubject.OnNext(expiredFlow);
            }
        }

        /// <summary>
        /// Pair flows to the resulting bidirectional flow respecting their extensions.
        /// <para/>
        /// This pairing always work as even if incomaptible flows are provided (eg., DNS and TLS) 
        /// the bidirectional IpFlow is produced.
        /// </summary>
        /// <param name="reqFlow">The request flow.</param>
        /// <param name="resFlow">The response flow.</param>
        /// <returns>Returns the bidirectional flow from the given pair of flows.</returns>
        private IpFlow PairFlows(IpFlow reqFlow, IpFlow resFlow)
        {
            return ((reqFlow, resFlow)) switch
            {
                (TlsFlow q, TlsFlow r) => FlowPairMapper.Map<TlsFlow>(new FlowPair<TlsFlow>(q, r)),
                (HttpFlow q, HttpFlow r) => FlowPairMapper.Map<HttpFlow>(new FlowPair<HttpFlow>(q, r)),
                (DnsFlow q, DnsFlow r) => FlowPairMapper.Map<DnsFlow>(new FlowPair<DnsFlow>(q, r)),
                (IpFlow q, IpFlow r) => FlowPairMapper.Map<IpFlow>(new FlowPair<IpFlow>(q, r)),
                _ => null
            };
        }

        public IDisposable Subscribe(IObserver<IpFlow> observer)
        {
            return _flowSubject.Subscribe(observer);
        }

        public class Cache<TKey, TValue> where TValue : class
        {
            private readonly SortedDictionary<DateTime, List<TKey>> _timeline = new SortedDictionary<DateTime, List<TKey>>();
            private readonly IDictionary<TKey, CacheItem<TValue>> _cache = new Dictionary<TKey, CacheItem<TValue>>();

            public TValue Get(TKey key)
            {
                // Check if the item is in the cache
                if (_cache.TryGetValue(key, out CacheItem<TValue> item))
                {
                    // Return the cached item
                    return item.Value;
                }
                // The item is not in the cache
                return null;
            }
            internal TValue GetAndRemove(TKey flowKey)
            {
                var item = Get(flowKey);
                if (item != null) _cache.Remove(flowKey);
                return item;
            }

            public void Set(TKey key, TValue value, DateTime expiresAt)
            {
                // Create a new cache item with the specified value and expiration time
                var item = new CacheItem<TValue>(value, expiresAt);

                if (_timeline.ContainsKey(expiresAt))
                {
                    _timeline[expiresAt].Add(key);
                }
                else
                {
                    _timeline.Add(expiresAt, new List<TKey>() { key });
                }
                // Add or update the item in the cache
                _cache[key] = item;
            }

            public IEnumerable<TValue> CleanupExpiredItems(DateTime currentDate)
            {
                var expiredItems = _timeline.TakeWhile(x => x.Key < currentDate).ToList();

                // Remove all expired items from the cache
                foreach (var expiredItem in expiredItems)
                {
                    var expDate = expiredItem.Key;
                    _timeline.Remove(expDate);
                    foreach (var expFlow in expiredItem.Value)
                    {
                        if (_cache.TryGetValue(expFlow, out CacheItem<TValue> item))
                        {
                            _cache.Remove(expFlow);
                            yield return item.Value;
                        }
                    }
                }
            }

            private class CacheItem<T>
            {
                public T Value { get; }
                public DateTime ExpiresAt { get; }

                public CacheItem(T value, DateTime expiresAt)
                {
                    Value = value;
                    ExpiresAt = expiresAt;
                }
            }
        }
    }

    public record FlowPair<TFlow>(TFlow ReqFlow, TFlow ResFlow) where TFlow : IpFlow;
    static class FlowPairMapper
    {
        static IMapper _mapper = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<FlowPair<IpFlow>, IpFlow>()
                .ForMember(d => d.FlowType, o => o.MapFrom(s => FlowType.BidirectionFlow))
                .ForMember(d => d.SentOctets, o => o.MapFrom(s => s.ReqFlow.SentOctets))
                .ForMember(d => d.DestinationAddress, o => o.MapFrom(s => s.ReqFlow.DestinationAddress))
                .ForMember(d => d.DestinationPort, o => o.MapFrom(s => s.ReqFlow.DestinationPort))
                .ForMember(d => d.SentPackets, o => o.MapFrom(s => s.ReqFlow.SentPackets))
                .ForMember(d => d.Protocol, o => o.MapFrom(s => s.ReqFlow.Protocol))
                .ForMember(d => d.ApplicationTag, o => o.MapFrom(s => s.ReqFlow.ApplicationTag))
                .ForMember(d => d.SourceAddress, o => o.MapFrom(s => s.ReqFlow.SourceAddress))
                .ForMember(d => d.SourcePort, o => o.MapFrom(s => s.ReqFlow.SourcePort))
                .ForMember(d => d.TimeStart, o => o.MapFrom(s => s.ReqFlow.TimeStart))
                .ForMember(d => d.TimeDuration, o => o.MapFrom(s => s.ReqFlow.TimeDuration))
                .ForMember(d => d.RecvPackets, o => o.MapFrom(s => s.ResFlow.SentPackets))
                .ForMember(d => d.RecvOctets, o => o.MapFrom(s => s.ResFlow.SentOctets));

            cfg.CreateMap<FlowPair<DnsFlow>, DnsFlow>()
                .ForMember(d => d.FlowType, o => o.MapFrom(s => FlowType.BidirectionFlow))
                .ForMember(d => d.SentOctets, o => o.MapFrom(s => s.ReqFlow.SentOctets))
                .ForMember(d => d.DestinationAddress, o => o.MapFrom(s => s.ReqFlow.DestinationAddress))
                .ForMember(d => d.DestinationPort, o => o.MapFrom(s => s.ReqFlow.DestinationPort))
                .ForMember(d => d.SentPackets, o => o.MapFrom(s => s.ReqFlow.SentPackets))
                .ForMember(d => d.Protocol, o => o.MapFrom(s => s.ReqFlow.Protocol))
                .ForMember(d => d.ApplicationTag, o => o.MapFrom(s => s.ReqFlow.ApplicationTag))
                .ForMember(d => d.SourceAddress, o => o.MapFrom(s => s.ReqFlow.SourceAddress))
                .ForMember(d => d.SourcePort, o => o.MapFrom(s => s.ReqFlow.SourcePort))
                .ForMember(d => d.TimeStart, o => o.MapFrom(s => s.ReqFlow.TimeStart))
                .ForMember(d => d.TimeDuration, o => o.MapFrom(s => s.ReqFlow.TimeDuration))
                .ForMember(d => d.RecvPackets, o => o.MapFrom(s => s.ResFlow.SentPackets))
                .ForMember(d => d.RecvOctets, o => o.MapFrom(s => s.ResFlow.SentOctets))

                .ForMember(d => d.Identifier, o => o.MapFrom(s => s.ResFlow.Identifier))
                .ForMember(d => d.QuestionCount, o => o.MapFrom(s => s.ResFlow.QuestionCount))
                .ForMember(d => d.AnswerCount, o => o.MapFrom(s => s.ResFlow.AnswerCount))
                .ForMember(d => d.AuthorityCount, o => o.MapFrom(s => s.ResFlow.AuthorityCount))
                .ForMember(d => d.AdditionalCount, o => o.MapFrom(s => s.ResFlow.AdditionalCount))
                .ForMember(d => d.ResponseType, o => o.MapFrom(s => s.ResFlow.ResponseType))
                .ForMember(d => d.ResponseClass, o => o.MapFrom(s => s.ResFlow.ResponseClass))
                .ForMember(d => d.ResponseTTL, o => o.MapFrom(s => s.ResFlow.ResponseTTL))
                .ForMember(d => d.ResponseName, o => o.MapFrom(s => s.ResFlow.ResponseName))
                .ForMember(d => d.ResponseCode, o => o.MapFrom(s => s.ResFlow.ResponseCode))
                .ForMember(d => d.ResponseData, o => o.MapFrom(s => s.ResFlow.ResponseData))
                .ForMember(d => d.QuestionType, o => o.MapFrom(s => s.ReqFlow.QuestionType))
                .ForMember(d => d.QuestionClass, o => o.MapFrom(s => s.ReqFlow.QuestionClass))
                .ForMember(d => d.QuestionName, o => o.MapFrom(s => s.ReqFlow.QuestionName))
                .ForMember(d => d.Flags, o => o.MapFrom(s => s.ReqFlow.Flags))
                .ForMember(d => d.Opcode, o => o.MapFrom(s => s.ReqFlow.Opcode))
                .ForMember(d => d.QueryResponseFlag, o => o.MapFrom(s => s.ReqFlow.QueryResponseFlag));

            cfg.CreateMap<FlowPair<HttpFlow>, HttpFlow>()
                .ForMember(d => d.FlowType, o => o.MapFrom(s => FlowType.BidirectionFlow))
                .ForMember(d => d.SentOctets, o => o.MapFrom(s => s.ReqFlow.SentOctets))
                .ForMember(d => d.DestinationAddress, o => o.MapFrom(s => s.ReqFlow.DestinationAddress))
                .ForMember(d => d.DestinationPort, o => o.MapFrom(s => s.ReqFlow.DestinationPort))
                .ForMember(d => d.SentPackets, o => o.MapFrom(s => s.ReqFlow.SentPackets))
                .ForMember(d => d.Protocol, o => o.MapFrom(s => s.ReqFlow.Protocol))
                .ForMember(d => d.ApplicationTag, o => o.MapFrom(s => s.ReqFlow.ApplicationTag))
                .ForMember(d => d.SourceAddress, o => o.MapFrom(s => s.ReqFlow.SourceAddress))
                .ForMember(d => d.SourcePort, o => o.MapFrom(s => s.ReqFlow.SourcePort))
                .ForMember(d => d.TimeStart, o => o.MapFrom(s => s.ReqFlow.TimeStart))
                .ForMember(d => d.TimeDuration, o => o.MapFrom(s => s.ReqFlow.TimeDuration))
                .ForMember(d => d.RecvPackets, o => o.MapFrom(s => s.ResFlow.SentPackets))
                .ForMember(d => d.RecvOctets, o => o.MapFrom(s => s.ResFlow.SentOctets))

                .ForMember(d => d.Hostname, o => o.MapFrom(s => s.ReqFlow.Hostname))
                .ForMember(d => d.Method, o => o.MapFrom(s => s.ReqFlow.Method))
                .ForMember(d => d.ResultCode, o => o.MapFrom(s => s.ResFlow.ResultCode))
                .ForMember(d => d.Url, o => o.MapFrom(s => s.ReqFlow.Url))
                .ForMember(d => d.OperatingSystem, o => o.MapFrom(s => s.ReqFlow.OperatingSystem))
                .ForMember(d => d.ApplicationInformation, o => o.MapFrom(s => s.ReqFlow.ApplicationInformation));
            
            cfg.CreateMap<FlowPair<TlsFlow>, TlsFlow>()
                .ForMember(d => d.FlowType, o => o.MapFrom(s => FlowType.BidirectionFlow))
                .ForMember(d => d.SentOctets, o => o.MapFrom(s => s.ReqFlow.SentOctets))
                .ForMember(d => d.DestinationAddress, o => o.MapFrom(s => s.ReqFlow.DestinationAddress))
                .ForMember(d => d.DestinationPort, o => o.MapFrom(s => s.ReqFlow.DestinationPort))
                .ForMember(d => d.SentPackets, o => o.MapFrom(s => s.ReqFlow.SentPackets))
                .ForMember(d => d.Protocol, o => o.MapFrom(s => s.ReqFlow.Protocol))
                .ForMember(d => d.ApplicationTag, o => o.MapFrom(s => s.ReqFlow.ApplicationTag))
                .ForMember(d => d.SourceAddress, o => o.MapFrom(s => s.ReqFlow.SourceAddress))
                .ForMember(d => d.SourcePort, o => o.MapFrom(s => s.ReqFlow.SourcePort))
                .ForMember(d => d.TimeStart, o => o.MapFrom(s => s.ReqFlow.TimeStart))
                .ForMember(d => d.TimeDuration, o => o.MapFrom(s => s.ReqFlow.TimeDuration))
                .ForMember(d => d.RecvPackets, o => o.MapFrom(s => s.ResFlow.SentPackets))
                .ForMember(d => d.RecvOctets, o => o.MapFrom(s => s.ResFlow.SentOctets))

                .ForMember(h => h.IssuerCommonName, o => o.MapFrom(s => s.ResFlow.IssuerCommonName))
                .ForMember(h => h.SubjectCommonName, o => o.MapFrom(s => s.ResFlow.SubjectCommonName))
                .ForMember(h => h.SubjectOrganisationName, o => o.MapFrom(s => s.ResFlow.SubjectOrganisationName))
                .ForMember(h => h.CertificateValidityFrom, o => o.MapFrom(s => s.ResFlow.CertificateValidityFrom))
                .ForMember(h => h.CertificateValidityTo, o => o.MapFrom(s => s.ResFlow.CertificateValidityTo))
                .ForMember(h => h.SignatureAlgorithm, o => o.MapFrom(s => s.ResFlow.SignatureAlgorithm))
                .ForMember(h => h.PublicKeyAlgorithm, o => o.MapFrom(s => s.ResFlow.PublicKeyAlgorithm))
                .ForMember(h => h.PublicKeyLength, o => o.MapFrom(s => s.ResFlow.PublicKeyLength))
                .ForMember(h => h.ClientVersion, o => o.MapFrom(s => s.ReqFlow.ClientVersion))
                .ForMember(h => h.CipherSuites, o => o.MapFrom(s => s.ReqFlow.CipherSuites))
                .ForMember(h => h.ClientRandomID, o => o.MapFrom(s => s.ReqFlow.ClientRandomID))
                .ForMember(h => h.ClientSessionID, o => o.MapFrom(s => s.ReqFlow.ClientSessionID))
                .ForMember(h => h.ExtensionTypes, o => o.MapFrom(s => s.ReqFlow.ExtensionTypes))
                .ForMember(h => h.ExtensionLengths, o => o.MapFrom(s => s.ReqFlow.ExtensionLengths))
                .ForMember(h => h.EllipticCurves, o => o.MapFrom(s => s.ReqFlow.EllipticCurves))
                .ForMember(h => h.EcPointFormats, o => o.MapFrom(s => s.ReqFlow.EcPointFormats))
                .ForMember(h => h.ClientKeyLength, o => o.MapFrom(s => s.ReqFlow.ClientKeyLength))
                .ForMember(h => h.JA3Fingerprint, o => o.MapFrom(s => s.ReqFlow.JA3Fingerprint))
                .ForMember(h => h.ContentType, o => o.MapFrom(s => s.ResFlow.ContentType))
                .ForMember(h => h.HandshakeType, o => o.MapFrom(s => s.ResFlow.HandshakeType))
                .ForMember(h => h.SetupTime, o => o.MapFrom(s => s.ResFlow.SetupTime))
                .ForMember(h => h.ServerVersion, o => o.MapFrom(s => s.ResFlow.ServerVersion))
                .ForMember(h => h.ServerRandomID, o => o.MapFrom(s => s.ResFlow.ServerRandomID))
                .ForMember(h => h.ServerSessionID, o => o.MapFrom(s => s.ResFlow.ServerSessionID))
                .ForMember(h => h.ServerCipherSuite, o => o.MapFrom(s => s.ResFlow.ServerCipherSuite))
                .ForMember(h => h.ApplicationLayerProtocolNegotiation, o => o.MapFrom(s => s.ReqFlow.ApplicationLayerProtocolNegotiation));
        }).CreateMapper();

        /// <summary>
        /// Maps the flow pair to single bidirectional flow.
        /// </summary>
        /// <param name="flowPair">The flow pair.</param>
        /// <returns>A bidirectional pair of flows.</returns>
        public static T Map<T>(FlowPair<T> flowPair) where T : IpFlow
        {
            return (T)_mapper.Map(flowPair, flowPair.GetType(), flowPair.ReqFlow.GetType());
        }
    }
}
