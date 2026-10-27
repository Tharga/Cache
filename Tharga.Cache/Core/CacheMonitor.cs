using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.ExceptionServices;
using Microsoft.Extensions.Logging;

namespace Tharga.Cache.Core;

internal class CacheMonitor : IManagedCacheMonitor
{
    private static readonly MethodInfo DropAsyncMethod = typeof(IPersist).GetMethod(nameof(IPersist.DropAsync))!;

    private readonly IPersistLoader _persistLoader;
    private readonly CacheOptions _cacheOptions;
    private readonly ILogger _logger;
    private readonly ConcurrentDictionary<Type, CacheTypeInfo> _caches = new();
    private readonly List<Func<int>> _fetchCount = new();

    public CacheMonitor(IPersistLoader persistLoader, CacheOptions cacheOptions, ILogger logger = null)
    {
        _persistLoader = persistLoader;
        _cacheOptions = cacheOptions;
        _logger = logger;
    }

    public event EventHandler<DataGetEventArgs> DataGetEvent;
    public event EventHandler<DataSetEventArgs> DataSetEvent;
    public event EventHandler<DataDropEventArgs> DataDropEvent;

    public void Set<T>(Type type, Key key, CacheItem<T> item, bool staleWhileRevalidate, bool returnDefaultOnFirstLoad)
    {
        var size = item.Data.ToSize();
        var persistType = _cacheOptions.Get<T>().PersistType;

        _caches.AddOrUpdate(type, new CacheTypeInfo
        {
            Type = type,
            PersistType = persistType,
            StaleWhileRevalidate = staleWhileRevalidate,
            ReturnDefaultOnFirstLoad = returnDefaultOnFirstLoad,
            Items = new ConcurrentDictionary<string, CacheItemInfo>(new Dictionary<string, CacheItemInfo>
            {
                {
                    key, new CacheItemInfo(item.CreateTime)
                    {
                        Size = size,
                        FreshSpan = item.FreshSpan,
                        LoadDuration = item.LoadDuration
                    }
                }
            })
        }, (_, b) =>
        {
            b.Items.AddOrUpdate(key, new CacheItemInfo(item.CreateTime)
            {
                Size = size,
                FreshSpan = item.FreshSpan,
                LoadDuration = item.LoadDuration
            }, (_, c) =>
            {
                c.SetUpdated(item.CreateTime, item.UpdateTime);
                return c;
            });
            return b;
        });

        DataSetEvent?.Invoke(this, new DataSetEventArgs(key, item.Data));
    }

    public void Track<T>(Type type, Key key, CacheItem<T> item, bool staleWhileRevalidate, bool returnDefaultOnFirstLoad)
    {
        if (_caches.ContainsKey(type) && _caches[type].Items.ContainsKey(key))
            return;

        var size = item.Data.ToSize();
        var persistType = _cacheOptions.Get<T>().PersistType;

        _caches.AddOrUpdate(type, new CacheTypeInfo
        {
            Type = type,
            PersistType = persistType,
            StaleWhileRevalidate = staleWhileRevalidate,
            ReturnDefaultOnFirstLoad = returnDefaultOnFirstLoad,
            Items = new ConcurrentDictionary<string, CacheItemInfo>(new Dictionary<string, CacheItemInfo>
            {
                {
                    key, new CacheItemInfo(item.CreateTime)
                    {
                        Size = size,
                        FreshSpan = item.FreshSpan,
                        LoadDuration = item.LoadDuration
                    }
                }
            })
        }, (_, b) =>
        {
            b.Items.TryAdd(key, new CacheItemInfo(item.CreateTime)
            {
                Size = size,
                FreshSpan = item.FreshSpan,
                LoadDuration = item.LoadDuration
            });
            return b;
        });
    }

    public void Accessed(Type type, Key key, bool buyMoreTime)
    {
        if (_caches.TryGetValue(type, out var info))
        {
            info.Items.FirstOrDefault(x => key.Equals(x.Key)).Value?.SetAccess();
            DataGetEvent?.Invoke(this, new DataGetEventArgs(key));
        }
    }

    public void Drop(Type type, Key key)
    {
        if (_caches.TryGetValue(type, out var info))
        {
            info.Items.TryRemove(key, out _);
            if (!info.Items.Any())
            {
                _caches.TryRemove(type, out _);
            }
            DataDropEvent?.Invoke(this, new DataDropEventArgs(key));
        }
    }

