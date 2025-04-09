namespace Tharga.Cache.Core;

internal class GenericTimeCache : TimeCacheBase
{
    public GenericTimeCache(IManagedCacheMonitor cacheMonitor, IPersistLoader persistLoader, Options options)
        : base(cacheMonitor, persistLoader, options)
    {
    }
}