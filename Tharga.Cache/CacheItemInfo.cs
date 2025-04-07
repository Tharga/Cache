namespace Tharga.Cache;

public record CacheItemInfo
{
    private int _count;

    public required int Size { get; init; }
    public int Count => _count;

    public CacheItemInfo SetAccess()
    {
        _count++;
        return this;
    }
}