namespace Tharga.Cache.Core;

internal interface IManagedCacheMonitor : ICacheMonitor
{
    /// <summary>
    /// Evicts the stale items from the persist configured for each item's type. Never throws.
    /// </summary>
    Task ClearStaleAsync();

    /// <summary>
    /// Evicts every item from the persist configured for each item's type. Never throws.
    /// </summary>
    Task ClearAllAsync();

    void Set<T>(Type type, Key key, CacheItem<T> item, bool staleWhileRevalidate, bool returnDefaultOnFirstLoad);
    void Track<T>(Type type, Key key, CacheItem<T> item, bool staleWhileRevalidate, bool returnDefaultOnFirstLoad);
    void Accessed(Type type, Key key, bool buyMoreTime);
    void Drop(Type type, Key key);
    Key Get<T>(EvictionPolicy evictionPolicy);
    void AddFetchCount(Func<int> func);
}