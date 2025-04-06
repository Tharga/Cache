namespace Tharga.Cache.Core;

internal interface IManagedCacheMonitor : ICacheMonitor
{
    void Add(Type type, Key key, object data);
    void Drop(Type type, Key key);
}