namespace Tharga.Cache;

public interface IPersist
{
    Task<CacheItem<T>> GetAsync<T>(Key key);
    Task SetAsync<T>(Key key, T data, TimeSpan? freshSpan);
    //Task SetAsync<T>(Key key, CacheItem<T> cacheItem);
    Task<CacheItem<T>> DropAsync<T>(Key key);

    IAsyncEnumerable<CacheItem<T>> GetAsync<T>();
    Task<(Key Key, CacheItem<T> Item)> DropFirst<T>();
}