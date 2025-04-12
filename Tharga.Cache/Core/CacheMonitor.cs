using System.Collections.Concurrent;
using Tharga.Cache.Persist;

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
        else
        {
            throw new NotImplementedException();
        }
    }

    public Key Get<T>(EvictionPolicy evictionPolicy)
    {
        if (!_caches.TryGetValue(typeof(T), out var val)) return default;

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

    public async Task<HealthDto> GetHealthAsync()
    {
        (bool Success, string Message) result = (true, null);
        var hasRedis = _cacheOptions.GetConfiguredPersistTypes.Any(x => x == PersistType.Redis || x == PersistType.MemoryWithRedis);
        if (hasRedis)
        {
            var redis = (IRedis)_persistLoader.GetPersist(PersistType.Redis);
            result = await redis.CanConnectAsync();
        }

        var totalSize = _caches.Values.Sum(x => x.Items.Sum(y => y.Value?.Size ?? 0));
        var totalCount = _caches.Values.Sum(x => x.Items.Count);

        return new HealthDto
        {
            Message = $"There are {totalCount} items cached with a size of {totalSize} bytes. {result.Message}".TrimEnd(),
            Success = result.Success,
        };
    }

    public int GetFetchQueueCount()
    {
        return _fetchCount.Select(x => x.Invoke()).Sum(x => x);
    }

    public void CleanSale()
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
}