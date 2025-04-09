using System.Collections.Concurrent;

namespace Tharga.Cache.Core;

internal class CacheMonitor : IManagedCacheMonitor
{
    private readonly ConcurrentDictionary<Type, CacheTypeInfo> _caches = new();

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

    public void Accessed(Type type, Key key)
    {
        if (_caches.TryGetValue(type, out var info))
        {
            info.Items.FirstOrDefault(x => x.Key.Equals(key)).Value.SetAccess();
        }
        else
        {
            throw new NotImplementedException();
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
                return val.Items.OrderByDescending(x => x.Value.LastAccessTime ?? DateTime.MinValue).FirstOrDefault().Key;
            case EvictionPolicy.FirstInFirstOut:
                return val.Items.OrderBy(x => x.Value.CreateTime).FirstOrDefault().Key;
            case EvictionPolicy.RandomReplacement:
                return val.Items.TakeRandom().Key;
            default:
                throw new ArgumentOutOfRangeException(nameof(EvictionPolicy), $"Unknown {nameof(EvictionPolicy)} {evictionPolicy}.");
        }
    }

    public event EventHandler<DataSetEventArgs> DataSetEvent;

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
}