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

    public async Task<T> GetAsync<T>(Key key, Func<Task<T>> fetch)
    {
        key = BuildKey<T>(key);

        var result = await _persist.GetAsync<T>(key);

        if (result.Found)
        {
            return result.Data;
        }

        var data = await fetch.Invoke();

        await _persist.SetAsync(key, data);

        DataSetEvent?.Invoke(this, new DataSetEventArgs());
        _cacheMonitor.Add(typeof(T), key, data);

        return data;
    }

    public async Task<T> PeekAsync<T>(Key key)
    {
        key = BuildKey<T>(key);

        var result = await _persist.GetAsync<T>(key);
        return result.Data;
    }

    public async Task SetAsync<T>(Key key, T data)
    {
        key = BuildKey<T>(key);

        await _persist.SetAsync(key, data);

        DataSetEvent?.Invoke(this, new DataSetEventArgs());
        _cacheMonitor.Add(typeof(T), key, data);
    }

    public async Task DropAsync<T>(Key key)
    {
    }

    protected virtual string BuildKey<T>(string key)
    {
        var k = $"{typeof(T).Name}.{key}";
        return k;
    }
}