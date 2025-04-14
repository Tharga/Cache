namespace Tharga.Cache.Core;

internal class EternalCache : CacheBase, IEternalCache, IScopeCache
{
    public EternalCache(IManagedCacheMonitor cacheMonitor, IPersistLoader persistLoader, IFetchQueue fetchQueue, CacheOptions options)
        : base(cacheMonitor, persistLoader, fetchQueue, options)
    {
    }

    protected override TimeSpan GetDefaultFreshSpan<T>() => TimeSpan.MaxValue;
}