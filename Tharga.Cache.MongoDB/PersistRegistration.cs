using Microsoft.Extensions.DependencyInjection;
using Tharga.MongoDB.Atlas;

namespace Tharga.Cache.MongoDB;

internal class PersistRegistration : IPersistRegistration
{
    public void Register(IServiceCollection services)
    {
        if (services.All(sd => sd.ServiceType != typeof(IExternalIpAddressService)))
        {
            //NOTE: MongoDB is not registered or is registered after the cache.
            //throw new InvalidOperationException($"Call {nameof(MongoDbRegistrationExtensions.AddMongoDB)} before {nameof(Register)} cache.");
        }
        else
        {
            //services.RegisterMongoDBCollection<ICacheRepositoryCollection, CacheRepositoryCollection>();
        }
    }
}