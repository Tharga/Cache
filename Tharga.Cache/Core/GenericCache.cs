namespace Tharga.Cache.Core;

internal class GenericCache : EternalCache
{
    public GenericCache(IManagedCacheMonitor cacheMonitor, IPersistLoader persistLoader, Options options)
        : base(cacheMonitor, persistLoader, options)
    {
    }
}