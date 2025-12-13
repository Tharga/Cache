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
            throw new InvalidOperationException($"Call {nameof(MongoDbRegistrationExtensions.AddMongoDB)} before {nameof(Register)} cache.");
        }
        else
        {
            //var collection = new CollectionType<ICacheRepositoryCollection, CacheRepositoryCollection>();
            services.RegisterMongoDBCollection<ICacheRepositoryCollection, CacheRepositoryCollection>();
        }
    }
}