﻿using Moq;
using Tharga.Cache.Core;

namespace Tharga.Cache.Tests.Helper;

internal static class CacheTypeLoader
{
    public static (T Cache, ICacheMonitor Monitor) GetCache<T>(Type cacheType, EvictionPolicy? evictionPolicy, bool staleWhileRevalidate, TimeSpan? defaultFreshSpan = default, string connectionString = "LOCAL")
    {
        var item = GetCache(cacheType, evictionPolicy, staleWhileRevalidate, defaultFreshSpan, connectionString);
        return ((T)item.Cache, item.Monitor);
    }

    public static (ICache Cache, ICacheMonitor Monitor) GetCache(Type cacheType, EvictionPolicy? evictionPolicy, bool staleWhileRevalidate, TimeSpan? defaultFreshSpan = default, string connectionString = "LOCAL")
    {
        var options = new CacheOptions
        {
            ConnectionStringLoader = _ => connectionString
        };

        options.RegisterType<string>(s =>
        {
            s.StaleWhileRevalidate = staleWhileRevalidate;
            s.DefaultFreshSpan = defaultFreshSpan ?? TimeSpan.FromSeconds(10);
            s.EvictionPolicy = evictionPolicy ?? EvictionPolicy.FirstInFirstOut;
        });

        var persistLoader = new Mock<IPersistLoader>(MockBehavior.Strict);
        var cacheMonitor = new CacheMonitor(persistLoader.Object, options);
        //var persist = new MemoryPersistLoader(cacheMonitor);

        ICache cache;
        switch (cacheType.Name)
        {
            case nameof(GenericCache):
                cache = new GenericCache(cacheMonitor, persistLoader.Object, options);
                break;
            case nameof(GenericTimeCache):
                cache = new GenericTimeCache(cacheMonitor, persistLoader.Object, options);
                break;
            case nameof(EternalCache):
                cache = new EternalCache(cacheMonitor, persistLoader.Object, options);
                break;
            case nameof(TimeToLiveCache):
                cache = new TimeToLiveCache(cacheMonitor, persistLoader.Object, options);
                break;
            case nameof(TimeToIdleCache):
                cache = new TimeToIdleCache(cacheMonitor, persistLoader.Object, options);
                break;
            default:
                throw new ArgumentOutOfRangeException($"Unknown cache type '{cacheType.Name}'.");
        }

        return (cache, cacheMonitor);
    }
}