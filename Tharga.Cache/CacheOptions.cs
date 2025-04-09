namespace Tharga.Cache;

public record CacheOptions
{
    private readonly Dictionary<Type, CacheTypeOptions> _typeOptions = new();

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
    //public string ConnectionString { get; set; }

    public void RegisterType<T>(Action<CacheTypeOptions> options = null)
    {
        var typeOptions = Default with { };
        options?.Invoke(typeOptions);
        _typeOptions.TryAdd(typeof(T), typeOptions);
    }

    internal CacheTypeOptions Get<T>()
    {
        return _typeOptions.GetValueOrDefault(typeof(T)) ?? Default;
    }

    internal IEnumerable<PersistType> GetConfiguredPersistTypes => _typeOptions.Values.Select(x => x.PersistType).Distinct();

    private CacheTypeOptions Default => new()
    {
        StaleWhileRevalidate = false,
        MaxCount = 1000,
        MaxSize = 100 * Size.MB,
        PersistType = PersistType.Memory
    };
}