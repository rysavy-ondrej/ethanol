using Ethanol.ContextBuilder.Context;
using Ethanol.ContextBuilder.Plugins;
using Ethanol.ContextBuilder.Plugins.Attributes;
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

        public abstract IStreamable<Empty, TIntermediate> BuildContext(IStreamable<Empty, TSource> source);

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



    /// <summary>
    /// Factory class supporting to instantiating flow readers.
    /// </summary>
    public class ContextBuilderFactory : PluginFactory<IContextBuilder<IpfixObject, object>>
    {
        protected override bool FilterPlugins((Type Type, PluginAttribute Plugin) plugin)
        {
            return plugin.Plugin.PluginType == PluginType.Builder;
        }
        static public ContextBuilderFactory Instance => new ContextBuilderFactory();
    }
}
