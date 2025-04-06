namespace Tharga.Cache;

public record CacheItem
{
    private int _count;
    private DateTime? _lastAccessTime;

    public CacheItem(object data, TimeSpan freshSpan)
    {
        Data = data;
        CreateTime = DateTime.UtcNow;
        FreshSpan = freshSpan;
    }

    public object Data { get; }
    public DateTime CreateTime { get; }
    //public DateTime? LastAccessTime => _lastAccessTime;
    public TimeSpan FreshSpan { get; }
    //public int Count => _count;

    //public void SetAccess()
    //{
    //    _lastAccessTime = DateTime.UtcNow;
    //    _count++;
    //}
}