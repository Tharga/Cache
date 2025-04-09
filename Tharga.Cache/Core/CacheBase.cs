namespace Tharga.Cache.Core;

internal abstract class CacheBase : ICache
{
    private readonly IManagedCacheMonitor _cacheMonitor;
    private readonly IPersistLoader _persistLoader;
    private readonly CacheOptions _options;

    protected CacheBase(IManagedCacheMonitor cacheMonitor, IPersistLoader persistLoader, CacheOptions options)
    {
        _cacheMonitor = cacheMonitor;
        _persistLoader = persistLoader;
        _options = options;
    }

    public event EventHandler<DataSetEventArgs> DataSetEvent;
    public event EventHandler<DataGetEventArgs> DataGetEvent;
    public event EventHandler<DataDropEventArgs> DataDropEvent;

    protected abstract TimeSpan DefaultFreshSpan { get; }

    public async IAsyncEnumerable<T> GetAsync<T>()
    {
        await foreach (var item in GetPersist<T>().GetAsync<T>())
        {
            yield return item.GetData<T>();
        }
    }

    public virtual Task<T> GetAsync<T>(Key key, Func<Task<T>> fetch)
    {
        return GetCoreAsync(key, fetch, DefaultFreshSpan);
    }

    protected async Task<T> GetCoreAsync<T>(Key key, Func<Task<T>> fetch, TimeSpan freshSpan)
    {
        var fs = freshSpan == TimeSpan.MaxValue ? (TimeSpan?)null : freshSpan;

        key = KeyBuilder.BuildKey<T>(key);

        var result = await GetPersist<T>().GetAsync<T>(key);

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
                await LoadData(key, fetch, fs);
            });

            return response;
        }

        return await LoadData(key, fetch, fs);
    }

    private CacheTypeOptions GetTypeOptions<T>()
    {
        var options = _options.Get<T>();
        return options;
    }

    private async Task<T> LoadData<T>(Key key, Func<Task<T>> fetch, TimeSpan? freshSpan)
    {
        var data = await fetch.Invoke();

        await GetPersist<T>().SetAsync(key, data, freshSpan);
        DropWhenStale<T>(key, freshSpan);
        await OnSetAsync(key, data);

        return data;
    }

    public virtual async Task<T> PeekAsync<T>(Key key)
    {
        key = KeyBuilder.BuildKey<T>(key);

        var result = await GetPersist<T>().GetAsync<T>(key);
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
        return SetCoreAsync(key, data, DefaultFreshSpan);
    }

    protected async Task SetCoreAsync<T>(Key key, T data, TimeSpan freshSpan)
    {
        var fs = freshSpan == TimeSpan.MaxValue ? (TimeSpan?)null : freshSpan;

        key = KeyBuilder.BuildKey<T>(key);

        await GetPersist<T>().SetAsync(key, data, fs);
        DropWhenStale<T>(key, fs);
        await OnSetAsync(key, data);
    }

    public virtual async Task<T> DropAsync<T>(Key key)
    {
        key = KeyBuilder.BuildKey<T>(key);

        var item = await GetPersist<T>().DropAsync<T>(key);
        if (item.IsValid())
        {
            OnDrop(key, item);
            return item.GetData<T>();
        }

        return default;
    }

    //protected string BuildKey<T>(string key)
    //{
    //    var k = $"{typeof(T).Name}.{key}";
    //    return k;
    //}

    private void DropWhenStale<T>(Key key, TimeSpan? freshSpan)
    {
        if (!GetTypeOptions<T>().StaleWhileRevalidate && freshSpan.HasValue && freshSpan != TimeSpan.MaxValue)
        {
            Task.Run(async () =>
            {
                await Task.Delay(freshSpan.Value);
                var item = await GetPersist<T>().DropAsync<T>(key);
                if (item != null)
                {
                    OnDrop<T>(key, item);
                }
            });
        }
    }

    private async Task OnSetAsync<T>(Key key, T data)
    {
        //NOTE: Evict if needed
        var result = _cacheMonitor.GetInfos().FirstOrDefault(x => x.Type == typeof(T));
        if (result?.Items.Count >= GetTypeOptions<T>().MaxCount
            || result?.Items.Sum(x => x.Value.Size) + data.ToSize() >= GetTypeOptions<T>().MaxSize)
        {
            var keyToDrop = _cacheMonitor.Get<T>(GetTypeOptions<T>().EvictionPolicy);
            var droppedItem = await GetPersist<T>().DropAsync<T>(keyToDrop);
            OnDrop<T>(keyToDrop, droppedItem);
        }

        DataSetEvent?.Invoke(this, new DataSetEventArgs(key, data));
        _cacheMonitor.Set(typeof(T), key, data);
    }

    private void OnGet<T>(Key key)
    {
        DataGetEvent?.Invoke(this, new DataGetEventArgs(key));
        _cacheMonitor.Accessed(typeof(T), key);
    }

    private void OnDrop<T>(Key key, CacheItem<T> item)
    {
        DataDropEvent?.Invoke(this, new DataDropEventArgs(key, item.Data));
        _cacheMonitor.Drop(typeof(T), key);
    }

    private IPersist GetPersist<T>()
    {
        var persist = _persistLoader.GetPersist(_options.Get<T>());
        return persist;
    }
}