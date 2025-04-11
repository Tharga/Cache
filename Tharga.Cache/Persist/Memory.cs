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

    public Task<bool> BuyMoreTime<T>(Key key)
    {
        return SetUpdateTimeAsync(key, DateTime.UtcNow);
    }

    public Task<bool> Invalidate<T>(Key key)
    {
        return SetUpdateTimeAsync(key, DateTime.MinValue);
    }

    public Task<bool> DropAsync<T>(Key key)
    {
        return Task.FromResult(_datas.TryRemove(key, out _));
    }

    private Task<bool> SetUpdateTimeAsync(Key key, DateTime updateTime)
    {
        if (_datas.TryGetValue(key, out var item))
        {
            var updatedItem = item with { UpdateTime = updateTime };
            var r = _datas.TryUpdate(key, updatedItem, item);
            return Task.FromResult(r);
        }

        return Task.FromResult(false);
    }
}