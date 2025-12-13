using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Tharga.Cache.File;

internal class PersistRegistration : IPersistRegistration
{
    public void Register(IServiceCollection services)
    {
        services.AddSingleton(Options.Create(new FileCacheOptions()));
        services.AddTransient<IFileService, FileService>();
        services.AddTransient<IFileFormatService, FileFormatService>();
    }
}