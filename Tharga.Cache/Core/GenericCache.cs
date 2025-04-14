namespace Tharga.Cache.Core;

internal class GenericCache : CacheBase
{
    public GenericCache(IManagedCacheMonitor cacheMonitor, IPersistLoader persistLoader, IFetchQueue fetchQueue, CacheOptions options)
        : base(cacheMonitor, persistLoader, fetchQueue, options)
    {
    }

    protected override TimeSpan GetDefaultFreshSpan<T>()
    {
        throw new NotImplementedException();
    }
}