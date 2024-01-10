using Ethanol.DataObjects;
using Ethanol.ContextBuilder.Enrichers;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using System.Linq;
using System.Text;
using LiteDB;


public class LongHash
{
    private const ulong FnvPrime = 1099511628211;
    private const ulong OffsetBasis = 14695981039346656037;

    public static long Get64BitHashCode(string str)
    {
        ulong hash = OffsetBasis;
        foreach (byte b in Encoding.UTF8.GetBytes(str))
        {
            hash ^= b;
            hash *= FnvPrime;
        }
        return (long)hash;
    }
}

public class LiteDatabaseTagDataSource : ITagDataSource<TagObject>
{
    LiteDatabase _database;
    ILiteCollection<TagObject> _collection;
    public LiteDatabaseTagDataSource(string dbPath)
    {
        _database = new LiteDatabase(dbPath);        
        _collection = _database.GetCollection<TagObject>("tags");
    }

    public void Dispose()
    {
        _database.Dispose();
    }

    public IEnumerable<TagObject> Get(string tagKey, DateTime start, DateTime end)
    {
        return _collection.Find(x => x.Key == tagKey).Where(x=> x.StartTime >= start && x.EndTime <= end);
    }

    public IEnumerable<TagObject> Get(string tagKey, string tagType, DateTime start, DateTime end)
    {

        return _collection.Find(x => x.Key == tagKey && x.Type == tagType).Where(x => x.StartTime >= start && x.EndTime <= end);        
    }

    public Task<IEnumerable<TagObject>> GetAsync(string key, DateTime start, DateTime end)
    {
        return Task.FromResult(Get(key, start, end));
    }

    public Task<IEnumerable<TagObject>> GetAsync(string key, string tagType, DateTime start, DateTime end)
    {
        return Task.FromResult(Get(key, tagType, start, end));
    }

    public IEnumerable<TagObject> GetMany(IEnumerable<string> keys, string tagType, DateTime start, DateTime end)
    {        
        foreach (var key in keys)
            foreach(var result in Get(key, tagType, start, end))
                yield return result;            
    }
}
