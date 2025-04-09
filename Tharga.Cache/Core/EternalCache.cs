namespace Tharga.Cache.Core;

internal class EternalCache : CacheBase, IEternalCache, IScopeCache
{
    public EternalCache(IManagedCacheMonitor cacheMonitor, IPersistLoader persistLoader, Options options)
        : base(cacheMonitor, persistLoader, options)
    {
    }

    protected override TimeSpan DefaultFreshSpan => TimeSpan.MaxValue;
}