namespace Tharga.Cache;

public record CacheItemInfo
{
    private int _accessCount;

    public required int Size { get; init; }
    public int AccessCount => _accessCount;

    public CacheItemInfo SetAccess()
    {
        _accessCount++;
        return this;
    }
}