namespace Tharga.Cache.Core;

internal class TimeToIdleCache : TimeCacheBase, ITimeToIdleCache
{
    public TimeToIdleCache(IManagedCacheMonitor cacheMonitor, IPersistLoader persistLoader, CacheOptions options)
        : base(cacheMonitor, persistLoader, options)
    {
    }

    protected override Task OnGetAsync<T>(Key key)
    {
        return OnGetCoreAsync<T>(key, true);
    }
}