using Tharga.Console.Commands.Base;

namespace Tharga.Cache.Console.Commands;

internal class CacheGetWithCallbackCommand : ActionCommandBase
{
    private readonly ITimeToLiveCache _timeToLiveCache;

    public CacheGetWithCallbackCommand(ITimeToLiveCache timeToLiveCache)
        : base("callback")
    {
        _timeToLiveCache = timeToLiveCache;
    }

    public override void Invoke(string[] param)
    {
        Task.Run(async () =>
        {
            //var key = Guid.NewGuid().ToString();
            var key = "key";
            OutputInformation($"Calling data with key {key}.");
            var item = await _timeToLiveCache.GetWithCallbackAsync(key, async () =>
            {
                await Task.Delay(3000);
                return $"qwerty {Guid.NewGuid()}";
            }, fresh =>
            {
                OutputInformation($"Fresh data arrived: {fresh} [key: {key}]");
                return Task.CompletedTask;
            });
            OutputInformation($"{item.Data} [Fresh: {item.Fresh}, key: {key}]");
        });
    }
}