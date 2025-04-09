namespace Tharga.Cache;

public interface IPersist
{
    Task<CacheItem<T>> GetAsync<T>(Key key);
    Task SetAsync<T>(Key key, T data, TimeSpan? freshSpan, bool staleWhileRevalidate);
    Task<bool> BuyMoreTime<T>(Key key);
    Task<bool> DropAsync<T>(Key key);
}