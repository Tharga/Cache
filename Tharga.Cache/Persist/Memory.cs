using System.Collections.Concurrent;
using Tharga.Cache.Core;

namespace Tharga.Cache.Persist;

internal class Memory : IMemory
{
    private readonly ConcurrentDictionary<string, CacheItem> _datas = new();

    public Memory(IManagedCacheMonitor cacheMonitor)
    {
        cacheMonitor.RequestEvictEvent += async (_, e) =>
        {
            await DropAsync(e.Key);
            cacheMonitor.Drop(e.Type, e.Key);
        };
    }

    public Task<CacheItem<T>> GetAsync<T>(Key key)
    {
        return Task.FromResult((CacheItem<T>)_datas.GetValueOrDefault(key));
    }

    public async IAsyncEnumerable<(Key Key, CacheItem<T> CacheItem)> FindAsync<T>(Key key)
    {
        var datas = _datas
            .Where(x => x.Value.GetType() == typeof(CacheItem<T>))
            .Where(x =>
                key.KeyParts
                    .All(kvp =>
                        x.Value.KeyParts.TryGetValue(kvp.Key, out var value) &&
                        value == kvp.Value
                    )
            );

        foreach (var data in datas)
        {
            yield return (data.Key, (CacheItem<T>)data.Value);
        }
    }

    public Task SetAsync<T>(Key key, CacheItem<T> item, bool staleWhileRevalidate)
    {
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

    public Task<bool> DropAsync(Key key)
    {
        var result = _datas.TryRemove(key, out _);
        return Task.FromResult(result);
    }

    public Task<(bool Success, string Message)> CanConnectAsync()
    {
        var cnt = _datas.Count;
        return Task.FromResult((true, $"There {(cnt == 1 ? "is" : "are")} {cnt} record{(cnt == 1 ? "" : "s")} cached."));
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