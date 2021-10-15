using System;

namespace Ethanol.Streaming
{
    /// <summary>
    /// Represents an observable transformation that internally represents stream pipeline with a single entry stream and a single exit stream.
    /// </summary>
    /// <typeparam name="TInputPayload"></typeparam>
    /// <typeparam name="TOutputPayload"></typeparam>
    public class ObservableTransformStream<TInputPayload, TOutputPayload> : IObserver<TInputPayload>, IObservable<TOutputPayload>
    {
        ObservableEgressStream<TOutputPayload> _exitStream;
        ObservableIngressStream<TInputPayload> _entryStream;

        public ObservableTransformStream(ObservableIngressStream<TInputPayload> entryStream, ObservableEgressStream<TOutputPayload> exitStream)
        {
            this._exitStream = exitStream;
            this._entryStream = entryStream;
        }

        public void OnCompleted()
        {
            ((IObserver<TInputPayload>)_entryStream).OnCompleted();
        }

        public void OnError(Exception error)
        {
            ((IObserver<TInputPayload>)_entryStream).OnError(error);
        }

        public void OnNext(TInputPayload value)
        {
            ((IObserver<TInputPayload>)_entryStream).OnNext(value);
        }

        public IDisposable Subscribe(IObserver<TOutputPayload> observer)
        {
            return ((IObservable<TOutputPayload>)_exitStream).Subscribe(observer);
        }
    }
}
