using Microsoft.Extensions.DependencyInjection;
using Tharga.Cache.Core;

namespace Tharga.Cache;

public static class CacheRegistrationExtensions
{
    public static void RegisterCache(this IServiceCollection serviceCollection, Action<Options> options = null)
    {
        var o = new Options
        {
            //ConnectionStringLoader = serviceProvider =>
            //{
            //    var configuration = serviceProvider.GetService<IConfiguration>();
            //    var connectionString = configuration?.GetSection("RedisCache:ConnectionString").Value;
            //    return connectionString;
            //},
        };
        options?.Invoke(o);

        serviceCollection.AddSingleton<ICacheMonitor>(s => s.GetService<IManagedCacheMonitor>());
        serviceCollection.AddSingleton<IManagedCacheMonitor>(s =>
        {
            //var hostEnvironment = s.GetService<IHostEnvironment>();
            var cacheMonitor = new CacheMonitor(); //s, hostEnvironment, o);
            //cacheMonitor.Add();
            return cacheMonitor;
        });

        serviceCollection.AddSingleton<IPersist>(s =>
        {
            //TODO: Add configuration to pick correct type of persist.
            return new Memory();
        });

        serviceCollection.AddSingleton<ICache>(s =>
        {
            throw new NotImplementedException($"Direct use of {nameof(ICache)} has not yet been implemented.");
            var cacheMonitor = s.GetService<IManagedCacheMonitor>();
            var persist = s.GetService<IPersist>();
            return new GenericCache(cacheMonitor, persist, o);
        });
        serviceCollection.AddSingleton<ITimeCache>(s =>
        {
            throw new NotImplementedException($"Direct use of {nameof(ITimeCache)} has not yet been implemented.");
            var cacheMonitor = s.GetService<IManagedCacheMonitor>();
            var persist = s.GetService<IPersist>();
            return new GenericTimeCache(cacheMonitor, persist, o);
        });
        serviceCollection.AddSingleton<IEternalCache>(s =>
        {
            var cacheMonitor = s.GetService<IManagedCacheMonitor>();
            var persist = s.GetService<IPersist>();
            return new EternalCache(cacheMonitor, persist, o);
        });
        serviceCollection.AddSingleton<ITimeToLiveCache>(s =>
        {
            var cacheMonitor = s.GetService<IManagedCacheMonitor>();
            var persist = s.GetService<IPersist>();
            return new TimeToLiveCache(cacheMonitor, persist, o);
        });
        serviceCollection.AddSingleton<ITimeToIdleCache>(s =>
        {
            var cacheMonitor = s.GetService<IManagedCacheMonitor>();
            var persist = s.GetService<IPersist>();
            return new TimeToIdleCache(cacheMonitor, persist, o);
        });

        serviceCollection.AddScoped<IScopeCache>(s =>
        {
            var cacheMonitor = s.GetService<IManagedCacheMonitor>();
            var persist = s.GetService<IPersist>();
            return new EternalCache(cacheMonitor, persist, o);
        });
    }
}