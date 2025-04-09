namespace Tharga.Cache.Core;

internal abstract class TimeCacheBase : CacheBase, ITimeCache
{
    protected TimeCacheBase(IManagedCacheMonitor cacheMonitor, IPersistLoader persistLoader, CacheOptions options)
        : base(cacheMonitor, persistLoader, options)
    {
    }

    protected override TimeSpan GetDefaultFreshSpan<T>()
    {
        return GetTypeOptions<T>().DefaultFreshSpan ?? throw new InvalidOperationException($"No freshSpan provided and no {nameof(CacheTypeOptions.DefaultFreshSpan)} configured for cache type {typeof(T).Name}.");
    }

    public virtual Task<T> GetAsync<T>(Key key, Func<Task<T>> fetch, TimeSpan freshSpan)
    {
        return GetCoreAsync(key, fetch, freshSpan);
    }

    public virtual Task SetAsync<T>(Key key, T data, TimeSpan freshSpan)
    {
        return SetCoreAsync(key, data, freshSpan);
    }
}