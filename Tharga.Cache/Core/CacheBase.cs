using System.Collections.Concurrent;

namespace Tharga.Cache.Core;

internal abstract class CacheBase : ICache
{
    private readonly IManagedCacheMonitor _cacheMonitor;
    private readonly IPersistLoader _persistLoader;
    private readonly CacheOptions _options;
    private readonly SemaphoreSlim _globalSemaphore;
    private readonly ConcurrentDictionary<Key, Lazy<Task<object>>> _inFlightFetches = new();

    protected CacheBase(IManagedCacheMonitor cacheMonitor, IPersistLoader persistLoader, CacheOptions options)
    {
        if (options.MaxConcurrentFetchCount <= 0) throw new InvalidOperationException($"Min value for {nameof(options.MaxConcurrentFetchCount)} is 1.");

        _cacheMonitor = cacheMonitor;
        _persistLoader = persistLoader;
        _options = options;
        _globalSemaphore = new(options.MaxConcurrentFetchCount, options.MaxConcurrentFetchCount);
        _cacheMonitor.QueueCountLoader = () => _inFlightFetches.Count;
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

        key = KeyBuilder.BuildKey<T>(key);

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

            Task.Run(async () =>
            {
                var data = await LoadData(key, fetch, fs);
                callback?.Invoke(data);
            });

            return (response, false);
        }

        var loadResponse = await LoadData(key, fetch, fs);
        await OnGetAsync<T>(key);
        return (loadResponse, true);
    }

    protected CacheTypeOptions GetTypeOptions<T>()
    {
        var options = _options.Get<T>();
        return options;
    }

    private async Task<T> LoadData<T>(Key key, Func<Task<T>> fetch, TimeSpan? freshSpan)
    {
        var lazyTask = _inFlightFetches.GetOrAdd(key, _ =>
            new Lazy<Task<object>>(async () =>
            {
                await _globalSemaphore.WaitAsync();
                try
                {
                    var result = await fetch();

                    await GetPersist<T>().SetAsync(key, result, freshSpan, GetTypeOptions<T>().StaleWhileRevalidate);
                    DropWhenStale<T>(key, freshSpan);
                    await OnSetAsync(key, result);

                    return result!;
                }
                finally
                {
                    _globalSemaphore.Release();
                    _inFlightFetches.TryRemove(key, out var _);
                }
            }, LazyThreadSafetyMode.ExecutionAndPublication)
        );

        var result = await lazyTask.Value;

        return (T)result!;
    }

    public virtual async Task<T> PeekAsync<T>(Key key)
    {
        key = KeyBuilder.BuildKey<T>(key);

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
        var fs = freshSpan == TimeSpan.MaxValue ? (TimeSpan?)null : freshSpan;

        key = KeyBuilder.BuildKey<T>(key);

        await GetPersist<T>().SetAsync(key, data, fs, GetTypeOptions<T>().StaleWhileRevalidate);
        DropWhenStale<T>(key, fs);
        await OnSetAsync(key, data);
    }

    public virtual async Task<bool> DropAsync<T>(Key key)
    {
        key = KeyBuilder.BuildKey<T>(key);

        var item = await GetPersist<T>().DropAsync<T>(key);
        if (item)
        {
            OnDrop<T>(key);
            return true;
        }

        return false;
    }

    public async Task InvalidateAsync<T>(Key key)
    {
        if (!GetTypeOptions<T>().StaleWhileRevalidate) await DropAsync<T>(key);
        await GetPersist<T>().Invalidate<T>(key);
    }

    private void DropWhenStale<T>(Key key, TimeSpan? freshSpan)
    {
        if (!GetTypeOptions<T>().StaleWhileRevalidate && freshSpan.HasValue && freshSpan != TimeSpan.MaxValue)
        {
            //TODO: We want to cancel this task, if buy-more-time is called, since we do not want threads not needed.
            //TODO: Use a watchdog instead of background-tasks
            Task.Run(async () =>
            {
                await Task.Delay(freshSpan.Value);
                var current = await GetPersist<T>().GetAsync<T>(key);
                if (!current.IsValid())
                {
                    await GetPersist<T>().DropAsync<T>(key);
                    OnDrop<T>(key);
                }
            });
        }
    }

    private async Task OnSetAsync<T>(Key key, T data)
    {
        await EvictItems(data);

        DataSetEvent?.Invoke(this, new DataSetEventArgs(key, data));
        _cacheMonitor.Set(typeof(T), key, data);
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
                await GetPersist<T>().DropAsync<T>(keyToDrop);
                OnDrop<T>(keyToDrop);
            }
        }
    }
}