using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.StreamProcessing.Aggregates;
using System.Linq.Expressions;
using Microsoft.StreamProcessing;

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
        class _Aggregate<TElement> : IAggregate<TElement, List<TElement>, TElement[]>
        {
            public Expression<Func<List<TElement>, long, TElement, List<TElement>>> Accumulate() =>
                (state, timestamp, item) => AddItem(state, item);

            public Expression<Func<List<TElement>, TElement[]>> ComputeResult() => l => l.ToArray();

            public Expression<Func<List<TElement>, long, TElement, List<TElement>>> Deaccumulate() =>
                (state, timestamp, item) => RemoveItem(state, item);

            public Expression<Func<List<TElement>, List<TElement>, List<TElement>>> Difference()
            {
                throw new NotImplementedException();
            }

            public Expression<Func<List<TElement>>> InitialState() => () => new List<TElement>();

        }
        public static IAggregate<TSource, List<TResult>, TResult[]> CollectFlows<TKey,TSource,TResult>(
            this Window<TKey, TSource> window, Expression<Func<TSource, TResult>> selector)
        {
            var aggregate = new _Aggregate<TResult>();
            return aggregate.Wrap(selector);
        }
    }
}
