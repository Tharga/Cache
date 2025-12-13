using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Tharga.Cache.File;

public static class CacheRegistrationExtensions
{
    public static void AddFileDBOptions(this CacheOptions cacheOptions, Action<FileCacheOptions> options = null)
    {
        var o = new FileCacheOptions();
        options?.Invoke(o);

        cacheOptions.RegistrationCallback(s => s.AddSingleton(Options.Create(o)));
    }
}