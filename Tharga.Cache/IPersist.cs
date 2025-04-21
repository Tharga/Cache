namespace Tharga.Cache;

public interface IPersist
{
    Task<CacheItem<T>> GetAsync<T>(Key key);
    Task SetAsync<T>(Key key, CacheItem<T> item, bool staleWhileRevalidate);
    Task<bool> BuyMoreTime<T>(Key key);
    Task<bool> Invalidate<T>(Key key);
    Task<bool> DropAsync(Key key);
}