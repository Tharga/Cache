namespace Tharga.Cache.Core;

internal static class CacheItemExtensions
{
    public static bool IsValid(this CacheItem item)
    {
        if (item == null) return false;
        if ((DateTime.UtcNow - item.CreateTime) > item.FreshSpan) return false;
        return true;
    }

    public static T GetData<T>(this CacheItem item)
    {
        if (item == null) return default;
        //item.SetAccess();
        return (T)item.Data;
    }
}