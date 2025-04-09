namespace Tharga.Cache;

public record CacheTypeOptions
{
    public bool StaleWhileRevalidate { get; set; }
    public long MaxSize { get; set; }
    public int MaxCount { get; set; }
    public EvictionPolicy EvictionPolicy { get; set; } = EvictionPolicy.FirstInFirstOut;
    public PersistType PersistType { get; set; } = PersistType.Memory;
    public TimeSpan? DefaultFreshSpan { get; set; }
}