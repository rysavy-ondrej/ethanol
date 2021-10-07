using Ethanol.Streaming;
using Microsoft.StreamProcessing;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace Ethanol.Demo
{
    public record Flow(string Proto, string SrcIp, int SrcPt, string DstIp, int DstPt);
    public record ContextFlow<TContext>(Flow Flow, TContext Context);
    public record ClassifiedContextFlow<TContext>(Flow Flow, string[] Tags, TContext Context);
    
    public static class ContextAggregator
    {
        public static IStreamable<Empty, ContextFlow<Target>> MergeContextFlowStreams<Source1,Source2,Target>(
            this IStreamable<Empty, ContextFlow<Source1>> source1, 
            IStreamable<Empty, ContextFlow<Source2>> source2,
            Func<Source1, Source2, Target> mergeFunc) 
            where Source1 : class where Source2 : class
        {
            return source1.LeftOuterJoin(source2,
                left => left.Flow,
                right => right.Flow,
                left => new ContextFlow<Target>(left.Flow, mergeFunc(left.Context, null)),
                //right => new ContextFlow<Target>(right.Flow, mergeFunc(null, right.Context)),
                (left, right) => new ContextFlow<Target>(left.Flow, mergeFunc(left.Context, right.Context))
                );
        }
        public static IStreamable<Empty, ContextFlow<Target>> AutoMergeContextFlowStreams<Source1, Source2, Target>(
            this IStreamable<Empty, ContextFlow<Source1>> source1,
            IStreamable<Empty, ContextFlow<Source2>> source2)
            where Source1 : class where Source2 : class
        {
            // get mergeFunc:
            Func<Source1,Source2,Target> mergeFunc = (Source1 x, Source2 y) => (Target)Activator.CreateInstance(typeof(Target), x, y);
            return source1.MergeContextFlowStreams(source2, mergeFunc);
        }

        record ContextPair<TLeft,TRight>(Flow Flow, TLeft Left, TRight Right);
        /// <summary>
        /// Merges three context streams. The first stream is a "master" stream, while the other streams are
        /// "slave" streams. The method uses LeftOuterJoin internally.
        /// </summary>
        /// <typeparam name="Source1"></typeparam>
        /// <typeparam name="Source2"></typeparam>
        /// <typeparam name="Source3"></typeparam>
        /// <typeparam name="Target"></typeparam>
        /// <param name="source1"></param>
        /// <param name="source2"></param>
        /// <param name="source3"></param>
        /// <param name="mergeFunc"></param>
        /// <returns></returns>
        public static IStreamable<Empty, ContextFlow<Target>> MergeContextFlowStreams<Source1, Source2, Source3, Target>(
            this IStreamable<Empty, ContextFlow<Source1>> source1,
            IStreamable<Empty, ContextFlow<Source2>> source2,
            IStreamable<Empty, ContextFlow<Source3>> source3,
            Func<Source1, Source2, Source3, Target> mergeFunc)
            where Source1 : class where Source2 : class where Source3 : class 
        {
            var merged = source1.Multicast(master =>
            {
                var left = master.Join(source2, left => left.Flow, right => right.Flow, (left, right) => new ContextPair<Source1, Source2>(left.Flow, left.Context, right.Context));
                var right = master.Join(source3, left => left.Flow, right => right.Flow, (left, right) => new ContextPair<Source1, Source3>(left.Flow, left.Context, right.Context));
                return left.Join(right, left => left.Flow, right => right.Flow,(left, right) => new ContextFlow<Target>(left.Flow, mergeFunc(left.Left, left.Right, right.Right)));
            });
            //
            // The problem here is that it generates a lot of intermediate results :(
            // Perhaps the problem is because of using Collect aggregates for generating the objects.
            // TODO: Get the biggest joined item
            return merged;
        }
    }
}
