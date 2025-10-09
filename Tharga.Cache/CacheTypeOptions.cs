namespace Tharga.Cache;

public record CacheTypeOptions
{
    /// <summary>
    /// If this is set to true, stale data will be returned directly and an update will be performed in the background.
    /// When the data is updated the event DataSetEvent will be fired and the method 'xx'.
    /// </summary>
    public bool StaleWhileRevalidate { get; set; }
    public long? MaxSize { get; set; }
    public int? MaxCount { get; set; }
    public EvictionPolicy EvictionPolicy { get; set; } = EvictionPolicy.FirstInFirstOut;
    public TimeSpan? DefaultFreshSpan { get; set; }

    internal Type PersistType { get; set; }
}