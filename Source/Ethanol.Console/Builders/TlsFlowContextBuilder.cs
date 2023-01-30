using Ethanol.Catalogs;
using Ethanol.ContextBuilder.Context;
using Microsoft.StreamProcessing;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Ethanol.ContextBuilder.Builders
{
    public class TlsFlowContextBuilder : ContextBuilder<IpfixRecord, InternalContextFlow<TlsContext>, ContextFlow<TlsContext>>
    {

        public TlsFlowContextBuilder(TimeSpan windowSize, TimeSpan windowHop) : base(new IpfixObservableStream(windowSize, windowHop))
        {
        }

        internal static IContextBuilder<IpfixRecord, object> Create(IReadOnlyDictionary<string, string> attributes)
        {
            if (!attributes.TryGetValue("window", out var windowSize)) windowSize = "00:01:00";
            if (!attributes.TryGetValue("hop", out var windowHop))  windowHop = "00:00:30";

            if (!TimeSpan.TryParse(windowSize, out var windowSizeTimeSpan)) windowSizeTimeSpan = TimeSpan.FromSeconds(60);
            if (!TimeSpan.TryParse(windowHop, out var windowHopTimeSpan)) windowHopTimeSpan = TimeSpan.FromSeconds(30);
            return new TlsFlowContextBuilder(windowSizeTimeSpan, windowHopTimeSpan);
        }

        protected override IStreamable<Empty, InternalContextFlow<TlsContext>> BuildContext(IStreamable<Empty, IpfixRecord> source)
        {
            return TlsContextBuilder.BuildTlsContext(source);
        }

        protected override ContextFlow<TlsContext> GetTarget(StreamEvent<InternalContextFlow<TlsContext>> arg)
        {
            return new ContextFlow<TlsContext>(arg.Payload.FlowKey.ToString(), arg.Payload.FlowKey, WindowSpan.FromLong(arg.StartTime, arg.EndTime), arg.Payload.Context);
        }
    }
}
