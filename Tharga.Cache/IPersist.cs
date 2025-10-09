namespace Tharga.Cache;

public interface IPersist
{
    Task<CacheItem<T>> GetAsync<T>(Key key);
    Task SetAsync<T>(Key key, CacheItem<T> cacheItem, bool staleWhileRevalidate);
    Task<bool> BuyMoreTime<T>(Key key);

    /// <summary>
    /// Change the time for the record so that it becomes invalid, but keeps the value in cache.
    /// This is needed when using StaleWhileRevalidate.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <returns></returns>
    Task<bool> Invalidate<T>(Key key);

    /// <summary>
    /// Returns true if something was removed.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    Task<bool> DropAsync(Key key);

    Task<(bool Success, string Message)> CanConnectAsync();
}