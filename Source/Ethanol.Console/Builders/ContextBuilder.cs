using Elastic.Clients.Elasticsearch.Core.Reindex;
using Ethanol.ContextBuilder.Context;
using Ethanol.Streaming;
using Microsoft.StreamProcessing;
using System;
using System.Linq;
using System.Reactive.Linq;
namespace Ethanol.ContextBuilder.Builders
{

    public interface IContextBuilder<in TSource, out TTarget> : IObserver<TSource>, IObservable<TTarget>
    {

    }
    public abstract class ContextBuilder<TSource, TIntermediate, TTarget> : IContextBuilder<TSource, TTarget>
    {
        private ObservableIngressStream<TSource> _ingressStream;
        private ObservableEgressStream<TIntermediate> _egressStream;
        protected ContextBuilder(ObservableIngressStream<TSource> ingressStream)
        {
            _ingressStream = ingressStream;
            _egressStream = new ObservableEgressStream<TIntermediate>(BuildContext(_ingressStream));
        }

        protected abstract IStreamable<Empty, TIntermediate> BuildContext(IStreamable<Empty, TSource> source);

        public IDisposable Subscribe(IObserver<TTarget> observer)
        {
            return _egressStream.Where(x=>x.IsEnd).Select(GetTarget).Subscribe(observer);
        }

        protected abstract TTarget GetTarget(StreamEvent<TIntermediate> arg);

        public void OnCompleted()
        {
            _ingressStream.OnCompleted();
        }

        public void OnError(Exception error)
        {
            _ingressStream.OnError(error);
        }

        public void OnNext(TSource value)
        {
            _ingressStream.OnNext(value);
        }
    }

    public static class ContextBuilderFactory
    {
        public static IContextBuilder<IpfixRecord, object> GetBuilder(ModuleSpecification moduleSpecification)
        {
            switch(moduleSpecification?.Name)
            {
                case nameof(TlsFlowContextBuilder): return TlsFlowContextBuilder.Create(moduleSpecification.Parameters);
                case nameof(IpHostContextBuilder): return IpHostContextBuilder.Create(moduleSpecification.Parameters);
            }
            return null;
        }
    }
}
