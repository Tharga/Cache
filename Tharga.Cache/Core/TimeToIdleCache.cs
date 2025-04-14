namespace Tharga.Cache.Core;

internal class TimeToIdleCache : TimeCacheBase, ITimeToIdleCache
{
    public TimeToIdleCache(IManagedCacheMonitor cacheMonitor, IPersistLoader persistLoader, IFetchQueue fetchQueue, CacheOptions options)
        : base(cacheMonitor, persistLoader, fetchQueue, options)
    {
    }

    protected override Task OnGetAsync<T>(Key key)
    {
        return OnGetCoreAsync<T>(key, true);
    }
}