namespace Tharga.Cache;

public record CacheTypeInfo
{
    public required Type Type { get; init; }
    public required Dictionary<string, CacheItemInfo> Items { get; init; }
}