namespace Tharga.Cache;

public interface ICacheMonitor
{
    public event EventHandler<DataGetEventArgs> DataGetEvent;
    public event EventHandler<DataSetEventArgs> DataSetEvent;
    public event EventHandler<DataDropEventArgs> DataDropEvent;

    IEnumerable<CacheTypeInfo> GetInfos();
    Dictionary<string, CacheItemInfo> GetByType<T>();
    Dictionary<string, CacheItemInfo> GetByType(Type type);
    Task<HealthDto> GetHealthAsync();
    int GetFetchQueueCount();
}