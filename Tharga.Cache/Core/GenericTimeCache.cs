namespace Tharga.Cache.Core;

internal class GenericTimeCache : TimeCacheBase
{
    public GenericTimeCache(IManagedCacheMonitor cacheMonitor, IPersistLoader persistLoader, IFetchQueue fetchQueue, CacheOptions options)
        : base(cacheMonitor, persistLoader, fetchQueue, options)
    {
    }
}