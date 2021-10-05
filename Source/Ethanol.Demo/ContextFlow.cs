using Microsoft.StreamProcessing;
using System;

namespace Ethanol.Demo
{
    public record Flow(string Proto, string SrcIp, int SrcPt, string DstIp, int DstPt);
    public record ContextFlow<TContext>(Flow Flow, TContext Context);
    public record ClassifiedContextFlow<TContext>(Flow Flow, string[] Tags, TContext Context);
    /// <summary>
    /// Support aggregation of the context from multiple sources.
    /// It provides operators to get key, value and empty context for the given source type.
    /// </summary>
    /// <typeparam name="TSource">The type of the source context.</typeparam>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TValue">The type of the value of context information.</typeparam>
    public interface IContextAggregator<TSource, TKey, TValue>
    {
        /// <summary>
        /// Gets the key for the given context information. Most often it represents a key flow.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public TKey GetKey(ContextFlow<TSource> source);

        public TValue GetValue(ContextFlow<TSource> source);

        public TValue GetEmpty();
    }
    public class ContextAggregator<TSource, TKey, TValue> : IContextAggregator<TSource, TKey, TValue>
    {
        Func<TValue> _getEmpty;
        Func<ContextFlow<TSource>, TKey> _getKey;
        Func<ContextFlow<TSource>, TValue> _getValue;

        public ContextAggregator(Func<ContextFlow<TSource>, TKey> getKey, Func<ContextFlow<TSource>, TValue> getValue, Func<TValue> getEmpty)
        {
            _getKey = getKey ?? throw new ArgumentNullException(nameof(getKey));
            _getValue = getValue ?? throw new ArgumentNullException(nameof(getValue));
            _getEmpty = getEmpty ?? throw new ArgumentNullException(nameof(getEmpty));
        }

        public TValue GetEmpty() => _getEmpty();

        public TKey GetKey(ContextFlow<TSource> source) => _getKey(source);

        public TValue GetValue(ContextFlow<TSource> source) => _getValue(source);
    }

    public static class ContextAggregator
    {
        public static ContextFlow<Target> Merge<Source1,Target1,Source2,Target2,Target>
        (
            ContextFlow<Source1> source1, 
            IContextAggregator<Source1,Flow, Target1> contextAggregator1,
            ContextFlow<Source2> source2,
            IContextAggregator<Source2, Flow, Target2> contextAggregator2,
            Func<Target1, Target2, Target> getResult
        )
        {
            var t1 = source1 != null ? contextAggregator1.GetValue(source1) : contextAggregator1.GetEmpty();
            var t2 = source2 != null ? contextAggregator2.GetValue(source2) : contextAggregator2.GetEmpty();
            var keyFlow = source1 != null ? contextAggregator1.GetKey(source1) 
                        : source2 != null ? contextAggregator2.GetKey(source2) 
                        : null;
            return new ContextFlow<Target>(keyFlow, getResult(t1, t2));
        }
        public static ContextFlow<Target> Merge<Source1, Target1, Source2, Target2, Source3, Target3, Target>
        (
            ContextFlow<Source1> source1,
            IContextAggregator<Source1, Flow, Target1> contextAggregator1,
            ContextFlow<Source2> source2,
            IContextAggregator<Source2, Flow, Target2> contextAggregator2,
            ContextFlow<Source3> source3,
            IContextAggregator<Source3, Flow, Target3> contextAggregator3,
            Func<Target1, Target2, Target3, Target> getResult
        )
        {
            var t1 = source1 != null ? contextAggregator1.GetValue(source1) : contextAggregator1.GetEmpty();
            var t2 = source2 != null ? contextAggregator2.GetValue(source2) : contextAggregator2.GetEmpty();
            var t3 = source3 != null ? contextAggregator3.GetValue(source3) : contextAggregator3.GetEmpty();
            var keyFlow = source1 != null ? contextAggregator1.GetKey(source1) 
                        : source2 != null ? contextAggregator2.GetKey(source2)
                        : source3 != null ? contextAggregator3.GetKey(source3)
                        : null;
            return new ContextFlow<Target>(keyFlow, getResult(t1, t2, t3));
        }

