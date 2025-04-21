using Tharga.Console.Commands.Base;

namespace Tharga.Cache.Console.Commands;

internal class CacheFecthQueueCommand : AsyncActionCommandBase
{
    private readonly ICacheMonitor _cacheMonitor;

    public CacheFecthQueueCommand(ICacheMonitor cacheMonitor)
        : base("fetchqueue")
    {
        _cacheMonitor = cacheMonitor;
    }

    public override Task InvokeAsync(string[] param)
    {
        OutputInformation($"Queue count: {_cacheMonitor.GetFetchQueueCount()}");
        return Task.CompletedTask;
    }
}