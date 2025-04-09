namespace Tharga.Cache;

public interface ICache
{
    event EventHandler<DataSetEventArgs> DataSetEvent;
    event EventHandler<DataGetEventArgs> DataGetEvent;
    event EventHandler<DataDropEventArgs> DataDropEvent;

    Task<T> GetAsync<T>(Key key, Func<Task<T>> fetch);
    Task<T> PeekAsync<T>(Key key);
    Task SetAsync<T>(Key key, T data);
    Task<bool> DropAsync<T>(Key key);
}