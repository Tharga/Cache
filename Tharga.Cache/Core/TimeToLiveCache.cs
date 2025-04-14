namespace Tharga.Cache.Core;

internal class TimeToLiveCache : TimeCacheBase, ITimeToLiveCache
{
    public TimeToLiveCache(IManagedCacheMonitor cacheMonitor, IPersistLoader persistLoader, IFetchQueue fetchQueue, CacheOptions options)
        : base(cacheMonitor, persistLoader, fetchQueue, options)
    {
    }
}