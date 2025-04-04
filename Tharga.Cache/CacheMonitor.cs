using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.Json;

namespace Tharga.Cache;

public record CacheTypeInfo
{
    public required Type Type { get; init; }
    public required Dictionary<string, CacheItem> Items { get; init; }
}

public record CacheItem
{
    public required int Size { get; init; }
}

internal class CacheMonitor : IManagedCacheMonitor
{
    private readonly ConcurrentDictionary<Type, CacheTypeInfo> _caches = new();

    public void Add(Type type, Key key, object data)
    {
        var bytes = JsonSerializer.SerializeToUtf8Bytes(data);

        var x = _caches.AddOrUpdate(type, new CacheTypeInfo
        {
            Type = type,
            Items = new Dictionary<string, CacheItem>
            {
                {
                    key, new CacheItem
                    {
                        Size = bytes.Length
                    }
                }
            }
            //Datas = new Dictionary<string, object> { { key, data } },
            //Size = bytes.Length
        }, (a, b) =>
        {
            Debugger.Break();
            return b;
        });

        DataSetEvent?.Invoke(this, new DataSetEventArgs());
    }

    public event EventHandler<DataSetEventArgs> DataSetEvent;

    public IEnumerable<CacheTypeInfo> Get()
    {
        return _caches.Values;
    }
}

public interface IPersist
{
    Task<(bool Found, T Data)> GetAsync<T>(Key key);
    Task SetAsync<T>(Key key, T data);
}

public interface IMemory : IPersist
{
}
public interface IRedis : IPersist
{
}
public interface IMongoDB : IPersist
{
}
internal class Memory : IMemory
{
    private readonly ConcurrentDictionary<string, object> _datas = new();

    public async Task<(bool, T)> GetAsync<T>(Key key)
    {
        if (_datas.TryGetValue(key, out var val)) return (true, (T)val);
        return (false, (T)default);
    }

    public Task SetAsync<T>(Key key, T data)
    {
        _datas.AddOrUpdate(key, data, (_, _) => data);
        return Task.CompletedTask;
    }
}

//internal class Redis : IRedis
//{
//    /*
//     * * - MemoryCache (System.Runtime.Caching)
//     * - MemoryCache based on Microsoft.Extensions.Caching.Memory
//     * - Redis using StackExchange.Redis
//     */

//    /*
//     * Cache Topology / Strategy
//     * - Write-through
//     * - Write-behind
//     * - Cache-aside
//     * - Read-through
//     */

//    /*
//     * Admission Policies
//     * - Bloom Filter
//     * - Doorkeeper
//     */
//    public Task<(bool, T)> GetAsync<T>(Key key)
//    {
//        throw new NotImplementedException();
//    }

//    public Task SetAsync<T>(Key key, T data)
//    {
//        throw new NotImplementedException();
//    }
//}
//internal class MongoDB : IMongoDB
//{
//    public Task<(bool, T)> GetAsync<T>(Key key)
//    {
//        throw new NotImplementedException();
//    }

//    public Task SetAsync<T>(Key key, T data)
//    {
//        throw new NotImplementedException();
//    }
//}

public enum ExpirationPolicies
{
    /// <summary>
    /// TTL (Time To Live)
    /// Data expires a fixed time after insertion. Most common.
    /// </summary>
    TimeToLive,

    /// <summary>
    /// TTI (Time To Idle)
    /// Each time a cached item is accessed, its expiration clock is reset.
    /// </summary>
    TimeToIdle,

    /// <summary>
    /// Data expires at a fixed point in time regardless of access.
    /// </summary>
    Absolute,

    /// <summary>
    /// Data never expires unless explicitly removed.
    /// </summary>
    Eternal,
}

public enum PersistType
{
    Memory,
    Redis,
    MongoDB
}