namespace Tharga.Cache.Persist;

internal static class CacheItemBuilder
{
    public static CacheItem<T> BuildCacheItem<T>(Dictionary<string,string> parts, T data, TimeSpan? freshSpan, TimeSpan? loadDuration = null, DateTime? createTime = null)
    {
        return new CacheItem<T> { KeyParts = parts, Data = data, FreshSpan = freshSpan, CreateTime = createTime ?? DateTime.UtcNow, LoadDuration = loadDuration };
    }
}