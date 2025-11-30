using Tharga.Console.Commands.Base;

namespace Tharga.Cache.Console.Commands;

internal class CacheInfoCommand : AsyncActionCommandBase
{
    private readonly ICacheMonitor _cacheMonitor;

    public CacheInfoCommand(ICacheMonitor cacheMonitor)
        : base("info")
    {
        _cacheMonitor = cacheMonitor;
    }

    public override async Task InvokeAsync(string[] param)
    {
        var items = _cacheMonitor.GetByType<string>().Select(x => new []
        {
            $"{x.Key}",
            $"{x.Value.Size}",
            $"{x.Value.AccessCount}",
            $"{x.Value.CreateTime}",
            $"{x.Value.UpdateTime}",
            $"{x.Value.ExpireTime}",
            $"{x.Value.LastAccessTime}",
            $"{x.Value.FreshSpan}",
            $"{x.Value.IsStale}",
        });
        OutputTable(["Key", "Size", "ACCCount", "Created", "Updated", "Expire", "Accessed", "FreshSpan", "Stale"], items);
    }
}