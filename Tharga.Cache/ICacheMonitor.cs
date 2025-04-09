namespace Tharga.Cache;

public interface ICacheMonitor
{
    public event EventHandler<DataSetEventArgs> DataSetEvent;

    IEnumerable<CacheTypeInfo> GetInfos();
    Dictionary<string, CacheItemInfo> GetByType<T>();
    Dictionary<string, CacheItemInfo> GetByType(Type type);
    Task<HealthDto> GetHealthAsync();
    int GetFetchQueueCount();
}