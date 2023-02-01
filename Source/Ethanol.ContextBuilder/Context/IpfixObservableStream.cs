using Ethanol.Streaming;
using System;

namespace Ethanol.ContextBuilder.Context
{
    public class IpfixObservableStream : ObservableIngressStream<IpfixObject>
    {
        static long GetTimestamp(IpfixObject ipfixRecord)
        {
            return ipfixRecord.TimeStart.Ticks;
        }
        public IpfixObservableStream(TimeSpan windowSize, TimeSpan windowHop) : base(GetTimestamp, windowSize, windowHop)
        {
        }
    }
}
