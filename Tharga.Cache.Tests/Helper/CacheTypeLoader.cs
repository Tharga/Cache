using Moq;
using Tharga.Cache.Core;
using Tharga.Cache.Persist;

namespace Tharga.Cache.Tests.Helper;

internal static class CacheTypeLoader
{
    public static (T Cache, ICacheMonitor Monitor) GetCache<T, TPersist>(Type cacheType, EvictionPolicy? evictionPolicy, bool staleWhileRevalidate, TimeSpan? defaultFreshSpan = null, string connectionString = "LOCAL")
        where T : ICache
        where TPersist : IPersist
    {
        var item = GetCache<TPersist>(cacheType, evictionPolicy, staleWhileRevalidate, defaultFreshSpan, connectionString);
        return ((T)item.Cache, item.Monitor);
    }

    public static (ICache Cache, ICacheMonitor Monitor) GetCache(Type cacheType, EvictionPolicy? evictionPolicy, bool staleWhileRevalidate, TimeSpan? defaultFreshSpan = null, string connectionString = "LOCAL")
    {
        return GetCache<IMemory>(cacheType, evictionPolicy, staleWhileRevalidate, defaultFreshSpan = null, connectionString);
    }

    public static (ICache Cache, ICacheMonitor Monitor) GetCache<TPersist>(Type cacheType, EvictionPolicy? evictionPolicy, bool staleWhileRevalidate, TimeSpan? defaultFreshSpan = null, string connectionString = "LOCAL")
        where TPersist : IPersist
    {
        var options = new CacheOptions
        {
            Default = new CacheTypeOptions
            {
                DefaultFreshSpan = defaultFreshSpan ?? TimeSpan.FromSeconds(10)
            }
        };

        options.RegisterType<string, TPersist>(s =>
        {
            s.StaleWhileRevalidate = staleWhileRevalidate;
            s.DefaultFreshSpan = defaultFreshSpan ?? TimeSpan.FromSeconds(10);
            s.EvictionPolicy = evictionPolicy ?? EvictionPolicy.FirstInFirstOut;
        });

        var persistLoader = new Mock<IPersistLoader>(MockBehavior.Strict);
        var cacheMonitor = new CacheMonitor(persistLoader.Object, options);
        persistLoader.Setup(x => x.GetPersist(It.IsAny<Type>())).Returns(new Memory(cacheMonitor));
        var fetchQueue = new FetchQueue(cacheMonitor, options, null);

        ICache cache;
        switch (cacheType.Name)
        {
            case nameof(GenericCache):
                cache = new GenericCache(cacheMonitor, persistLoader.Object, fetchQueue, options);
                break;
            case nameof(GenericTimeCache):
                cache = new GenericTimeCache(cacheMonitor, persistLoader.Object, fetchQueue, options);
                break;
            case nameof(EternalCache):
                cache = new EternalCache(cacheMonitor, persistLoader.Object, fetchQueue, options);
                break;
            case nameof(TimeToLiveCache):
                cache = new TimeToLiveCache(cacheMonitor, persistLoader.Object, fetchQueue, options);
                break;
            case nameof(TimeToIdleCache):
                cache = new TimeToIdleCache(cacheMonitor, persistLoader.Object, fetchQueue, options);
                break;
            default:
                throw new ArgumentOutOfRangeException($"Unknown cache type '{cacheType.Name}'.");
        }

        return (cache, cacheMonitor);
    }
}