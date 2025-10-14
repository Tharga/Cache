using Tharga.Cache.Persist;

namespace Tharga.Cache.Core;

internal abstract class CacheBase : ICache
{
    private readonly IManagedCacheMonitor _cacheMonitor;
    private readonly IPersistLoader _persistLoader;
    private readonly IFetchQueue _fetchQueue;
    protected readonly CacheOptions _options;

    protected CacheBase(IManagedCacheMonitor cacheMonitor, IPersistLoader persistLoader, IFetchQueue fetchQueue, CacheOptions options)
    {
        if (options.MaxConcurrentFetchCount <= 0) throw new InvalidOperationException($"Min value for {nameof(options.MaxConcurrentFetchCount)} is 1.");

        _cacheMonitor = cacheMonitor;
        _persistLoader = persistLoader;
        _fetchQueue = fetchQueue;
        _options = options;
    }

    public event EventHandler<DataSetEventArgs> DataSetEvent;
    public event EventHandler<DataGetEventArgs> DataGetEvent;
    public event EventHandler<DataDropEventArgs> DataDropEvent;

    protected abstract TimeSpan GetDefaultFreshSpan<T>();

    public virtual async Task<T> GetAsync<T>(Key key, Func<Task<T>> fetch)
    {
        return (await GetCoreAsync(key, fetch, GetDefaultFreshSpan<T>())).Data;
    }

    protected async Task<(T Data, bool Fresh)> GetCoreAsync<T>(Key key, Func<Task<T>> fetch, TimeSpan freshSpan, Func<T, Task> callback = default)
    {
        var fs = freshSpan == TimeSpan.MaxValue ? (TimeSpan?)null : freshSpan;

        //key = key.SetTypeKey<T>();
        key = KeyBuilder.SetTypeKey<T>(key);

        var result = await GetPersist<T>().GetAsync<T>(key);

        if (result.IsValid())
        {
            await OnGetAsync<T>(key);
            return (result.GetData(), true);
        }

        if (GetTypeOptions<T>().StaleWhileRevalidate && result != null)
        {
            var response = result.GetData();
            await OnGetAsync<T>(key);
            BackgroundLoad(key, fetch, callback, fs);

            return (response, false);
        }

        var loadResponse = await _fetchQueue.LoadData(key, fetch, fs, FetchCallback);
        await OnGetAsync<T>(key);
        return (loadResponse, true);
    }

    private void BackgroundLoad<T>(Key key, Func<Task<T>> fetch, Func<T, Task> callback, TimeSpan? fs)
    {
        Task.Run(async () =>
        {
            var data = await _fetchQueue.LoadData(key, fetch, fs, FetchCallback);
            callback?.Invoke(data);
        });
    }

    private async Task FetchCallback<T>(Key key, CacheItem<T> item, bool staleWhileRevalidate)
    {
        await GetPersist<T>().SetAsync(key, item, staleWhileRevalidate);
        await OnSetAsync(key, item, staleWhileRevalidate);
    }

    protected CacheTypeOptions GetTypeOptions<T>()
    {
        var options = _options.Get<T>();
        return options;
    }

    public virtual async Task<T> PeekAsync<T>(Key key)
    {
        //key = key.SetTypeKey<T>();
        key = KeyBuilder.SetTypeKey<T>(key);

        var result = await GetPersist<T>().GetAsync<T>(key);
        if (result.IsValid())
        {
            var response = result.GetData();
            await OnGetAsync<T>(key);

            return response;
        }

        if (GetTypeOptions<T>().StaleWhileRevalidate && result != null)
        {
            var response = result.GetData();
            await OnGetAsync<T>(key);

            return response;
        }

        return default;
    }

    public virtual Task SetAsync<T>(Key key, T data)
    {
        return SetCoreAsync(key, data, GetDefaultFreshSpan<T>());
    }

