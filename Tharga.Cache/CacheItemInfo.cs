namespace Tharga.Cache;

public record CacheItemInfo
{
    private DateTime _createTime;
    private DateTime? _updateTime;
    private DateTime? _lastAccessTime;
    private int _accessCount;

    public CacheItemInfo(DateTime createTime)
    {
        _createTime = createTime;
    }

    public required int Size { get; init; }
    public required TimeSpan? FreshSpan { get; init; }
    public DateTime CreateTime => _createTime;
    public DateTime? UpdateTime => _updateTime;
    public DateTime? ExpireTime => FreshSpan.HasValue ? (UpdateTime ?? CreateTime).Add(FreshSpan.Value) : null;
    public DateTime? LastAccessTime => _lastAccessTime;
    public int AccessCount => _accessCount;
    public bool IsStale => ExpireTime.HasValue && DateTime.UtcNow > ExpireTime;

    public CacheItemInfo SetAccess()
    {
        _accessCount++;
        _lastAccessTime = DateTime.UtcNow;
        return this;
    }

    public CacheItemInfo SetUpdated(DateTime createTime, DateTime? updateTime)
    {
        _createTime = createTime;
        _updateTime = updateTime;
        return this;
    }
}