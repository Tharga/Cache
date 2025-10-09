using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Tharga.Cache.Core;
using Tharga.Cache.Persist;

namespace Tharga.Cache;

public static class CacheRegistrationExtensions
{
    [Obsolete($"Use {nameof(AddCache)} instead.")]
    public static void RegisterCache(this IServiceCollection serviceCollection, Action<CacheOptions> options = null)
    {
        AddCache(serviceCollection, options);
    }

    public static void AddCache(this IServiceCollection serviceCollection, Action<CacheOptions> options = null)
    {
        var o = new CacheOptions
        {
            ConnectionStringLoader = serviceProvider =>
            {
                var configuration = serviceProvider.GetService<IConfiguration>();
                var connectionString = configuration?.GetSection("ThargaCache:ConnectionString").Value;
                return connectionString;
            }
        };
        options?.Invoke(o);

        serviceCollection.AddSingleton(Options.Create(o));
        serviceCollection.AddSingleton<ICacheMonitor>(s => s.GetService<IManagedCacheMonitor>());
        serviceCollection.AddSingleton<IFetchQueue>(s =>
        {
            var cacheMonitor = s.GetService<IManagedCacheMonitor>();
            var loggerFactory = s.GetService<ILoggerFactory>();
            var logger = loggerFactory?.CreateLogger<FetchQueue>();
            return new FetchQueue(cacheMonitor, o, logger);
        });
        serviceCollection.AddSingleton<IManagedCacheMonitor>(s =>
        {
            var persistLoader = s.GetService<IPersistLoader>();
            var cacheMonitor = new CacheMonitor(persistLoader, o);
            return cacheMonitor;
        });

        RegisterPersist(serviceCollection);

        serviceCollection.AddSingleton<ICache>(s =>
        {
            throw new NotImplementedException($"Direct use of {nameof(ICache)} has not yet been implemented.");
            var cacheMonitor = s.GetService<IManagedCacheMonitor>();
            var persistLoader = s.GetService<IPersistLoader>();
            var fetchQueue = s.GetService<IFetchQueue>();
            return new GenericCache(cacheMonitor, persistLoader, fetchQueue, o);
        });
        serviceCollection.AddSingleton<ITimeCache>(s =>
        {
            throw new NotImplementedException($"Direct use of {nameof(ITimeCache)} has not yet been implemented.");
            var cacheMonitor = s.GetService<IManagedCacheMonitor>();
            var persistLoader = s.GetService<IPersistLoader>();
            var fetchQueue = s.GetService<IFetchQueue>();
            return new GenericTimeCache(cacheMonitor, persistLoader, fetchQueue, o);
        });
        serviceCollection.AddSingleton<IEternalCache>(s =>
        {
            var cacheMonitor = s.GetService<IManagedCacheMonitor>();
            var persistLoader = s.GetService<IPersistLoader>();
            var fetchQueue = s.GetService<IFetchQueue>();
            return new EternalCache(cacheMonitor, persistLoader, fetchQueue, o);
        });
        serviceCollection.AddSingleton<ITimeToLiveCache>(s =>
        {
            var cacheMonitor = s.GetService<IManagedCacheMonitor>();
            var persistLoader = s.GetService<IPersistLoader>();
            var fetchQueue = s.GetService<IFetchQueue>();
            return new TimeToLiveCache(cacheMonitor, persistLoader, fetchQueue, o);
        });
        serviceCollection.AddSingleton<ITimeToIdleCache>(s =>
        {
            var cacheMonitor = s.GetService<IManagedCacheMonitor>();
            var persistLoader = s.GetService<IPersistLoader>();
            var fetchQueue = s.GetService<IFetchQueue>();
            return new TimeToIdleCache(cacheMonitor, persistLoader, fetchQueue, o);
        });
        serviceCollection.AddScoped<IScopeCache>(s =>
        {
            var cacheMonitor = s.GetService<IManagedCacheMonitor>();
            var persistLoader = s.GetService<IPersistLoader>();
            var fetchQueue = s.GetService<IFetchQueue>();
            return new EternalCache(cacheMonitor, persistLoader, fetchQueue, o);
        });

        serviceCollection.AddSingleton<IWatchDogService, WatchDogService>();
        serviceCollection.AddHostedService<WatchDogService>();
    }

    private static void RegisterPersist(IServiceCollection serviceCollection)
    {
        serviceCollection.AddTransient<IPersistLoader, PersistLoader>();
        serviceCollection.AddSingleton<IPersist>(_ => throw new InvalidOperationException($"Cannot inject {nameof(IPersist)} directly, use {nameof(IPersistLoader)} instead."));
        serviceCollection.AddSingleton<IMemory, Memory>();
        //serviceCollection.AddSingleton<IRedis, Redis>();
        //serviceCollection.AddSingleton<IMemoryWithRedis, MemoryWithRedis>();
    }
}