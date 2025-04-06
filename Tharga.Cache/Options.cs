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

public record Options
{
    //public TimeSpan? DefaultFreshSpan { get; set; }
    //public EvictionPolicy? EvictionPolicy { get; set; }
    //public bool StaleWhileRevalidate { get; set; }
    //public long? MaxSize { get; set; }
    //public long? MaxCount { get; set; }
    //public int? MaxParallelSourceLoadCount { get; set; }
    //public PersistType PersistType { get; set; }
}