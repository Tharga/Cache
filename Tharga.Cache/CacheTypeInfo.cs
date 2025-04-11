using System.Collections.Concurrent;

namespace Tharga.Cache;

public record CacheTypeInfo
{
    public required Type Type { get; init; }
    public required bool StaleWhileRevalidate { get; init; }
    public required ConcurrentDictionary<string, CacheItemInfo> Items { get; init; }
}