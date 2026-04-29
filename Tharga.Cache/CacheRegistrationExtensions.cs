using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Reflection;
using Tharga.Cache.Core;
using Tharga.Cache.Persist;

namespace Tharga.Cache;

public static class CacheRegistrationExtensions
{
    private static readonly Dictionary<Type, CacheTypeOptions> _configuredPersistTypes = new();

    internal static void ResetRegistrations()
    {
        _configuredPersistTypes.Clear();
    }

    public static void AddCache(this IServiceCollection serviceCollection, Action<CacheOptions> options = null)
    {
        var o = new CacheOptions
        {
            Default = CacheOptions.BuildDefault()
        };
        options?.Invoke(o);

        AppendPreviousRegistrations(o);

        // Replace IOptions<CacheOptions> on each call so it carries the merged type registrations.
        serviceCollection.RemoveAll<IOptions<CacheOptions>>();
        serviceCollection.AddSingleton(Options.Create(o));

        serviceCollection.TryAddSingleton<ICacheMonitor>(s => s.GetService<IManagedCacheMonitor>());
        serviceCollection.TryAddSingleton<IFetchQueue>(s =>
        {
            var cacheMonitor = s.GetService<IManagedCacheMonitor>();
            var loggerFactory = s.GetService<ILoggerFactory>();
            var logger = loggerFactory?.CreateLogger<FetchQueue>();
            return new FetchQueue(cacheMonitor, o, logger);
        });
        serviceCollection.TryAddSingleton<IManagedCacheMonitor>(s =>
        {
            var persistLoader = s.GetService<IPersistLoader>();
            var cacheMonitor = new CacheMonitor(persistLoader, o);
            return cacheMonitor;
        });

        RegisterPersist(serviceCollection);
        RegisterAllIPersistImplementations(serviceCollection, o);

        serviceCollection.TryAddSingleton<ICache>(s =>
        {
            throw new NotImplementedException($"Direct use of {nameof(ICache)} has not yet been implemented.");
        });
        serviceCollection.TryAddSingleton<ITimeCache>(s =>
        {
            throw new NotImplementedException($"Direct use of {nameof(ITimeCache)} has not yet been implemented.");
        });
        serviceCollection.TryAddSingleton<IEternalCache>(s =>
        {
            var cacheMonitor = s.GetService<IManagedCacheMonitor>();
            var persistLoader = s.GetService<IPersistLoader>();
            var fetchQueue = s.GetService<IFetchQueue>();
            return new EternalCache(cacheMonitor, persistLoader, fetchQueue, o);
        });
        serviceCollection.TryAddSingleton<ITimeToLiveCache>(s =>
        {
            var cacheMonitor = s.GetService<IManagedCacheMonitor>();
            var persistLoader = s.GetService<IPersistLoader>();
            var fetchQueue = s.GetService<IFetchQueue>();
            return new TimeToLiveCache(cacheMonitor, persistLoader, fetchQueue, o);
        });
        serviceCollection.TryAddSingleton<ITimeToIdleCache>(s =>
        {
            var cacheMonitor = s.GetService<IManagedCacheMonitor>();
            var persistLoader = s.GetService<IPersistLoader>();
            var fetchQueue = s.GetService<IFetchQueue>();
            return new TimeToIdleCache(cacheMonitor, persistLoader, fetchQueue, o);
        });
        serviceCollection.TryAddScoped<IScopeCache>(s =>
        {
            var cacheMonitor = s.GetService<IManagedCacheMonitor>();
            var persistLoader = s.GetService<IPersistLoader>();
            var fetchQueue = s.GetService<IFetchQueue>();
            return new EternalCache(cacheMonitor, persistLoader, fetchQueue, o);
        });

        serviceCollection.TryAddSingleton<IWatchDogService, WatchDogService>();
        if (!serviceCollection.Any(s => s.ServiceType == typeof(Microsoft.Extensions.Hosting.IHostedService) && s.ImplementationType == typeof(WatchDogService)))
        {
            serviceCollection.AddHostedService<WatchDogService>();
        }

        InvokeAllPersistRegistrations(serviceCollection);

        o.RegisterConfigurations(serviceCollection);
    }

    /// <summary>
    /// If AddCache is called several times, this method merges all registrations so they can be used in the end.
    /// First registration wins — duplicate types are silently skipped.
    /// </summary>
    private static void AppendPreviousRegistrations(CacheOptions o)
    {
        var previouslyRegisteredTypes = _configuredPersistTypes.ToArray();
        foreach (var item in o.GetRegistered())
        {
            _configuredPersistTypes.TryAdd(item.Key, item.Value);
        }
        foreach (var previouslyRegisteredType in previouslyRegisteredTypes)
        {
            o.TryAddType(previouslyRegisteredType.Key, previouslyRegisteredType.Value);
        }
    }

    private static void RegisterPersist(IServiceCollection serviceCollection)
    {
        serviceCollection.TryAddTransient<IPersistLoader, PersistLoader>();
        serviceCollection.TryAddSingleton<IPersist>(_ => throw new InvalidOperationException($"Cannot inject {nameof(IPersist)} directly, use {nameof(IPersistLoader)} instead."));
        serviceCollection.TryAddSingleton<IMemory, Memory>();
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
        var types = GetTypesSafe(assembly);

        // Find all interfaces extending IPersist (excluding IPersist itself)
        var persistInterfaces = types
            .Where(t => t.IsInterface && ipersistType.IsAssignableFrom(t) && t != ipersistType)
            .ToList();

        foreach (var iface in persistInterfaces)
        {
            // Find a non-abstract, non-interface class that implements this interface
            var implementation = types
                .FirstOrDefault(c =>
                    c.IsClass &&
                    !c.IsInterface &&
                    !c.IsAbstract &&
                    iface.IsAssignableFrom(c));

            if (implementation != null)
            {
                services.TryAddSingleton(iface, implementation);
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
            foreach (var type in GetTypesSafe(assembly))
            {
                if (!registrationType.IsAssignableFrom(type)) continue;
                if (type.IsAbstract || type.IsInterface) continue;

                // Check for parameterless constructor
                if (type.GetConstructor(Type.EmptyTypes) is not ConstructorInfo ctor)
                {
                    continue;
                }

                // Create and invoke
                var instance = (IPersistRegistration)ctor.Invoke(null);
                instance.Register(services);
            }
        }
    }

    private static readonly HashSet<string> _warnedAssemblies = new();
    private static readonly object _warnLock = new();

    internal static Type[] GetTypesSafe(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            var loaded = ex.Types.Where(t => t != null).Cast<Type>().ToArray();
            var missed = ex.Types.Length - loaded.Length;

            // Warn once per assembly so the root cause isn't silently swallowed.
            var name = assembly.FullName ?? "(unknown)";
            bool warn = false;
            lock (_warnLock)
            {
                if (_warnedAssemblies.Add(name)) warn = true;
            }
            if (warn)
            {
                Console.Error.WriteLine(
                    $"[Tharga.Cache] Skipped {missed} unresolvable type(s) in '{name}' during assembly scan: {ex.LoaderExceptions.FirstOrDefault()?.Message}");
            }

            return loaded;
        }
    }
}