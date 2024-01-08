using System;
using System.Threading.Tasks;

namespace Ethanol.ContextBuilder.Writers
{
    public interface IDataWriter<TRecord> : IObserver<TRecord>
    {
        Task Completed { get; }

        IPerformanceCounters Counters { get; }
    }
}
