namespace Tharga.Cache.Core;

internal class GenericTimeCache : TimeCacheBase
{
    public GenericTimeCache(IManagedCacheMonitor cacheMonitor, IPersistLoader persistLoader, CacheOptions options)
        : base(cacheMonitor, persistLoader, options)
    {
    }
}