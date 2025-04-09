namespace Tharga.Cache.Core;

internal class TimeToLiveCache : TimeCacheBase, ITimeToLiveCache
{
    public TimeToLiveCache(IManagedCacheMonitor cacheMonitor, IPersistLoader persistLoader, CacheOptions options)
        : base(cacheMonitor, persistLoader, options)
    {
    }
}