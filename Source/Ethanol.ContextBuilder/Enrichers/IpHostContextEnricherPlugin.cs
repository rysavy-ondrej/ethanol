using Ethanol.ContextBuilder.Context;
using Ethanol.ContextBuilder.Observable;
using Ethanol.ContextBuilder.Plugins.Attributes;
using System;

namespace Ethanol.ContextBuilder.Enrichers
{
    /// <summary>
    /// Enrich the computed context with additional known information.
    /// </summary>
    [Plugin(PluginCategory.Enricher, "IpHostContextEnricher", "Enriches the context for IP hosts from the provided data.")]
    public class IpHostContextEnricherPlugin : IObservableTransformer
    {
        private IpHostContextEnricher _enricher;

        public IpHostContextEnricherPlugin(IpHostContextEnricher enricher)
        {
            _enricher = enricher;
        }

        public string TransformerName => nameof(IpHostContextEnricherPlugin);

        public Type SourceType => typeof(ObservableEvent<IpHostContext>);

        public Type TargetType => typeof(ObservableEvent<RawHostContext>);

        [PluginCreate]
        internal static IObservableTransformer Create(EnricherConfiguration hostTagConfiguraiton)
        {
            ITagDataProvider<TagObject> tagsProvider = hostTagConfiguraiton.GetTagProvider();

            var enricher = new IpHostContextEnricher(tagsProvider);
            return new IpHostContextEnricherPlugin(enricher);
        }

        public void OnCompleted()
        {
            _enricher.OnCompleted();
        }

        public void OnError(Exception error)
        {
            _enricher.OnError(error);
        }

        public void OnNext(object value)
        {
            _enricher.OnNext((ObservableEvent<IpHostContext>)value);
        }

        public IDisposable Subscribe(IObserver<object> observer)
        {
            return _enricher.Subscribe(observer);
        }
    }
}
