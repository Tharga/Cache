namespace Tharga.Cache.Core;

internal interface IManagedCacheMonitor : ICacheMonitor
{
    void Set(Type type, Key key, object data);
    void Accessed(Type type, Key key, bool buyMoreTime);
    void Drop(Type type, Key key);
    Key Get<T>(EvictionPolicy evictionPolicy);
    Func<int> QueueCountLoader { get; set; }
}