using Ethanol.Streaming;
using System;

namespace Ethanol.ContextBuilder.Context
{
    public class IpfixObservableStream : ObservableIngressStream<IpfixRecord>
    {
        static long GetTimestamp(IpfixRecord ipfixRecord)
        {
            return ipfixRecord.TimeStart.Ticks;
        }
        public IpfixObservableStream(TimeSpan windowSize, TimeSpan windowHop) : base(GetTimestamp, windowSize, windowHop)
        {
        }
    }
}
