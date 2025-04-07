namespace Tharga.Cache;

public interface IPersist
{
    Task<CacheItem> GetAsync<T>(Key key);
    Task SetAsync<T>(Key key, T data, TimeSpan freshSpan);
    Task<CacheItem> DropAsync<T>(Key key);

    IAsyncEnumerable<CacheItem> GetAsync<T>();
    Task<(Key Key, CacheItem Item)> DropFirst();
}