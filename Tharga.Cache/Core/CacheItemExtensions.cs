namespace Tharga.Cache.Core;

internal static class CacheItemExtensions
{
    public static bool IsValid(this CacheItem item, DateTime utcNow)
    {
        if (item == null) return false;

        var time = utcNow - (item.UpdateTime ?? item.CreateTime);
        if (time > item.FreshSpan) return false;

        return true;
    }

    public static T GetData<T>(this CacheItem<T> item)
    {
        if (item == null) return default;
        return item.Data;
    }
}