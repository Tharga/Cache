namespace Tharga.Cache;

public record CacheItem
{
    //public required object Data { get; init; }
    public required DateTime CreateTime { get; init; }
    public TimeSpan? FreshSpan { get; init; }
}

public record CacheItem<T> : CacheItem
{
    public required T Data { get; init; }
}