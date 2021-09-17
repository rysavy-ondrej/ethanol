using System;

namespace Ethanol.Providers
{
    public interface IArtifactObservableProvider<TRawRecord>
    {
        public bool Match(TRawRecord src);
        public void Push(TRawRecord src);
        void Close();

        public Type ArtifactType { get; }
    }
}