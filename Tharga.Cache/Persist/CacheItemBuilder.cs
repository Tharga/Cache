namespace Tharga.Cache.Persist;

internal static class CacheItemBuilder
{
    public static CacheItem<T> BuildCacheItem<T>(T data, TimeSpan? freshSpan)
    {
        return new CacheItem<T> { Data = data, FreshSpan = freshSpan, CreateTime = DateTime.UtcNow };
    }
}