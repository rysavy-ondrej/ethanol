using System;
using System.Reactive.Subjects;

namespace Ethanol.Providers
{
    public class ArtifactObservableProvider<TRawRecord, TArtifact> : IArtifactObservableProvider<TRawRecord>, IObservable<TArtifact>, IDisposable
    {
        IArtifactMapper<TRawRecord,TArtifact> _mapper;
        private readonly Func<TRawRecord, bool> _filter;
        Subject<TArtifact> _subject;

        public ArtifactObservableProvider(IArtifactMapper<TRawRecord,TArtifact> mapper, Func<TRawRecord, bool> filter)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _filter = filter ?? throw new ArgumentNullException(nameof(_filter));
            _subject = new Subject<TArtifact>();
        }

        public Type ArtifactType => typeof(TArtifact);

        public bool Match(TRawRecord src)
        {
            return _filter(src);
        }

        public void Push(TRawRecord src)
        {
            var artifact = _mapper.Map(src);
            _subject.OnNext(artifact);    
        }

        public IDisposable Subscribe(IObserver<TArtifact> observer)
        {
            return ((IObservable<TArtifact>)_subject).Subscribe(observer);
        }

        public void Dispose()
        {
            _subject.Dispose();
        }

        public void Close()
        {
            _subject.OnCompleted();
        }
    }
}