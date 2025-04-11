//using Tharga.Cache.Core;
//using Tharga.Cache.Persist;

//namespace Tharga.Cache.Tests.Helper;

//internal class MemoryPersistLoader : IPersistLoader
//{
//    private readonly ICacheMonitor _cacheMonitor;
//    private Memory _memory;

//    public MemoryPersistLoader(ICacheMonitor cacheMonitor)
//    {
//        _cacheMonitor = cacheMonitor;
//    }

//    public IPersist GetPersist(PersistType persistType)
//    {
//        return _memory ??= new Memory(_cacheMonitor);
//    }
//}