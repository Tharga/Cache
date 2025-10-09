using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Tharga.Cache.MongoDB;

public static class CacheRegistrationExtensions
{
    public static void AddMongoDBOptions(this CacheOptions cacheOptions, Action<MongoDBCacheOptions> options = null)
    {
        var o = new MongoDBCacheOptions();
        options?.Invoke(o);

        cacheOptions.RegistrationCallback(s => s.AddSingleton(Options.Create(o)));
    }
}