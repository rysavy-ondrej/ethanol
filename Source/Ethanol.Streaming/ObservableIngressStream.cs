using Ethanol.Streaming;
using Microsoft.StreamProcessing;
using System;
using System.Reactive.Subjects;

namespace Ethanol.Streaming
{
    /// <summary>
    /// Represents observer that takes records and push them in the streamable. 
    /// It can be used as ingress stream for providing data to a complex stream pipeline.
    /// </summary>
    /// <typeparam name="TPayload">The type of payload./typeparam>
    public class ObservableIngressStream<TPayload> : IObserver<TPayload>, IStreamable<Empty, TPayload>, IDisposable 
    {
        Subject<TPayload> _subject;
        IStreamable<Empty, TPayload> _stream;
        public ObservableIngressStream(Func<TPayload,long> timeFunc, TimeSpan windowSize, TimeSpan windowHop)
        {
            _subject = new Subject<TPayload>();
            _stream = _subject.GetWindowedEventStream(timeFunc, windowSize, windowHop);
        }

        public StreamProperties<Empty, TPayload> Properties => _stream.Properties;

        public string ErrorMessages => _stream.ErrorMessages;

        public void Dispose()
        {
            ((IDisposable)_subject).Dispose();
        }

        public void OnCompleted()
        { 
            ((IObserver<TPayload>)_subject).OnCompleted();
        }

        public void OnError(Exception error)
        {
            ((IObserver<TPayload>)_subject).OnError(error);
        }

        public void OnNext(TPayload value)
        {
            ((IObserver<TPayload>)_subject).OnNext(value);
        }

        public IDisposable Subscribe(IStreamObserver<Empty, TPayload> observer)
        {
            return _stream.Subscribe(observer);
        }
    }
}
