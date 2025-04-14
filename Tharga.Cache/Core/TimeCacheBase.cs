namespace Tharga.Cache.Core;

internal abstract class TimeCacheBase : CacheBase, ITimeCache
{
    protected TimeCacheBase(IManagedCacheMonitor cacheMonitor, IPersistLoader persistLoader, IFetchQueue fetchQueue, CacheOptions options)
        : base(cacheMonitor, persistLoader, fetchQueue, options)
    {
    }

    protected override TimeSpan GetDefaultFreshSpan<T>()
    {
        return GetTypeOptions<T>().DefaultFreshSpan ?? throw new InvalidOperationException($"No freshSpan provided and no {nameof(CacheTypeOptions.DefaultFreshSpan)} configured for cache type {typeof(T).Name}.");
    }

    public virtual async Task<T> GetAsync<T>(Key key, Func<Task<T>> fetch, TimeSpan freshSpan)
    {
        return (await GetCoreAsync(key, fetch, freshSpan)).Data;
    }

    public async Task<(T Data, bool Fresh)> GetWithCallbackAsync<T>(Key key, Func<Task<T>> fetch, Func<T, Task> callback, TimeSpan? freshSpan)
    {
        var response = await GetCoreAsync(key, fetch, freshSpan ?? GetDefaultFreshSpan<T>(), callback);
        return (response.Data, response.Fresh);
    }

    public virtual Task SetAsync<T>(Key key, T data, TimeSpan freshSpan)
    {
        return SetCoreAsync(key, data, freshSpan);
    }
}