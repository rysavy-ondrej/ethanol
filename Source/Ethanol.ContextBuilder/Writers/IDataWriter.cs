using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ethanol.ContextBuilder.Writers
{
    public interface IDataWriter<TRecord> : IObserver<TRecord>
    {
        Task Completed { get; }
        void OnNextBatch(IEnumerable<TRecord> record);
    }
}
