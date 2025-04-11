namespace Tharga.Cache.Core;

internal interface IManagedCacheMonitor : ICacheMonitor
{
    event EventHandler<RequestEvictEventArgs> RequestEvictEvent;

    void Set<T>(Type type, Key key, CacheItem<T> item, bool staleWhileRevalidate);
    void Accessed(Type type, Key key, bool buyMoreTime);
    void Drop(Type type, Key key);
    Key Get<T>(EvictionPolicy evictionPolicy);
    void AddFetchCount(Func<int> func);
}