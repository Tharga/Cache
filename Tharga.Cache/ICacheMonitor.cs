namespace Tharga.Cache;

public interface ICacheMonitor
{
    event EventHandler<DataGetEventArgs> DataGetEvent;
    event EventHandler<DataSetEventArgs> DataSetEvent;
    event EventHandler<DataDropEventArgs> DataDropEvent;

    IEnumerable<CacheTypeInfo> GetInfos();
    IDictionary<string, CacheItemInfo> GetByType<T>();
    IDictionary<string, CacheItemInfo> GetByType(Type type);
    Task<HealthDto> GetHealthAsync();
    int GetFetchQueueCount();
    void CleanSale();
}