using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Reflection;
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
            Default = CacheOptions.BuildDefault()
        };
        options?.Invoke(o);

        serviceCollection.AddSingleton(Options.Create(o));
        o.RegisterConfigurations(serviceCollection);
        //serviceCollection.AddSingleton(typeof(object), o._cfg);

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
        RegisterAllIPersistImplementations(serviceCollection, o);

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

        InvokeAllPersistRegistrations(serviceCollection);
    }

    private static void RegisterPersist(IServiceCollection serviceCollection)
    {
        serviceCollection.AddTransient<IPersistLoader, PersistLoader>();
        serviceCollection.AddSingleton<IPersist>(_ => throw new InvalidOperationException($"Cannot inject {nameof(IPersist)} directly, use {nameof(IPersistLoader)} instead."));
        serviceCollection.AddSingleton<IMemory, Memory>();
    }

    private static void RegisterAllIPersistImplementations(IServiceCollection services, CacheOptions options)
    {
        var assemblies = AppDomain.CurrentDomain
            .GetAssemblies()
            .Where(a => !a.IsDynamic && !a.FullName.StartsWith("System") && !a.FullName.StartsWith("Microsoft"));

        foreach (var assembly in assemblies)
        {
            RegisterIPersistFromAssembly(services, assembly, typeof(IPersist));
        }
    }

    private static void RegisterIPersistFromAssembly(IServiceCollection services, Assembly assembly, Type ipersistType)
    {
        // Find all interfaces extending IPersist (excluding IPersist itself)
        var persistInterfaces = assembly.GetTypes()
            .Where(t => t.IsInterface && ipersistType.IsAssignableFrom(t) && t != ipersistType)
            .ToList();

        foreach (var iface in persistInterfaces)
        {
            // Find a non-abstract, non-interface class that implements this interface
            var implementation = assembly.GetTypes()
                .FirstOrDefault(c =>
                    c.IsClass &&
                    !c.IsInterface &&
                    !c.IsAbstract &&
                    iface.IsAssignableFrom(c));

            if (implementation != null)
            {
                services.AddSingleton(iface, implementation);
            }
        }
    }

    private static void InvokeAllPersistRegistrations(IServiceCollection services)
    {
        var registrationType = typeof(IPersistRegistration);
        var assemblies = AppDomain.CurrentDomain
            .GetAssemblies()
            .Where(a => !a.IsDynamic && !a.FullName.StartsWith("System") && !a.FullName.StartsWith("Microsoft"));

        foreach (var assembly in assemblies)
        {
            foreach (var type in assembly.GetTypes())
            {
                if (!registrationType.IsAssignableFrom(type)) continue;
                if (type.IsAbstract || type.IsInterface) continue;

                // Check for parameterless constructor
                if (type.GetConstructor(Type.EmptyTypes) is not ConstructorInfo ctor)
                {
                    // Optional: log or throw
                    continue;
                }

                // Create and invoke
                var instance = (IPersistRegistration)ctor.Invoke(null);
                instance.Register(services);
            }
        }
    }
}