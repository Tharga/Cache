using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Tharga.Cache.Persist;

namespace Tharga.Cache.Core;

internal class FetchQueue : IFetchQueue
{
    private readonly CacheOptions _options;
    private readonly ILogger<FetchQueue> _logger;
    private readonly ConcurrentDictionary<Key, Lazy<Task<object>>> _inFlightFetches = new();
    private readonly ConcurrentStack<FetchWorkItem> _fetchStack = new();
    private readonly SemaphoreSlim _globalSemaphore;
    private readonly object _dispatchLock = new();
    private bool _isDispatching;

    public FetchQueue(IManagedCacheMonitor cacheMonitor, CacheOptions options, ILogger<FetchQueue> logger)
    {
        _options = options;
        _logger = logger;
        _globalSemaphore = new(options.MaxConcurrentFetchCount, options.MaxConcurrentFetchCount);
        //cacheMonitor.AddFetchCount(() => _fetchStack.Count);
        cacheMonitor.AddFetchCount(() => _inFlightFetches.Count);
    }

    public async Task<T> LoadData<T>(Key key, Func<Task<T>> fetch, TimeSpan? freshSpan, Func<Key, CacheItem<T>, bool, Task> fetchCallback)
    {
        var lazyTask = _inFlightFetches.GetOrAdd(key, _ =>
            new Lazy<Task<object>>(() =>
            {
                var tcs = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);

                _fetchStack.Push(new FetchWorkItem
                {
                    Key = key,
                    Fetch = async () =>
                    {
                        try
                        {
                            var result = await fetch();

                            var staleWhileRevalidate = _options.Get<T>().StaleWhileRevalidate;
                            var item = CacheItemBuilder.BuildCacheItem(result, freshSpan);
                            await fetchCallback.Invoke(key, item, staleWhileRevalidate);

                            tcs.TrySetResult(result!);
                        }
                        catch (Exception ex)
                        {
                            _logger?.LogError(ex, ex.Message);
                            tcs.TrySetException(ex);
                        }
                        finally
                        {
                            _inFlightFetches.TryRemove(key, out var _);
                        }
                    }
                });

                StartDispatcher();
                return tcs.Task;
            }, LazyThreadSafetyMode.ExecutionAndPublication)
        );

        var result = await lazyTask.Value;
        return (T)result!;
    }

    private void StartDispatcher()
    {
        lock (_dispatchLock)
        {
            if (_isDispatching) return;

            _isDispatching = true;
            _ = Task.Run(DispatchLoop);
        }
    }

    private async Task DispatchLoop()
    {
        while (true)
        {
            if (!_fetchStack.TryPop(out var work))
            {
                lock (_dispatchLock)
                {
                    _isDispatching = false;
                    return;
                }
            }

            await _globalSemaphore.WaitAsync();

            _ = Task.Run(async () =>
            {
                try
                {
                    await work.Fetch();
                }
                finally
                {
                    _globalSemaphore.Release();
                }
            });
        }
    }

    private record FetchWorkItem
    {
        public required Key Key { get; init; }
        public required Func<Task> Fetch { get; init; }
    }
}