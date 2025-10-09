using Microsoft.Extensions.DependencyInjection;
using Tharga.MongoDB;
using Tharga.MongoDB.Atlas;
using Tharga.MongoDB.Configuration;

namespace Tharga.Cache.MongoDB;

internal class PersistRegistration : IPersistRegistration
{
    public void Register(IServiceCollection services)
    {
        if (services.All(sd => sd.ServiceType != typeof(IExternalIpAddressService)))
        {
            //NOTE: MongoDB is not registered or is registered after the cache.
            services.AddMongoDB(o => { });
        }
        else
        {
            //NOTE: MongoDB is registered before the cache.
            //TODO: Here I just want to register the collection type, not run the entire registration, since that will mess upp other settings.
            //There should be a method that does only that...
            services.AddMongoDB(o =>
            {
                o.RegisterCollections = [new CollectionType<ICacheRepositoryCollection, CacheRepositoryCollection>()];
            });
        }
    }
}