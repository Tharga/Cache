namespace Tharga.Cache;

public interface ITimeCache : ICache
{
    Task<T> GetAsync<T>(Key key, Func<Task<T>> fetch, TimeSpan freshSpan);

    /// <summary>
    /// When using StaleWhileRevalidate stale data might be returned and the fresh data will be fetched later.
    /// If the data is Fresh this will be indicated by the Fresh flag set to true.
    /// Provide a method that will be called when fresh data has been loaded. If the data is fresh to start with, this method will never be called.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="fetch"></param>
    /// <param name="callback"></param>
    /// <param name="freshSpan"></param>
    /// <returns></returns>
    Task<(T Data, bool Fresh)> GetWithCallbackAsync<T>(Key key, Func<Task<T>> fetch, Func<T, Task> callback, TimeSpan? freshSpan = default);

    Task SetAsync<T>(Key key, T data, TimeSpan freshSpan);
}