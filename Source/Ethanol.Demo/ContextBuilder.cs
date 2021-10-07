using Ethanol.Streaming;
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
        /// <summary>
        /// Transforms the source stream to a new stream by expanding individual elements of each source event.
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TElement"></typeparam>
        /// <typeparam name="TTarget"></typeparam>
        /// <param name="source"></param>
        /// <param name="getElements"></param>
        /// <param name="getFlowKey"></param>
        /// <param name="selector"></param>
        /// <returns></returns>
        public static IStreamable<Empty, TTarget> Expand<TSource, TElement, TTarget, TKey>(this IStreamable<Empty, TSource> source, Func<TSource, IEnumerable<TElement>> getElements, Func<TElement, TSource, TTarget> selector, Func<TTarget,TKey> getKey)
        {
            var target = source.SelectMany(record => getElements(record).Select(item => selector(item, record)));
            // need to group and select only the first one otherwise we have duplicities, WHY?
            return target.GroupApply(
                val => getKey(val),
                group => group.Aggregate(aggregate => aggregate.CollectSet(flow => flow)),
                (key, value) => value.First());
        }
    }   
}
