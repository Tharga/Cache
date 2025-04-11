using Tharga.Cache.Core;

namespace Tharga.Cache.Persist;

internal class MemoryWithRedis : IMemoryWithRedis, IAsyncDisposable, IDisposable
{
    private readonly IMemory _memory;
    private readonly IRedis _redis;

    public MemoryWithRedis(IMemory memory, IRedis redis, IManagedCacheMonitor cacheMonitor)
    {
        _memory = memory;
        _redis = redis;

        cacheMonitor.RequestEvictEvent += async (s, e) =>
        {
            await DropAsync(e.Type, e.Key);
            cacheMonitor.Drop(e.Type, e.Key);
        };
    }

    public void Dispose()
    {
        _redis?.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        if (_redis != null) await _redis.DisposeAsync();
    }

    public async Task<CacheItem<T>> GetAsync<T>(Key key)
    {
        var result = await _memory.GetAsync<T>(key);
        if (result != default) return result;

        return await _redis.GetAsync<T>(key);
    }

    public Task SetAsync<T>(Key key, CacheItem<T> item, bool staleWhileRevalidate)
    {
        var memoryTask = _memory.SetAsync(key, item, staleWhileRevalidate);
        var redisTask = _redis.SetAsync(key, item, staleWhileRevalidate);
        return Task.WhenAll(memoryTask, redisTask);
    }

    public async Task<bool> BuyMoreTime<T>(Key key)
    {
        var memoryTask = _memory.BuyMoreTime<T>(key);
        var redisTask = _redis.BuyMoreTime<T>(key);

        await Task.WhenAll(memoryTask, redisTask);
        return memoryTask.Result || redisTask.Result;
    }

    public async Task<bool> Invalidate<T>(Key key)
    {
        var memoryTask = _memory.Invalidate<T>(key);
        var redisTask = _redis.Invalidate<T>(key);

        await Task.WhenAll(memoryTask, redisTask);
        return memoryTask.Result || redisTask.Result;
    }

    public Task<bool> DropAsync<T>(Key key)
    {
        return DropAsync(typeof(T), key);
    }

    private async Task<bool> DropAsync(Type type, Key key)
    {
        var memoryTask = ((Memory)_memory).DropAsync(type, key);
        var redisTask = ((Redis)_redis).DropAsync(type, key);

        await Task.WhenAll(memoryTask, redisTask);
        //_cacheMonitor.Drop(type, key);
        return memoryTask.Result || redisTask.Result;
    }
}