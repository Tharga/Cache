namespace Tharga.Cache;

public interface IFetchQueue
{
    Task<T> LoadData<T>(Key key, Func<Task<T>> fetch, TimeSpan? freshSpan, Func<Key, CacheItem<T>, bool, Task> fetchCallback);
}