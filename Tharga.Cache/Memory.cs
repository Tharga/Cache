using System.Collections.Concurrent;

namespace Tharga.Cache;

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
        var item = _datas.First();
        _datas.TryRemove(item.Key, out var removed);
        return (item.Key, removed);
    }
}