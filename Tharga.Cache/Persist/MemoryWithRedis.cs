namespace Tharga.Cache.Persist;

internal class MemoryWithRedis : IMemoryWithRedis, IAsyncDisposable, IDisposable
{
    private readonly IMemory _memory;
    private readonly IRedis _redis;

    public MemoryWithRedis(IMemory memory, IRedis redis)
    {
        _memory = memory;
        _redis = redis;
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

    public async Task SetAsync<T>(Key key, T data, TimeSpan? freshSpan, bool staleWhileRevalidate)
    {
        var memoryTask = _memory.SetAsync(key, data, freshSpan, staleWhileRevalidate);
        var redisTask = _redis.SetAsync(key, data, freshSpan, staleWhileRevalidate);

        await Task.WhenAll(memoryTask, redisTask);
    }

    public async Task<bool> DropAsync<T>(Key key)
    {
        var memoryTask = _memory.DropAsync<T>(key);
        var redisTask = _redis.DropAsync<T>(key);

        await Task.WhenAll(memoryTask, redisTask);
        return memoryTask.Result || redisTask.Result;
    }
}