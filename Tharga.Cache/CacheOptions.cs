using Microsoft.Extensions.DependencyInjection;
using Tharga.Cache.Persist;

namespace Tharga.Cache;

public record CacheOptions
{
    private readonly Dictionary<Type, CacheTypeOptions> _typeOptions = new();
    private readonly List<Action<IServiceCollection>> _callbacks = new();

    public int MaxConcurrentFetchCount { get; set; } = 10;
    public TimeSpan WatchDogInterval { get; set; } = TimeSpan.FromSeconds(60);

    public void RegisterType<TCache>(Action<CacheTypeOptions> options = null)
    {
        RegisterType<TCache, IMemory>(options);
    }

    public void RegisterType<TCache, TPersist>(Action<CacheTypeOptions> options = null) where TPersist : IPersist
    {
        var typeOptions = Default with { };
        typeOptions.PersistType = typeof(TPersist);
        options?.Invoke(typeOptions);
        _typeOptions.TryAdd(typeof(TCache), typeOptions);
    }

    internal CacheTypeOptions Get<T>()
    {
        return _typeOptions.GetValueOrDefault(typeof(T)) ?? Default;
    }

    public CacheTypeOptions Default => new()
    {
        StaleWhileRevalidate = false,
        PersistType = typeof(IMemory)
    };

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