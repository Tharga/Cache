using StackExchange.Redis;
using System.Collections.Concurrent;

namespace Tharga.Cache.Persist;

internal class Redis : IRedis
{
    private readonly IDatabase _db;

    public Redis(/*IConnectionMultiplexer redis*/)
    {
        //_db = redis.GetDatabase();
    }

    public async Task<CacheItem> GetAsync<T>(Key key)
    {
        //var value = await _db.StringGetAsync((string)key);
        //return value.HasValue
        //    ? JsonSerializer.Deserialize<T>(value!)
        //    : default;
        throw new NotImplementedException();
    }

    public Task SetAsync<T>(Key key, T data, TimeSpan freshSpan)
    {
        throw new NotImplementedException();
    }

    public Task<CacheItem> DropAsync<T>(Key key)
    {
        throw new NotImplementedException();
    }

    public IAsyncEnumerable<CacheItem> GetAsync<T>()
    {
        throw new NotImplementedException();
    }

    public Task<(Key Key, CacheItem Item)> DropFirst()
    {
        throw new NotImplementedException();
    }
}

/*
builder.Services.AddSingleton<IRedisCacheService, RedisCacheService>();

public class RedisCacheService : IRedisCacheService
   {
       private readonly IDatabase _db;

       public RedisCacheService(IConnectionMultiplexer redis)
       {
           _db = redis.GetDatabase();
       }

       public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
       {
           var json = JsonSerializer.Serialize(value);
           await _db.StringSetAsync(key, json, expiration);
       }

       public async Task<T?> GetAsync<T>(string key)
       {
           var value = await _db.StringGetAsync(key);
           return value.HasValue
               ? JsonSerializer.Deserialize<T>(value!)
               : default;
       }

       public async Task RemoveAsync(string key)
       {
           await _db.KeyDeleteAsync(key);
       }
   }

 */

internal class Memory : IMemory
{
    private readonly ConcurrentDictionary<string, CacheItem> _datas = new();

    public async Task<CacheItem> GetAsync<T>(Key key)
    {
        return _datas.GetValueOrDefault(key);
    }

    public Task SetAsync<T>(Key key, T data, TimeSpan freshSpan)
    {
        var item = new CacheItem(data, freshSpan);
        _datas.AddOrUpdate(key, item, (_, _) => item);
        return Task.CompletedTask;
    }

    public async Task<CacheItem> DropAsync<T>(Key key)
    {
        if (_datas.TryRemove(key, out var val)) return val;
        return null;
    }

    public IAsyncEnumerable<CacheItem> GetAsync<T>()
    {
        throw new NotImplementedException();
    }

    public async Task<(Key Key, CacheItem Item)> DropFirst()
    {
        var item = _datas.OrderBy(x => x.Value.CreateTime).First();
        _datas.TryRemove(item.Key, out var removed);
        return (item.Key, removed);
    }
}