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
        return GetAsyncX(key, fetch, DefaultFreshSpan);
    }

    protected async Task<T> GetAsyncX<T>(Key key, Func<Task<T>> fetch, TimeSpan freshSpan)
    {
        key = BuildKey<T>(key);

        var result = await _persist.GetAsync<T>(key);

        if (result.IsValid())
        {
            OnGet<T>(key);
            return result.GetData<T>();
        }

        if (GetTypeOptions<T>().StaleWhileRevalidate && result != null)
        {
            var response = result.GetData<T>();
            OnGet<T>(key);

            Task.Run(async () =>
            {
                await LoadData(key, fetch, freshSpan);
            });

            return response;
        }

        return await LoadData(key, fetch, freshSpan);
    }

    private TypeOptions GetTypeOptions<T>()
    {
        var options = _options.Get<T>();
        return options;
    }

    private async Task<T> LoadData<T>(Key key, Func<Task<T>> fetch, TimeSpan freshSpan)
    {
        var data = await fetch.Invoke();

        await _persist.SetAsync(key, data, freshSpan);
        DropWhenStale<T>(key, freshSpan);
        OnSet(key, data);

        return data;
    }

    public virtual async Task<T> PeekAsync<T>(Key key)
    {
        key = BuildKey<T>(key);

        var result = await _persist.GetAsync<T>(key);
        if (result.IsValid())
        {
            var response = result.GetData<T>();
            OnGet<T>(key);

            return response;
        }

        if (GetTypeOptions<T>().StaleWhileRevalidate && result != null)
        {
            var response = result.GetData<T>();
            OnGet<T>(key);

            return response;
        }

        return default;
    }

    public virtual Task SetAsync<T>(Key key, T data)
    {
        return SetAsyncX(key, data, DefaultFreshSpan);
    }

    protected async Task SetAsyncX<T>(Key key, T data, TimeSpan freshSpan)
    {
        key = BuildKey<T>(key);

        await _persist.SetAsync(key, data, freshSpan);
        DropWhenStale<T>(key, freshSpan);
        OnSet(key, data);
    }

    public virtual async Task<T> DropAsync<T>(Key key)
    {
        key = BuildKey<T>(key);

        var item = await _persist.DropAsync<T>(key);
        if (item.IsValid())
        {
            OnDrop<T>(key, item);
            return item.GetData<T>();
        }

        return default;
    }

    protected virtual string BuildKey<T>(string key)
    {
        var k = $"{typeof(T).Name}.{key}";
        return k;
    }

    private void DropWhenStale<T>(Key key, TimeSpan freshSpan)
    {
        if (!GetTypeOptions<T>().StaleWhileRevalidate)
        {
            Task.Run(async () =>
            {
                await Task.Delay(freshSpan);
                var item = await _persist.DropAsync<T>(key);
                if (item != null)
                {
                    OnDrop<T>(key, item);
                }
            });
        }
    }

    private void OnSet<T>(Key key, T data)
    {
        DataSetEvent?.Invoke(this, new DataSetEventArgs(key, data));
        _cacheMonitor.Set(typeof(T), key, data);
    }

    private void OnGet<T>(Key key)
    {
        DataGetEvent?.Invoke(this, new DataGetEventArgs(key));
        _cacheMonitor.Get(typeof(T), key);
    }

    private void OnDrop<T>(Key key, CacheItem item)
    {
        DataDropEvent?.Invoke(this, new DataDropEventArgs(key, item.Data));
        _cacheMonitor.Drop(typeof(T), key);
    }
}