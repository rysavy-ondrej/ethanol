using Microsoft.StreamProcessing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

namespace Ethanol.Demo
{

    public class ContextBuilder
    {
    }


    public static class StremableContextFlowOperators
    {
        public static Flow GetFlow(this RawIpfixRecord f)
        {
            return new Flow(f.Protocol, f.SrcIp, f.SrcPort, f.DstIp, f.DstPort);
        }
        public static IStreamable<Empty, ContextFlow<Target>> AsContextFlow<Key, Source, Target>(this IStreamable<Empty, KeyValuePair<Key, IEnumerable<Source>>> source, Func<Source, Flow> toKey, Func<Key, IEnumerable<Source>, Target> toValue)
        {
            return source.SelectMany(x => x.Value.Select(f => new ContextFlow<Target>(toKey(f), toValue(x.Key, x.Value))));
        }
    }   
}
