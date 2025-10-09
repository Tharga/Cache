using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Tharga.Cache.Redis;

public static class CacheRegistrationExtensions
{
    public static void AddRedisDBOptions(this CacheOptions cacheOptions, Action<RedisCacheOptions> options = null)
    {
        var o = new RedisCacheOptions();
        options?.Invoke(o);

        cacheOptions.RegistrationCallback(s => s.AddSingleton(Options.Create(o)));
    }
}