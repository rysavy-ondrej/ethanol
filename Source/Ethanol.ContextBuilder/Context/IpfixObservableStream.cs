using Ethanol.Streaming;
using System;

namespace Ethanol.ContextBuilder.Context
{
    public class IpfixObservableStream : ObservableIngressStream<IpFlow>
    {
        static long GetTimestamp(IpFlow ipfixRecord)
        {
            return ipfixRecord.TimeStart.Ticks;
        }
        public IpfixObservableStream(TimeSpan windowSize, TimeSpan windowHop) : base(GetTimestamp, windowSize, windowHop)
        {
        }
    }
}
