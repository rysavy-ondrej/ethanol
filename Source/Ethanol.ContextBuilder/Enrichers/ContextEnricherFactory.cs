using Ethanol.ContextBuilder.Context;
using Ethanol.ContextBuilder.Observable;
using Ethanol.ContextBuilder.Plugins;
using Ethanol.ContextBuilder.Plugins.Attributes;
using System;
using System.Reactive.Subjects;

namespace Ethanol.ContextBuilder.Enrichers
{
    public class ContextEnricherFactory : PluginFactory<IObservableTransformer<object, object>>
    {
        /// <inheritdoc/>>
        protected override bool FilterPlugins((Type Type, PluginAttribute Plugin) plugin)
        {
            return plugin.Plugin.PluginType == PluginType.Enricher;
        }

        /// <summary>
        /// Gets the singleton of the factory.
        /// </summary>
        static public ContextEnricherFactory Instance { get; } = new ContextEnricherFactory();
    }


    public class IdentityTransformer<TSource> : IObservableTransformer<TSource, TSource>
    {
        Subject<TSource> _subject = new Subject<TSource>();

        public void OnCompleted()
        {
            _subject.OnCompleted();
        }

        public void OnError(Exception error)
        {
            _subject.OnError(error);
        }

        public void OnNext(TSource value)
        {
            _subject.OnNext(value);
        }

        public IDisposable Subscribe(IObserver<TSource> observer)
        {
            return _subject.Subscribe(observer);
        }
    }
}
