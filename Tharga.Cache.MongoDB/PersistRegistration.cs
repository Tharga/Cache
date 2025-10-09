using Microsoft.Extensions.DependencyInjection;
using Tharga.MongoDB;
using Tharga.MongoDB.Atlas;

namespace Tharga.Cache.MongoDB;

internal class PersistRegistration : IPersistRegistration
{
    public void Register(IServiceCollection services)
    {
        if (services.All(sd => sd.ServiceType != typeof(IExternalIpAddressService)))
        {
            services.AddMongoDB(o => { });
        }

        //services.AddTransient<ICacheRepository, CacheRepository>();
        //services.AddTransient<ICacheRepositoryCollection, CacheRepositoryCollection>();
    }
}