using Microsoft.StreamProcessing;
using System;
using System.Reactive.Linq;

namespace Ethanol.Streaming
{
    /// <summary>
    /// Represents observable for the given stream provided during object creation. 
    /// The events visible in observable can be filtered by the provided event filter.
    /// </summary>
    /// <typeparam name="TPayload">The type of payload.</typeparam>
    public class ObservableEgressStream<TPayload> : IObservable<StreamEvent<TPayload>>
    {
        IObservable<StreamEvent<TPayload>> _observable;
           
        public ObservableEgressStream(IStreamable<Empty,TPayload> stream, Func<StreamEvent<TPayload>, bool> eventFilter)
        {
            _observable = stream.ToStreamEventObservable().Where(eventFilter);
        }

        public IDisposable Subscribe(IObserver<StreamEvent<TPayload>> observer)
        {
            return _observable.Subscribe(observer);
        }
    }
}
