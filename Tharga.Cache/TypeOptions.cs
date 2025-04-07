namespace Tharga.Cache;

public record TypeOptions
{
    public bool StaleWhileRevalidate { get; set; }
    public long MaxSize { get; set; }
    public int MaxCount { get; set; }
    public EvictionPolicy EvictionPolicy { get; set; } = EvictionPolicy.FirstInFirstOut;
}