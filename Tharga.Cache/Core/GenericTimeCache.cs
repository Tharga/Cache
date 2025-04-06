namespace Tharga.Cache.Core;

internal class GenericTimeCache : TimeCacheBase
{
    public GenericTimeCache(IManagedCacheMonitor cacheMonitor, IPersist persist, Options options)
        : base(cacheMonitor, persist, options)
    {
    }
}