using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Ethanol.ContextBuilder.Context;
using Ethanol.ContextBuilder.Observable;

namespace Ethanol
{
    public class ContextTransformCatalog
    {
        private EthanolEnvironment ethanolEnvironment;

        public ContextTransformCatalog(EthanolEnvironment ethanolEnvironment)
        {
            this.ethanolEnvironment = ethanolEnvironment;
        }

        public EthanolEnvironment Environment => ethanolEnvironment;

        public IObservableTransformer<ObservableEvent<IpHostContext>, ObservableEvent<IpHostContextWithTags>> GetVoidEnricher()
        {
            return new ObservableTransformer<ObservableEvent<IpHostContext>, ObservableEvent<IpHostContextWithTags>>(item => new ObservableEvent<IpHostContextWithTags>(new IpHostContextWithTags { HostAddress = item.Payload?.HostAddress, Flows = item.Payload?.Flows, Tags = new TagObject[0] }, item.StartTime, item.EndTime));
        }
        public class ObservableTransformer<T, R> : IObservableTransformer<T, R>
        {
            Subject<R> output = new Subject<R>();
            private readonly Func<T, R> _transformer;

            public ObservableTransformer(Func<T,R> transformer)
            {
                _transformer = transformer;
            }

            public void OnCompleted()
            {
                output.OnCompleted();
            }

            public void OnError(Exception error)
            {
                output.OnError(error);
            }

            public void OnNext(T value)
            {
                output.OnNext(_transformer(value)); 
            }

            public IDisposable Subscribe(IObserver<R> observer)
            {
                return output.Subscribe(observer);
            }
        }
    }
}