using Ethanol.ContextBuilder.Context;
using Ethanol.ContextBuilder.Enrichers.TagProviders;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

public class CachedTagDataProvider<TagDataType> : ITagDataProvider<TagDataType>
{
    private readonly ITagDataProvider<TagDataType> _dataProvider;
    private IMemoryCache _cache;

    public CachedTagDataProvider(ITagDataProvider<TagDataType> dataProvider, long sizeLimit, TimeSpan expirationScanFrequency)
    {
        this._dataProvider = dataProvider;
        var options = new MemoryCacheOptions()
        {
            ExpirationScanFrequency = expirationScanFrequency,
            SizeLimit = sizeLimit
        };
        _cache = new MemoryCache(options);
    }

    public IEnumerable<TagDataType> Get(string tagKey, DateTime start, DateTime end)
    {
        string cacheKey = $"Get_{tagKey}_{start}_{end}";
        return _cache.GetOrCreate<TagDataType[]>(cacheKey, entry =>
        {
            var data = _dataProvider.Get(tagKey, start, end).ToArray();
            entry.SetSize(1+data.Length);
            return data;
        });
    }

    public IEnumerable<TagDataType> Get(string tagKey, string tagType, DateTime start, DateTime end)
    {
        string cacheKey = $"Get_{tagKey}_{tagType}_{start}_{end}";
        return _cache.GetOrCreate<TagDataType[]>(cacheKey, entry =>
        {
            var data = _dataProvider.Get(tagKey, tagType, start, end).ToArray();
            entry.SetSize(1+data.Length);
            return data;
        });
    }
     
    public Task<IEnumerable<TagDataType>> GetAsync(string tagKey, DateTime start, DateTime end)
    {
        return Task.FromResult(Get(tagKey, start, end));
    }

    public Task<IEnumerable<TagDataType>> GetAsync(string tagKey, string tagType, DateTime start, DateTime end)
    {
        return Task.FromResult(Get(tagKey, tagType, start, end));
    }

    public IEnumerable<TagDataType> GetMany(IEnumerable<string> tagKeys, string tagType, DateTime start, DateTime end)
    {
        string cacheKey = $"GetMany_{String.Join("_", tagKeys)}_{tagType}_{start}_{end}";
        return _cache.GetOrCreate<TagDataType[]>(cacheKey, entry =>
        {
            var data = _dataProvider.GetMany(tagKeys, tagType, start, end).ToArray();
            entry.SetSize(1+data.Length);
            return data;
        });
    }
}
