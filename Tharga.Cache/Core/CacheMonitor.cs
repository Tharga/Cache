using System.Collections.Concurrent;

namespace Tharga.Cache.Core;

internal class CacheMonitor : IManagedCacheMonitor
{
    private readonly IPersistLoader _persistLoader;
    private readonly CacheOptions _cacheOptions;
    private readonly ConcurrentDictionary<Type, CacheTypeInfo> _caches = new();
    private readonly List<Func<int>> _fetchCount = new();

    public CacheMonitor(IPersistLoader persistLoader, CacheOptions cacheOptions)
    {
        _persistLoader = persistLoader;
        _cacheOptions = cacheOptions;
    }

    public event EventHandler<RequestEvictEventArgs> RequestEvictEvent;
    public event EventHandler<DataGetEventArgs> DataGetEvent;
    public event EventHandler<DataSetEventArgs> DataSetEvent;
    public event EventHandler<DataDropEventArgs> DataDropEvent;

    public void Set<T>(Type type, Key key, CacheItem<T> item, bool staleWhileRevalidate)
    {
        var size = item.Data.ToSize();

        _caches.AddOrUpdate(type, new CacheTypeInfo
        {
            Type = type,
            StaleWhileRevalidate = staleWhileRevalidate,
            Items = new ConcurrentDictionary<string, CacheItemInfo>(new Dictionary<string, CacheItemInfo>
            {
                {
                    key, new CacheItemInfo(item.CreateTime)
                    {
                        Size = size,
                        FreshSpan = item.FreshSpan
                    }
                }
            })
        }, (_, b) =>
        {
            b.Items.AddOrUpdate(key, new CacheItemInfo(item.CreateTime)
            {
                Size = size,
                FreshSpan = item.FreshSpan
            }, (_, c) =>
            {
                c.SetUpdated(item.CreateTime, item.UpdateTime);
                return c;
            });
            return b;
        });

        DataSetEvent?.Invoke(this, new DataSetEventArgs(key, item.Data));
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

    public void ClearStale()
    {
        var infos = GetInfos().Where(x => !x.StaleWhileRevalidate).ToArray();
        foreach (var info in infos)
        {
            foreach (var item in info.Items.Where(x => x.Value.IsStale))
            {
                RequestEvictEvent?.Invoke(this, new RequestEvictEventArgs(info.Type, item.Key));
            }
        }
    }

    public void ClearAll()
    {
        foreach (var info in GetInfos())
        {
            foreach (var item in info.Items)
            {
                RequestEvictEvent?.Invoke(this, new RequestEvictEventArgs(info.Type, item.Key));
            }
        }
    }
}