using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.Json;

namespace Tharga.Cache.Core;

internal class CacheMonitor : IManagedCacheMonitor
{
    private readonly ConcurrentDictionary<Type, CacheTypeInfo> _caches = new();

    public void Add(Type type, Key key, object data)
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
        }, (a, b) =>
        {
            Debugger.Break();
            return b;
        });

        DataSetEvent?.Invoke(this, new DataSetEventArgs());
    }

    public void Drop(Type type, Key key)
    {
        if (_caches.TryGetValue(type, out var info))
        {
            var redused = info.Items.Where(x => !x.Key.Equals(key)).ToDictionary();
            var updated = info with { Items = redused };
            _caches.TryUpdate(type, updated, info);
        }
        else
        {
            throw new NotImplementedException();
        }
    }

    public event EventHandler<DataSetEventArgs> DataSetEvent;

    public IEnumerable<CacheTypeInfo> Get()
    {
        return _caches.Values;
    }
}