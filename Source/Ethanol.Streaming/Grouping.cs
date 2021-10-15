using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Ethanol.Streaming
{
    internal record Grouping<TKey, TElement> : IGrouping<TKey, TElement>
    {
        private readonly TKey key;
        private readonly IEnumerable<TElement> values;

        public Grouping(TKey key, IEnumerable<TElement> values)
        {
            this.key = key ?? throw new ArgumentNullException(nameof(key));
            this.values = values ?? throw new ArgumentNullException(nameof(values));
        }

        public TKey Key
        {
            get { return key; }
        }

        public IEnumerator<TElement> GetEnumerator()
        {
            return values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
