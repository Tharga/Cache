namespace Tharga.Cache;

//namespace Tharga.Cache;

////public record DistributedCacheOptions : CacheOptions
////{
////    /// <summary>
////    /// Loader that returns the connection string for the redis cache.
////    /// This can also be configured in appsettings.json under the key "RedisCache:ConnectionString".
////    /// Special featured values for the connection string are...
////    /// - DISABLED: Disable the distributed cache and always fetch from the source.
////    /// - LOCAL: Use the local cache instead of the distributed cache. This can be used in development or testing environments when there is only one instance of the application running.
////    /// </summary>
////    public Func<IServiceProvider, string> ConnectionStringLoader { get; set; }
////}

//public record CacheOptions
//{
//    public TimeSpan? DefaultKeepTime { get; set; }
//    //public Action<ActionEventArgs> ActionEvent { get; set; }
//}

public record TypeOptions
{
    public bool StaleWhileRevalidate { get; set; }
    public long MaxSize { get; set; }
    public int MaxCount { get; set; }
    public EvictionPolicy EvictionPolicy { get; set; } = EvictionPolicy.FirstInFirstOut;
}

public record Options
{
    private readonly Dictionary<Type, TypeOptions> _typeOptions = new();

    //public TimeSpan? DefaultFreshSpan { get; set; }
    //public EvictionPolicy? EvictionPolicy { get; set; }
    //public long? MaxSize { get; set; }
    //public long? MaxCount { get; set; }
    //public int? MaxParallelSourceLoadCount { get; set; }
    //public PersistType PersistType { get; set; }
    //public bool StaleWhileRevalidate { get; set; }

    public void RegisterType<T>(Action<TypeOptions> options = null)
    {
        var typeOptions = Default with { };
        options?.Invoke(typeOptions);
        _typeOptions.TryAdd(typeof(T), typeOptions);
    }

    public TypeOptions Get<T>()
    {
        return _typeOptions.GetValueOrDefault(typeof(T)) ?? Default;
    }

    public TypeOptions Default => new() { StaleWhileRevalidate = false, MaxCount = 1000, MaxSize = 100 * Size.MB };
}

public static class Size
{
    public const long OneKilobyte = 1024;
    public const long OneMegabyte = OneKilobyte * 1024;
    public const long OneGigabyte = OneMegabyte * 1024;
    public const long OneTerabyte = OneGigabyte * 1024;

    // Aliases for convenience
    public const long KB = OneKilobyte;
    public const long MB = OneMegabyte;
    public const long GB = OneGigabyte;
    public const long TB = OneTerabyte;
}