using Microsoft.Extensions.DependencyInjection;
using Tharga.Cache.Persist;

namespace Tharga.Cache;

public record CacheOptions
{
    private readonly Dictionary<Type, CacheTypeOptions> _typeOptions = new();
    private readonly List<Action<IServiceCollection>> _callbacks = new();

    public int MaxConcurrentFetchCount { get; set; } = 10;

    /// <summary>
    /// The interval for the backgroud job that cleans out stale data.
    /// </summary>
    public TimeSpan WatchDogInterval { get; set; } = TimeSpan.FromSeconds(60);

    //TODO: Enable this method in fugure version, so that memory will be default.
    //public void RegisterType<TCache>(Action<CacheTypeOptions> options = null)
    //{
    //    RegisterType<TCache, IMemory>(options);
    //}

    public void RegisterType<TCache, TPersist>(Action<CacheTypeOptions> options = null) where TPersist : IPersist
    {
        var typeOptions = (Default ?? BuildDefault()) with { };
        typeOptions.PersistType = typeof(TPersist);
        options?.Invoke(typeOptions);
        if (!_typeOptions.TryAdd(typeof(TCache), typeOptions)) throw new InvalidOperationException($"The type '{typeof(TCache).Name}' has already been registered.");
    }

    internal CacheTypeOptions Get<T>()
    {
        return _typeOptions.GetValueOrDefault(typeof(T))
               ?? Default
               ?? BuildDefault();
    }

    public static CacheTypeOptions BuildDefault()
    {
        return new CacheTypeOptions
        {
            StaleWhileRevalidate = false,
            PersistType = typeof(IMemory)
        };
    }

    public CacheTypeOptions Default { get; set; }

    internal IEnumerable<Type> GetConfiguredPersistTypes => _typeOptions.Values.Select(x => x.PersistType).Distinct();

    internal void RegisterConfigurations(IServiceCollection serviceCollection)
    {
        foreach (var callback in _callbacks)
        {
            callback.Invoke(serviceCollection);
        }
    }

    internal void RegistrationCallback(Action<IServiceCollection> callback)
    {
        _callbacks.Add(callback);
    }
}