using Tharga.Console.Commands.Base;

namespace Tharga.Cache.Console.Commands;

internal class CacheCommands : ContainerCommandBase
{
    public CacheCommands(ICacheMonitor cacheMonitor)
        : base("cache")
    {
        cacheMonitor.DataSetEvent += (_, _) => { System.Console.WriteLine(nameof(cacheMonitor.DataSetEvent)); };

        RegisterCommand<CacheListCommand>();
        RegisterCommand<CacheGetCommand>();
        RegisterCommand<CachePeekCommand>();
        RegisterCommand<CacheSetCommand>();
        RegisterCommand<CacheDropCommand>();
    }
}