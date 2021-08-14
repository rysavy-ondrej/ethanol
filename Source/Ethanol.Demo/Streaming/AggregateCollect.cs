using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.StreamProcessing.Aggregates;
using System.Linq.Expressions;
using Microsoft.StreamProcessing;
using System.Collections.Immutable;

namespace Ethanol.Demo.Streaming
{
    public static class AggregateCollect
    {
        public static List<TElement> AddItem<TElement>(List<TElement> list, TElement x)
        {
            list.Add(x);
            return list;
        }
        public static List<TElement> RemoveItem<TElement>(List<TElement> list, TElement x)
        {
            list.Remove(x);
            return list;
        }
        class _Aggregate<TElement> : IAggregate<TElement, ImmutableList<TElement>, TElement[]>
        {
            public Expression<Func<ImmutableList<TElement>, long, TElement, ImmutableList<TElement>>> Accumulate() =>
                (state, timestamp, item) => state.Add(item);

            public Expression<Func<ImmutableList<TElement>, TElement[]>> ComputeResult() => l => l.ToArray();

            public Expression<Func<ImmutableList<TElement>, long, TElement, ImmutableList<TElement>>> Deaccumulate() =>
                (state, timestamp, item) => state.Remove(item);

            public Expression<Func<ImmutableList<TElement>, ImmutableList<TElement>, ImmutableList<TElement>>> Difference() =>
                (left, right) => left.RemoveRange(right);

            public Expression<Func<ImmutableList<TElement>>> InitialState() => () => ImmutableList<TElement>.Empty;

        }
        /// <summary>
        /// Collects objects in a streamable.
        /// </summary>
        /// <typeparam name="TKey">The key of the stream.</typeparam>
        /// <typeparam name="TSource">The type of source objects.</typeparam>
        /// <typeparam name="TResult">The type of results.</typeparam>
        /// <param name="window">The source window consisting of objects.</param>
        /// <param name="selector">Function to project from <typeparamref name="TSource"/> to <typeparamref name="TResult"/> objects.</param>
        /// <returns>An array of object collected in the given <paramref name="window"/>.</returns>
        public static IAggregate<TSource, ImmutableList<TResult>, TResult[]> Collect<TKey,TSource,TResult>(
            this Window<TKey, TSource> window, Expression<Func<TSource, TResult>> selector)
        {
            var aggregate = new _Aggregate<TResult>();
            return aggregate.Wrap(selector);
        }
    }
}
