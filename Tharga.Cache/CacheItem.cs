namespace Tharga.Cache;

public abstract record CacheItem
{
    public required Dictionary<string, string> KeyParts { get; init; }
    public required DateTime CreateTime { get; init; }
    public DateTime? UpdateTime { get; init; }
    public TimeSpan? FreshSpan { get; init; }
}

public record CacheItem<T> : CacheItem
{
    public required T Data { get; init; }
}