    public Key Get<T>(EvictionPolicy evictionPolicy)
    {
        if (!_caches.TryGetValue(typeof(T), out var val)) return null;

        switch (evictionPolicy)
        {
            case EvictionPolicy.LeastRecentlyUsed:
                return val.Items.OrderByDescending(x => x.Value?.LastAccessTime ?? DateTime.MinValue).FirstOrDefault().Key;
            case EvictionPolicy.FirstInFirstOut:
                return val.Items.OrderBy(x => x.Value?.CreateTime).FirstOrDefault().Key;
            case EvictionPolicy.RandomReplacement:
                return val.Items.TakeRandom().Key;
            default:
                throw new ArgumentOutOfRangeException(nameof(EvictionPolicy), $"Unknown {nameof(EvictionPolicy)} {evictionPolicy}.");
        }
    }

    public void AddFetchCount(Func<int> func)
    {
        _fetchCount.Add(func);
    }

    public IEnumerable<CacheTypeInfo> GetInfos()
    {
        return _caches.Values;
    }

    public IDictionary<string, CacheItemInfo> GetByType<T>()
    {
        return GetByType(typeof(T));
    }

    public IDictionary<string, CacheItemInfo> GetByType(Type type)
    {
        if (_caches.TryGetValue(type, out var data)) return data.Items;
        return new Dictionary<string, CacheItemInfo>();
    }

    public IEnumerable<HealthType> GetHealthTypes()
    {
        foreach (var healthType in _cacheOptions.GetConfiguredPersistTypes)
        {
            yield return new HealthType
            {
                Type = healthType.Name,
                GetHealthAsync = async () =>
                {
                    var persist = _persistLoader.GetPersist(healthType);
                    var result = await persist.CanConnectAsync();

                    return new HealthDto
                    {
                        Message = result.Message,
                        Success = result.Success
                    };
                }
            };
        }
    }

    public int GetFetchQueueCount()
    {
        return _fetchCount.Select(x => x.Invoke()).Sum(x => x);
    }

    /// <summary>
    /// Starts evicting the stale items. Each item is dropped from the persist configured for its type; a failed
    /// drop is logged and the item stays tracked, so it is tried again on the next run.
    /// </summary>
    public void ClearStale()
    {
        _ = ClearStaleAsync();
    }

    /// <summary>
    /// Starts evicting every item. Each item is dropped from the persist configured for its type; a failed drop is
    /// logged and the item stays tracked.
    /// </summary>
    public void ClearAll()
    {
        _ = ClearAllAsync();
    }

    public Task ClearStaleAsync()
    {
        var items = GetInfos()
            .Where(x => !x.StaleWhileRevalidate)
            .SelectMany(info => info.Items.Where(x => x.Value.IsStale).Select(x => (Info: info, Key: x.Key)))
            .ToArray();
        return EvictAsync(items);
    }

    public Task ClearAllAsync()
    {
        var items = GetInfos()
            .SelectMany(info => info.Items.Select(x => (Info: info, Key: x.Key)))
            .ToArray();
        return EvictAsync(items);
    }

    /// <summary>
    /// Never throws: every failure is caught and logged per item, so a caller that does not await the returned
    /// task cannot crash the process or leave an unobserved exception.
    /// </summary>
    private async Task EvictAsync((CacheTypeInfo Info, string Key)[] items)
    {
        foreach (var item in items)
        {
            try
            {
                var persist = _persistLoader.GetPersist(item.Info.PersistType);
                await DropFromPersistAsync(persist, item.Info.Type, item.Key);
                Drop(item.Info.Type, item.Key);
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Failed to evict key '{Key}' of type '{Type}' from persist '{PersistType}'; it is kept and tried again later ({Message}).", item.Key, item.Info.Type.Name, item.Info.PersistType?.Name, ex.Message);
            }
        }
    }

    private static Task DropFromPersistAsync(IPersist persist, Type type, Key key)
    {
        try
        {
            return (Task)DropAsyncMethod.MakeGenericMethod(type).Invoke(persist, [key])!;
        }
        catch (TargetInvocationException ex) when (ex.InnerException != null)
        {
            ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
            throw;
        }
    }
}