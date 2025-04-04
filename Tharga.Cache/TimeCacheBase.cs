namespace Tharga.Cache;

internal abstract class TimeCacheBase : CacheBase, ITimeCache
{
    protected TimeCacheBase(IManagedCacheMonitor cacheMonitor, IPersist persist, Options options)
        : base(cacheMonitor, persist, options)
    {
    }

    public Task<T> GetAsync<T>(Key key, Func<Task<T>> fetch, TimeSpan freshSpan)
    {
        throw new NotImplementedException();
    }

    public Task SetAsync<T>(Key key, T data, TimeSpan freshSpan)
    {
        throw new NotImplementedException();
    }
}