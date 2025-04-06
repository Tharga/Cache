namespace Tharga.Cache.Core;

internal class GenericCache : EternalCache
{
    public GenericCache(IManagedCacheMonitor cacheMonitor, IPersist persist, Options options)
        : base(cacheMonitor, persist, options)
    {
    }
}