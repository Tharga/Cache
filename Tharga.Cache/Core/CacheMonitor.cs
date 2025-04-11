using System.Collections.Concurrent;
using Tharga.Cache.Persist;

namespace Tharga.Cache.Core;

internal class CacheMonitor : IManagedCacheMonitor
{
    private readonly IPersistLoader _persistLoader;
    private readonly CacheOptions _cacheOptions;
    private readonly ConcurrentDictionary<Type, CacheTypeInfo> _caches = new();

    public CacheMonitor(IPersistLoader persistLoader, CacheOptions cacheOptions)
    {
        _persistLoader = persistLoader;
        _cacheOptions = cacheOptions;
    }

    public Func<int> QueueCountLoader { get; set; }

    public void Set(Type type, Key key, object data)
    {
        var size = data.ToSize();

        _caches.AddOrUpdate(type, new CacheTypeInfo
        {
            Type = type,
            Items = new Dictionary<string, CacheItemInfo>
            {
                {
                    key, new CacheItemInfo
                    {
                        Size = size,
                    }
                }
            }
        }, (_, b) =>
        {
            b.Items.TryAdd(key, new CacheItemInfo
            {
                Size = size
            });
            return b;
        });

        DataSetEvent?.Invoke(this, new DataSetEventArgs(key, data));
    }

    public void Accessed(Type type, Key key, bool buyMoreTime)
    {
        if (_caches.TryGetValue(type, out var info))
        {
            info.Items.FirstOrDefault(x => key.Equals(x.Key)).Value?.SetAccess();
        }
    }

    public void Drop(Type type, Key key)
    {
        if (_caches.TryGetValue(type, out var info))
        {
            var redused = info.Items.Where(x => !x.Key.Equals(key)).ToDictionary();
            if (redused.Any())
            {
                var updated = info with { Items = redused };
                _caches.TryUpdate(type, updated, info);
            }
            else
            {
                _caches.TryRemove(type, out _);
            }
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

    public event EventHandler<DataGetEventArgs> DataGetEvent;
    public event EventHandler<DataSetEventArgs> DataSetEvent;
    public event EventHandler<DataDropEventArgs> DataDropEvent;

    public IEnumerable<CacheTypeInfo> GetInfos()
    {
        return _caches.Values;
    }

    public Dictionary<string, CacheItemInfo> GetByType<T>()
    {
        return GetByType(typeof(T));
    }

    public Dictionary<string, CacheItemInfo> GetByType(Type type)
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
        return QueueCountLoader?.Invoke() ?? -1;
    }
}