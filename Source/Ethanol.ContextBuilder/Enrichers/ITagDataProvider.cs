using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ethanol.ContextBuilder.Enrichers
{
    public interface ITagDataProvider<T>
    {
        IEnumerable<TagRecord> Get(string tagKey, DateTime start, DateTime end);
        Task<IEnumerable<TagRecord>> GetAsync(string key, DateTime start, DateTime end);
    }
}