namespace Tharga.Cache;

internal interface IManagedCacheMonitor : ICacheMonitor
{
    void Add(Type type, Key key, object data);
}

public interface ICacheMonitor
{
    public event EventHandler<DataSetEventArgs> DataSetEvent;

    IEnumerable<CacheTypeInfo> Get();
}