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

    /// <summary>
    /// The abstract class for implementing context builder classes.
    /// </summary>
    /// <typeparam name="TSource">The type of source data.</typeparam>
    /// <typeparam name="TIntermediate">The type of intermediate data.</typeparam>
    /// <typeparam name="TTarget">The type of generated context.</typeparam>
    public abstract class ContextBuilder<TSource, TIntermediate, TTarget> : IObservableTransformer<TSource, TTarget>
    {
        private ObservableIngressStream<TSource> _ingressStream;
        private ObservableEgressStream<TIntermediate> _egressStream;
        protected ContextBuilder(ObservableIngressStream<TSource> ingressStream)
        {
            _ingressStream = ingressStream;
            _egressStream = new ObservableEgressStream<TIntermediate>(BuildContext(_ingressStream), e => e.IsEnd);
        }

        /// <summary>
        /// Implements the procedure of building the context for records from the given <paramref name="source"/> stream.
        /// </summary>
        /// <param name="source">The source stream.</param>
        /// <returns>The output stream.</returns>
        public abstract IStreamable<Empty, TIntermediate> BuildContext(IStreamable<Empty, TSource> source);

        /// <inheritdoc/>>
        public IDisposable Subscribe(IObserver<TTarget> observer)
        {
            return _egressStream.Select(GetTarget).Subscribe(observer);
        }

        /// <summary>
        /// Gets the target object from the intermediate data.
        /// </summary>
        /// <param name="arg">The intermediate object.</param>
        /// <returns>The resulting context object.</returns>
        protected abstract TTarget GetTarget(StreamEvent<TIntermediate> arg);

        /// <inheritdoc/>>
        public void OnCompleted()
        {
            _ingressStream.OnCompleted();
        }
        /// <inheritdoc/>>
        public void OnError(Exception error)
        {
            _ingressStream.OnError(error);
        }
        /// <inheritdoc/>>
        public void OnNext(TSource value)
        {
            _ingressStream.OnNext(value);
        }
    }



    /// <summary>
    /// Factory class supporting to instantiating flow readers.
    /// </summary>
    public class ContextBuilderFactory : PluginFactory<IObservableTransformer<IpFlow, object>>
    {
        /// <inheritdoc/>>
        protected override bool FilterPlugins((Type Type, PluginAttribute Plugin) plugin)
        {
            return plugin.Plugin.PluginType == PluginType.Builder;
        }

        /// <summary>
        /// Gets the singleton of the factory.
        /// </summary>
        static public ContextBuilderFactory Instance { get; } = new ContextBuilderFactory();
    }

}
