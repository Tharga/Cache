namespace Tharga.Cache.Core;

internal class TimeToIdleCache : TimeCacheBase, ITimeToIdleCache
{
    public TimeToIdleCache(IManagedCacheMonitor cacheMonitor, IPersist persist, Options options)
        : base(cacheMonitor, persist, options)
    {
    }

    //public override async Task<T> GetAsync<T>(Key key, Func<Task<T>> fetch, TimeSpan freshSpan)
    //{
    //    return await base.GetAsync(key, fetch, freshSpan);
    //}

    //public override async Task SetAsync<T>(Key key, T data, TimeSpan freshSpan)
    //{
    //    await base.SetAsync(key, data, freshSpan);
    //}
}