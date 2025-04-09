namespace Tharga.Cache.Core;

internal abstract class TimeCacheBase : CacheBase, ITimeCache
{
    protected TimeCacheBase(IManagedCacheMonitor cacheMonitor, IPersistLoader persistLoader, CacheOptions options)
        : base(cacheMonitor, persistLoader, options)
    {
    }

    protected override TimeSpan DefaultFreshSpan => TimeSpan.FromMinutes(60);

    public Task<T> GetAsync<T>(Key key, Func<Task<T>> fetch, TimeSpan freshSpan)
    {
        return GetCoreAsync(key, fetch, freshSpan);
    }

    public Task SetAsync<T>(Key key, T data, TimeSpan freshSpan)
    {
        return SetCoreAsync(key, data, freshSpan);
    }


    //public virtual async Task<T> GetAsync<T>(Key key, Func<Task<T>> fetch, TimeSpan freshSpan)
    //{
    //    key = BuildKey<T>(key);

    //    var result = await _persist.GetAsync<T>(key);

    //    if (result.IsValid())
    //    {
    //        //DataGetEvent?.Invoke(this, new DataGetEventArgs());
    //        return result.GetData<T>();
    //    }

    //    var data = await fetch.Invoke();

    //    await _persist.SetAsync(key, data, freshSpan);

    //    //DataSetEvent?.Invoke(this, new DataSetEventArgs());
    //    //DataGetEvent?.Invoke(this, new DataGetEventArgs());
    //    _cacheMonitor.Set(typeof(T), key, data);

    //    return data;
    //}

    //public override async Task SetAsync<T>(Key key, T data, TimeSpan freshSpan)
    //{
    //    await base.SetAsync(key, data, freshSpan);
    //}
}