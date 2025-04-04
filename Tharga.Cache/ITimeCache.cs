namespace Tharga.Cache;

public interface ITimeCache : ICache
{
    Task<T> GetAsync<T>(Key key, Func<Task<T>> fetch, TimeSpan freshSpan);
    Task SetAsync<T>(Key key, T data, TimeSpan freshSpan);
}