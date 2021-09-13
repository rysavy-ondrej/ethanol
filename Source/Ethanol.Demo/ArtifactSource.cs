using Ethanol.Providers;
using System;
using System.Reactive.Linq;

namespace Ethanol.Demo
{
    public class ArtifactSource<T> : IObservable<T> where T : IpfixArtifact
    {
        IObservable<T> _observable;

        public ArtifactSource()
        {
            _observable = Observable.FromEvent<T>(
                fsHandler => RecordFetched += fsHandler,
                fsHandler => RecordFetched -= fsHandler);
        }

        event Action<T> RecordFetched;

        public void LoadFrom(string filename)
        {
            foreach (var obj in CsvArtifactProvider<T>.LoadFrom(filename))
            {
                RecordFetched?.Invoke(obj);
            }
        }
        public IDisposable Subscribe(IObserver<T> observer)
        {
            return _observable.Subscribe(observer);
        }
    }
}
