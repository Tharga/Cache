using Tharga.Console.Commands.Base;

namespace Tharga.Cache.Console.Commands;

internal class CacheGetWithCallbackCommand : AsyncActionCommandBase
{
    private readonly ITimeToLiveCache _timeToLiveCache;

    public CacheGetWithCallbackCommand(ITimeToLiveCache timeToLiveCache)
        : base("callback")
    {
        _timeToLiveCache = timeToLiveCache;
    }

    public override async Task InvokeAsync(string[] param)
    {
        var item = await _timeToLiveCache.GetWithCallbackAsync("key", async () =>
        {
            await Task.Delay(3000);
            return $"qwerty {Guid.NewGuid()}";
        }, fresh =>
        {
            OutputInformation($"Fresh data arrived: {fresh}");
            return Task.CompletedTask;
        });
        OutputInformation($"{item.Data} (Fresh: {item.Fresh})");
    }
}