    protected async Task SetCoreAsync<T>(Key key, T data, TimeSpan freshSpan)
    {
        //key = key.SetTypeKey<T>();
        key = KeyBuilder.SetTypeKey<T>(key);

        var staleWhileRevalidate = GetTypeOptions<T>().StaleWhileRevalidate;
        //TODO: Implement
        //var item = CacheItemBuilder.BuildCacheItem(key.KeyParts, data, freshSpan);
        var item = CacheItemBuilder.BuildCacheItem([], data, freshSpan);
        await GetPersist<T>().SetAsync(key, item, staleWhileRevalidate);
        await OnSetAsync(key, item, staleWhileRevalidate);
    }

    public virtual async Task<int> DropAsync<T>(Key key)
    {
        //TODO: Implement
        //NOTE: Look for KeyParts to drop.
        //if (key.KeyParts.Any())
        //{
        //    var results = GetPersist<T>().FindAsync<T>(key);
        //    var count = 0;
        //    await foreach (var result in results)
        //    {
        //        var dropped = await GetPersist<T>().DropAsync(result.Key);
        //        if (dropped)
        //        {
        //            OnDrop<T>(result.Key);
        //            count++;
        //        }
        //    }

        //    return count;
        //}

        //key = key.SetTypeKey<T>();
        key = KeyBuilder.SetTypeKey<T>(key);

        var item = await GetPersist<T>().DropAsync(key);
        if (item)
        {
            OnDrop<T>(key);
            return 1;
        }

        return 0;
    }

    public async Task<int> InvalidateAsync<T>(Key key)
    {
        if (!GetTypeOptions<T>().StaleWhileRevalidate) return await DropAsync<T>(key);

        //TODO: Implement
        //NOTE: Look for KeyParts to invalidate.
        //if (key.KeyParts.Any())
        //{
        //    var results = GetPersist<T>().FindAsync<T>(key);
        //    var count = 0;
        //    await foreach (var result in results)
        //    {
        //        if (await GetPersist<T>().Invalidate<T>(result.Key))
        //        {
        //            count++;
        //        }
        //    }

        //    return count;
        //}

        //key = key.SetTypeKey<T>();
        key = KeyBuilder.SetTypeKey<T>(key);

        return await GetPersist<T>().Invalidate<T>(key) ? 1 : 0;
    }

    private async Task OnSetAsync<T>(Key key, CacheItem<T> item, bool staleWhileRevalidate)
    {
        await EvictItems(item.Data);

        DataSetEvent?.Invoke(this, new DataSetEventArgs(key, item.Data));
        _cacheMonitor.Set(typeof(T), key, item, staleWhileRevalidate);
    }

    protected virtual Task OnGetAsync<T>(Key key)
    {
        return OnGetCoreAsync<T>(key, false);
    }

    protected async Task OnGetCoreAsync<T>(Key key, bool buyMoreTime)
    {
        DataGetEvent?.Invoke(this, new DataGetEventArgs(key));

        var moreTimeBought = false;
        if (buyMoreTime)
        {
            moreTimeBought = await GetPersist<T>().BuyMoreTime<T>(key);
        }

        _cacheMonitor.Accessed(typeof(T), key, moreTimeBought);
    }

    private void OnDrop<T>(Key key)
    {
        DataDropEvent?.Invoke(this, new DataDropEventArgs(key));
        _cacheMonitor.Drop(typeof(T), key);
    }

    private IPersist GetPersist<T>()
    {
        var persist = _persistLoader.GetPersist(_options.Get<T>().PersistType);
        return persist;
    }

    private async Task EvictItems<T>(T data)
    {
        var maxCount = GetTypeOptions<T>().MaxCount;
        var maxSize = GetTypeOptions<T>().MaxSize;

        if (maxCount != null || maxSize != null)
        {
            var result = _cacheMonitor.GetInfos().FirstOrDefault(x => x.Type == typeof(T));
            if (maxCount <= result?.Items.Count || maxSize <= result?.Items.Sum(x => x.Value.Size) + data.ToSize())
            {
                var keyToDrop = _cacheMonitor.Get<T>(GetTypeOptions<T>().EvictionPolicy);
                await GetPersist<T>().DropAsync(keyToDrop);
                OnDrop<T>(keyToDrop);
            }
        }
    }
}