namespace Tharga.Cache;

public interface ICache
{
    event EventHandler<DataSetEventArgs> DataSetEvent;

    Task<T> GetAsync<T>(Key key, Func<Task<T>> fetch);
    Task<T> PeekAsync<T>(Key key);
    Task SetAsync<T>(Key key, T data);
    Task DropAsync<T>(Key key);
}