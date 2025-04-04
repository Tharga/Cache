namespace Tharga.Cache;

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

    public async Task<T> GetAsync<T>(Key key, Func<Task<T>> fetch)
    {
        key = BuildKey<T>(key);

        var result = await _persist.GetAsync<T>(key);

        if (result.Found)
        {
            DataGetEvent?.Invoke(this, new DataGetEventArgs());
            return result.Data;
        }

        var data = await fetch.Invoke();

        await _persist.SetAsync(key, data);

        DataSetEvent?.Invoke(this, new DataSetEventArgs());
        DataGetEvent?.Invoke(this, new DataGetEventArgs());
        _cacheMonitor.Add(typeof(T), key, data);

        return data;
    }

    public async Task<T> PeekAsync<T>(Key key)
    {
        key = BuildKey<T>(key);

        var result = await _persist.GetAsync<T>(key);
        if (result.Found)
        {
            DataGetEvent?.Invoke(this, new DataGetEventArgs());
        }

        return result.Data;
    }

    public async Task SetAsync<T>(Key key, T data)
    {
        key = BuildKey<T>(key);

        await _persist.SetAsync(key, data);

        DataSetEvent?.Invoke(this, new DataSetEventArgs());
        _cacheMonitor.Add(typeof(T), key, data);
    }

    public async Task<T> DropAsync<T>(Key key)
    {
        key = BuildKey<T>(key);

        var item = await _persist.DropAsync<T>(key);
        if (item.Found)
        {
            DataDropEvent?.Invoke(this, new DataDropEventArgs());
            _cacheMonitor.Drop(typeof(T), key);
            return item.Data;
        }

        return default;
    }

    protected virtual string BuildKey<T>(string key)
    {
        var k = $"{typeof(T).Name}.{key}";
        return k;
    }
}