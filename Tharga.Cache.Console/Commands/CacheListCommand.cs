using Tharga.Console.Commands.Base;

namespace Tharga.Cache.Console.Commands;

internal class CacheListCommand : AsyncActionCommandBase
{
    private readonly ICacheMonitor _cacheMonitor;

    public CacheListCommand(ICacheMonitor cacheMonitor)
        : base("list")
    {
        _cacheMonitor = cacheMonitor;
    }

    public override async Task InvokeAsync(string[] param)
    {
        var items = _cacheMonitor.GetInfos().Select(x => new[]
        {
            $"{x.Type}",
            $"{x.Items.Sum(y => y.Value.Size)}",
            $"{x.Items.Count}",
            $"{x.Items.Count(y => y.Value.IsStale)}"
        });
        OutputTable(["Type", "Size", "Count", "StaleCount"], items);
    }
}