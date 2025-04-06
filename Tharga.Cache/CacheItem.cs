namespace Tharga.Cache;

public record CacheItem
{
    public CacheItem(object data, TimeSpan freshSpan)
    {
        Data = data;
        CreateTime = DateTime.UtcNow;
        FreshSpan = freshSpan;
    }

    public object Data { get; }
    public DateTime CreateTime { get; }
    public TimeSpan FreshSpan { get; }
}