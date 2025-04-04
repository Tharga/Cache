namespace Tharga.Cache;

internal class GenericCache : CacheBase
{
    public GenericCache(IManagedCacheMonitor cacheMonitor, IPersist persist, Options options)
        : base(cacheMonitor, persist, options)
    {
    }
}