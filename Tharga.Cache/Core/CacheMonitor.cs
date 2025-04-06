using System.Collections.Concurrent;
using System.Text.Json;

namespace Tharga.Cache.Core;

internal class CacheMonitor : IManagedCacheMonitor
{
    private readonly ConcurrentDictionary<Type, CacheTypeInfo> _caches = new();

    public void Set(Type type, Key key, object data)
    {
        var bytes = JsonSerializer.SerializeToUtf8Bytes(data);

        _caches.AddOrUpdate(type, new CacheTypeInfo
        {
            Type = type,
            Items = new Dictionary<string, CacheItemInfo>
            {
                {
                    key, new CacheItemInfo
                    {
                        Size = bytes.Length
                    }
                }
            }
        }, (_, b) =>
        {
            b.Items.TryAdd(key, new CacheItemInfo
            {
                Size = bytes.Length
            });
            return b;
        });

        DataSetEvent?.Invoke(this, new DataSetEventArgs(key, data));
    }

    public void Get(Type type, Key key)
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

    public event EventHandler<DataSetEventArgs> DataSetEvent;

    public IEnumerable<CacheTypeInfo> GetInfos()
    {
        return _caches.Values;
    }
}