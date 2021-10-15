using Microsoft.StreamProcessing;
using System;
using System.Reactive.Linq;

namespace Ethanol.Streaming
{
    /// <summary>
    /// Represents observable for the given stream provide during object creation. 
    /// </summary>
    /// <typeparam name="TPayload">The type of payload.</typeparam>
    public class ObservableEgressStream<TPayload> : IObservable<StreamEvent<TPayload>>
    {
        IObservable<StreamEvent<TPayload>> _observable;
           
        public ObservableEgressStream(IStreamable<Empty,TPayload> stream)
        {
            _observable = stream.ToStreamEventObservable().Where(e=>e.IsData);
        }

        public IDisposable Subscribe(IObserver<StreamEvent<TPayload>> observer)
        {
            return _observable.Subscribe(observer);
        }
    }
}