        public static ContextFlow<Target> Merge<Source1, Target1, Source2, Target2, Source3, Target3, Source4, Target4, Target>
        (
            ContextFlow<Source1> source1,
            IContextAggregator<Source1, Flow, Target1> contextAggregator1,
            ContextFlow<Source2> source2,
            IContextAggregator<Source2, Flow, Target2> contextAggregator2,
            ContextFlow<Source3> source3,
            IContextAggregator<Source3, Flow, Target3> contextAggregator3,
            ContextFlow<Source4> source4,
            IContextAggregator<Source4, Flow, Target4> contextAggregator4,
            Func<Target1, Target2, Target3, Target4, Target> getResult
        )
        {
            var t1 = source1 != null ? contextAggregator1.GetValue(source1) : contextAggregator1.GetEmpty();
            var t2 = source2 != null ? contextAggregator2.GetValue(source2) : contextAggregator2.GetEmpty();
            var t3 = source3 != null ? contextAggregator3.GetValue(source3) : contextAggregator3.GetEmpty();
            var t4 = source4 != null ? contextAggregator4.GetValue(source4) : contextAggregator4.GetEmpty();
            var keyFlow = source1 != null ? contextAggregator1.GetKey(source1)
                        : source2 != null ? contextAggregator2.GetKey(source2)
                        : source3 != null ? contextAggregator3.GetKey(source3)
                        : source4 != null ? contextAggregator4.GetKey(source4)
                        : null;
            return new ContextFlow<Target>(keyFlow, getResult(t1, t2, t3, t4));
        }

        public static IStreamable<Empty, ContextFlow<Target>> MergeContextFlowStreams<Source1,Source2,Target>(
            Func<Source1, Source2, Target> mergeFunc, 
            IStreamable<Empty, ContextFlow<Source1>> source1, 
            IStreamable<Empty, ContextFlow<Source2>> source2) 
            where Source1 : class where Source2 : class
        {
            return source1.FullOuterJoin(source2,
                left => left.Flow,
                right => right.Flow,
                left => new ContextFlow<Target>(left.Flow, mergeFunc(left.Context, null)),
                right => new ContextFlow<Target>(right.Flow, mergeFunc(null, right.Context)),
                (left, right) => new ContextFlow<Target>(left.Flow, mergeFunc(left.Context, right.Context))
                );
        }

        record ContextPair<TLeft,TRight>(Flow Flow, TLeft Left, TRight Right);
        public static IStreamable<Empty, ContextFlow<Target>> MergeContextFlowStreams<Source1, Source2, Source3, Target>(
            Func<Source1, Source2, Source3, Target> mergeFunc,
            IStreamable<Empty, ContextFlow<Source1>> source1,
            IStreamable<Empty, ContextFlow<Source2>> source2,
            IStreamable<Empty, ContextFlow<Source3>> source3)
            where Source1 : class where Source2 : class where Source3 : class
        {
            return source1
                .FullOuterJoin(source2,
                    left => left.Flow,
                    right => right.Flow,
                    left => new ContextPair<Source1, Source2>(left.Flow, left.Context, null),
                    right => new ContextPair<Source1, Source2>(right.Flow, null, right.Context),
                    (left, right) => new ContextPair<Source1, Source2>(left.Flow, left.Context,right.Context)
                )
                .FullOuterJoin(source3,
                    left => left.Flow,
                    right => right.Flow,
                    left => new ContextFlow<Target>(left.Flow, mergeFunc(left.Left, left.Right, null)),
                    right => new ContextFlow<Target>(right.Flow, mergeFunc(null, null, right.Context)),
                    (left, right) => new ContextFlow<Target>(left.Flow, mergeFunc(left.Left, left.Right, right.Context))
                );

        }
    }
}
