namespace Tharga.Cache.Core;

internal interface IManagedCacheMonitor : ICacheMonitor
{
    void Set(Type type, Key key, object data);
    void Get(Type type, Key key);
    void Drop(Type type, Key key);
}