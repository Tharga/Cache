namespace Tharga.Cache;

public record Options
{
    private readonly Dictionary<Type, TypeOptions> _typeOptions = new();

    //public TimeSpan? DefaultFreshSpan { get; set; }
    //public int? MaxParallelSourceLoadCount { get; set; }
    //public PersistType PersistType { get; set; }

    /// <summary>
    /// Loader that returns the connection string for the redis cache.
    /// This can also be configured in appsettings.json under the key "RedisCache:ConnectionString".
    /// Special featured values for the connection string are...
    /// - DISABLED: Disable the distributed cache and always fetch from the source.
    /// - LOCAL: Use the local cache instead of the distributed cache. This can be used in development or testing environments when there is only one instance of the application running.
    /// </summary>
    public Func<IServiceProvider, string> ConnectionStringLoader { get; set; }

    public void RegisterType<T>(Action<TypeOptions> options = null)
    {
        var typeOptions = Default with { };
        options?.Invoke(typeOptions);
        _typeOptions.TryAdd(typeof(T), typeOptions);
    }

    internal TypeOptions Get<T>()
    {
        return _typeOptions.GetValueOrDefault(typeof(T)) ?? Default;
    }

    internal TypeOptions Default => new() { StaleWhileRevalidate = false, MaxCount = 1000, MaxSize = 100 * Size.MB };
}