using System.Reflection.Emit;
using Microsoft.Extensions.Logging;
using Moq;

namespace Tharga.Cache.Tests;

internal static class CacheTypeLoader
{
    public static T GetCache<T>(Type cacheType, EvictionPolicy? evictionPolicy, bool staleWhileRevalidate, TimeSpan? defaultFreshSpan = default, string connectionString = "LOCAL")
    {
        return (T)GetCache(cacheType, evictionPolicy, staleWhileRevalidate, defaultFreshSpan, connectionString);
    }

    public static ICache GetCache(Type cacheType, EvictionPolicy? evictionPolicy, bool staleWhileRevalidate, TimeSpan? defaultFreshSpan = default, string connectionString = "LOCAL")
    {
        var cacheMonitor = new Mock<IManagedCacheMonitor>();

        var persist = new Memory();

        //var serviceProvider = new Mock<IServiceProvider>();
        //var hostEnvironment = new Mock<IHostEnvironment>();
        var loggerFactory = new Mock<ILoggerFactory>(MockBehavior.Loose);
        var options = new Options
        {
            DefaultFreshSpan = defaultFreshSpan,
            //EvictionPolicy = evictionPolicy,
            //StaleWhileRevalidate = staleWhileRevalidate,
        };

        switch (cacheType.Name)
        {
            case nameof(GenericCache):
                return new GenericCache(cacheMonitor.Object, persist, options);
            case nameof(GenericTimeCache):
                return new GenericTimeCache(cacheMonitor.Object, persist, options);
            case nameof(EternalCache):
                return new EternalCache(cacheMonitor.Object, persist, options);
            case nameof(TimeToLiveCache):
                return new TimeToLiveCache(cacheMonitor.Object, persist, options);
            default:
                throw new ArgumentOutOfRangeException($"Unknown cache type '{cacheType.Name}'.");
        }
    }
}