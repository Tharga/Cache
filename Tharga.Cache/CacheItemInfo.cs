namespace Tharga.Cache;

public record CacheItemInfo
{
    private int _accessCount;
    private DateTime? _lastAccessTime;

    public required int Size { get; init; }
    public DateTime CreateTime { get; private set; } = DateTime.UtcNow;
    public DateTime? LastAccessTime => _lastAccessTime;
    public int AccessCount => _accessCount;

    public CacheItemInfo SetAccess()
    {
        _accessCount++;
        _lastAccessTime = DateTime.UtcNow;
        return this;
    }
}