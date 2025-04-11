namespace Tharga.Cache;

public record CacheOptions
{
    private readonly Dictionary<Type, CacheTypeOptions> _typeOptions = new();

    public int MaxConcurrentFetchCount { get; set; } = 10;
    public Func<IServiceProvider, string> ConnectionStringLoader { get; set; }
    public TimeSpan WatchDogInterval { get; set; } = TimeSpan.FromSeconds(10);

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

    public CacheTypeOptions Default => new()
    {
        StaleWhileRevalidate = false,
        PersistType = PersistType.Memory
    };
}