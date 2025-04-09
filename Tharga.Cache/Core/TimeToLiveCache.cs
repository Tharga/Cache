namespace Tharga.Cache.Core;

internal class TimeToLiveCache : TimeCacheBase, ITimeToLiveCache
{
    public TimeToLiveCache(IManagedCacheMonitor cacheMonitor, IPersistLoader persistLoader, CacheOptions options)
        : base(cacheMonitor, persistLoader, options)
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