using Microsoft.StreamProcessing;
using Microsoft.StreamProcessing.Aggregates;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;

namespace Ethanol.Streaming
{

    public static class StreamableAggregations
    {
        class _AggregateSet<TElement> : IAggregate<TElement, IImmutableSet<TElement>, TElement[]>
        {
            public Expression<Func<IImmutableSet<TElement>, long, TElement, IImmutableSet<TElement>>> Accumulate() => acc;
            private static Expression<Func<IImmutableSet<TElement>, long, TElement, IImmutableSet<TElement>>> acc =
                (state, timestamp, item) => state.Add(item);

            public Expression<Func<IImmutableSet<TElement>, TElement[]>> ComputeResult() => comp;
            private static Expression<Func<IImmutableSet<TElement>, TElement[]>> comp = l => l.ToArray();

            public Expression<Func<IImmutableSet<TElement>, long, TElement, IImmutableSet<TElement>>> Deaccumulate() => dec;
            private static Expression<Func<IImmutableSet<TElement>, long, TElement, IImmutableSet<TElement>>> dec =
                (state, timestamp, item) => state.Remove(item);

            public Expression<Func<IImmutableSet<TElement>, IImmutableSet<TElement>, IImmutableSet<TElement>>> Difference() => dif;
            private static Expression<Func<IImmutableSet<TElement>, IImmutableSet<TElement>, IImmutableSet<TElement>>> dif =
                (left, right) => left.Except(right);

            private readonly Expression<Func<IImmutableSet<TElement>>> initialState = () => ImmutableHashSet.Create<TElement>();
            public Expression<Func<IImmutableSet<TElement>>> InitialState() => initialState;

        }
        class _AggregateList<TElement> : IAggregate<TElement, IImmutableList<TElement>, TElement[]>
        {
            public Expression<Func<IImmutableList<TElement>, long, TElement, IImmutableList<TElement>>> Accumulate() =>
                (state, timestamp, item) => state.Add(item);

            public Expression<Func<IImmutableList<TElement>, TElement[]>> ComputeResult() => l => l.ToArray();

            public Expression<Func<IImmutableList<TElement>, long, TElement, IImmutableList<TElement>>> Deaccumulate() =>
                (state, timestamp, item) => state.Remove(item);

            public Expression<Func<IImmutableList<TElement>, IImmutableList<TElement>, IImmutableList<TElement>>> Difference() =>
                (left, right) => left.RemoveRange(right).ToImmutableList();

            public Expression<Func<IImmutableList<TElement>>> InitialState() => () => ImmutableList<TElement>.Empty;

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
        public static IAggregate<TSource, List<TResult>, TResult[]> CollectList<TKey,TSource,TResult>(
            this Window<TKey, TSource> window, Expression<Func<TSource, TResult>> selector)
        {
            var aggregate = new __AggregateList<TResult>();
            return aggregate.Wrap(selector);
        }
        public static IAggregate<TSource, IImmutableSet<TResult>, TResult[]> CollectSet<TKey, TSource, TResult>(
        this Window<TKey, TSource> window, Expression<Func<TSource, TResult>> selector)
        {
            var aggregate = new _AggregateSet<TResult>();
            return aggregate.Wrap(selector);
        }

        private class __AggregateList<TElement> : ListAggregateBase<TElement, TElement[]>
        {
            public override Expression<Func<List<TElement>, TElement[]>> ComputeResult() => l => l.ToArray();
        }
        internal abstract class ListAggregateBase<T, R> : IAggregate<T, List<T>, R>
        {
            private static readonly Expression<Func<List<T>>> init = () => new List<T>();
            public Expression<Func<List<T>>> InitialState() => init;

            public Expression<Func<List<T>, long, T, List<T>>> Accumulate()
            {
                Expression<Action<List<T>, long, T>> temp = (set, timestamp, input) => set.Add(input);
                var block = Expression.Block(temp.Body, temp.Parameters[0]);
                return Expression.Lambda<Func<List<T>, long, T, List<T>>>(block, temp.Parameters);
            }

            public Expression<Func<List<T>, long, T, List<T>>> Deaccumulate()
            {
                Expression<Action<List<T>, long, T>> temp = (set, timestamp, input) => set.Remove(input);
                var block = Expression.Block(temp.Body, temp.Parameters[0]);
                return Expression.Lambda<Func<List<T>, long, T, List<T>>>(block, temp.Parameters);
            }

            public Expression<Func<List<T>, List<T>, List<T>>> Difference() => (leftSet, rightSet) => SetExcept(leftSet, rightSet);

            private static List<T> SetExcept(List<T> left, List<T> right)
            {
                var newList = new List<T>(left);
                foreach (var t in right) newList.Remove(t);
                return newList;
            }

            public abstract Expression<Func<List<T>, R>> ComputeResult();
        }
    }
}
