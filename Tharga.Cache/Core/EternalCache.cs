namespace Tharga.Cache.Core;

internal class EternalCache : CacheBase, IEternalCache, IScopeCache
{
    public EternalCache(IManagedCacheMonitor cacheMonitor, IPersist persist, Options options)
        : base(cacheMonitor, persist, options)
    {
    }

    protected override TimeSpan DefaultFreshSpan => TimeSpan.MaxValue;
}