namespace Tharga.Cache.Core;

internal class GenericCache : CacheBase
{
    public GenericCache(IManagedCacheMonitor cacheMonitor, IPersistLoader persistLoader, CacheOptions options)
        : base(cacheMonitor, persistLoader, options)
    {
    }

    protected override TimeSpan GetDefaultFreshSpan<T>()
    {
        throw new NotImplementedException();
    }
}