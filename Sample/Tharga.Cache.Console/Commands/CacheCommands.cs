using Tharga.Console.Commands.Base;

namespace Tharga.Cache.Console.Commands;

internal class CacheCommands : ContainerCommandBase
{
    public CacheCommands(ICacheMonitor cacheMonitor)
        : base("cache")
    {
        cacheMonitor.DataSetEvent += (_, _) => { OutputEvent(nameof(cacheMonitor.DataSetEvent)); };
        cacheMonitor.DataGetEvent += (_, _) => { OutputEvent(nameof(cacheMonitor.DataGetEvent)); };
        cacheMonitor.DataDropEvent += (_, _) => { OutputEvent(nameof(cacheMonitor.DataDropEvent)); };

        RegisterCommand<CacheListCommand>();
        RegisterCommand<CacheInfoCommand>();
        RegisterCommand<CacheGetCommand>();
        RegisterCommand<CacheGetWithCallbackCommand>();
        RegisterCommand<CachePeekCommand>();
        RegisterCommand<CacheSetCommand>();
        RegisterCommand<CacheDropCommand>();
        RegisterCommand<CacheInvalidateCommand>();
        RegisterCommand<CacheFecthQueueCommand>();
    }
}