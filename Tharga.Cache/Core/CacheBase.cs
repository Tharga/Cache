using System;

namespace Tharga.Cache.Core;

internal abstract class CacheBase : ICache
{
    private readonly IManagedCacheMonitor _cacheMonitor;
    private readonly IPersist _persist;
    private readonly Options _options;

    protected CacheBase(IManagedCacheMonitor cacheMonitor, IPersist persist, Options options)
    {
        _cacheMonitor = cacheMonitor;
        _persist = persist;
        _options = options;
    }

    public event EventHandler<DataSetEventArgs> DataSetEvent;
    public event EventHandler<DataGetEventArgs> DataGetEvent;
    public event EventHandler<DataDropEventArgs> DataDropEvent;

    protected abstract TimeSpan DefaultFreshSpan { get; }

    public virtual Task<T> GetAsync<T>(Key key, Func<Task<T>> fetch)
    {
        return GetAsync(key, fetch, DefaultFreshSpan);
    }

    //TODO: Rename and protect
    public virtual async Task<T> GetAsync<T>(Key key, Func<Task<T>> fetch, TimeSpan freshSpan)
    {
        key = BuildKey<T>(key);

        var result = await _persist.GetAsync<T>(key);

        if (result.IsValid())
        {
            DataGetEvent?.Invoke(this, new DataGetEventArgs());
            return result.GetData<T>();
        }

        var data = await fetch.Invoke();

        await _persist.SetAsync(key, data, freshSpan);

        DataSetEvent?.Invoke(this, new DataSetEventArgs());
        DataGetEvent?.Invoke(this, new DataGetEventArgs());
        _cacheMonitor.Add(typeof(T), key, data);

        return data;
    }

    public virtual async Task<T> PeekAsync<T>(Key key)
    {
        key = BuildKey<T>(key);

        var result = await _persist.GetAsync<T>(key);
        if (result.IsValid())
        {
            DataGetEvent?.Invoke(this, new DataGetEventArgs());
        }

        return result == null ? default : result.GetData<T>();
    }

    public virtual Task SetAsync<T>(Key key, T data)
    {
        return SetAsync(key, data, DefaultFreshSpan);
    }

    //TODO: Rename and protect
    public virtual async Task SetAsync<T>(Key key, T data, TimeSpan freshSpan)
    {
        key = BuildKey<T>(key);

        await _persist.SetAsync(key, data, freshSpan);

        DataSetEvent?.Invoke(this, new DataSetEventArgs());
        _cacheMonitor.Add(typeof(T), key, data);
    }

    public virtual async Task<T> DropAsync<T>(Key key)
    {
        key = BuildKey<T>(key);

        var item = await _persist.DropAsync<T>(key);
        if (item.IsValid())
        {
            DataDropEvent?.Invoke(this, new DataDropEventArgs());
            _cacheMonitor.Drop(typeof(T), key);
            return item.GetData<T>();
        }

        return default;
    }

    protected virtual string BuildKey<T>(string key)
    {
        var k = $"{typeof(T).Name}.{key}";
        return k;
    }
}