using System.Collections.Concurrent;

namespace Tharga.Cache.Persist;

internal class Memory : IMemory
{
    private readonly ConcurrentDictionary<string, CacheItem> _datas = new();

    public Task<CacheItem<T>> GetAsync<T>(Key key)
    {
        return Task.FromResult((CacheItem<T>)_datas.GetValueOrDefault(key));
    }

    public Task SetAsync<T>(Key key, T data, TimeSpan? freshSpan, bool staleWhileRevalidate)
    {
        var item = CacheItemBuilder.BuildCacheItem(data, freshSpan);
        _datas.AddOrUpdate(key, item, (_, _) => item);
        return Task.CompletedTask;
    }

    public Task<bool> BuyMoreTime(Key key)
    {
        if (_datas.TryGetValue(key, out var item))
        {
            var updatedItem = item with { UpdateTime = DateTime.UtcNow };
            var r = _datas.TryUpdate(key, updatedItem, item);
            return Task.FromResult(r);
        }

        return Task.FromResult(false);
    }

    public Task<bool> DropAsync<T>(Key key)
    {
        return Task.FromResult(_datas.TryRemove(key, out _));
    }

    public Task<(Key Key, CacheItem<T> Item)> DropFirst<T>()
    {
        var item = _datas.OrderBy(x => x.Value.CreateTime).First();
        _datas.TryRemove(item.Key, out var removed);
        return Task.FromResult<(Key Key, CacheItem<T> Item)>((item.Key, (CacheItem<T>)removed));
    }
}