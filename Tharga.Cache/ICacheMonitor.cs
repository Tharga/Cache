namespace Tharga.Cache;

public interface ICacheMonitor
{
    public event EventHandler<DataSetEventArgs> DataSetEvent;

    IEnumerable<CacheTypeInfo> Get();
}