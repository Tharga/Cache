using Microsoft.Extensions.Logging;
using Tharga.MongoDB;
using Tharga.MongoDB.Disk;

namespace Tharga.Cache.MongoDB;

internal class CacheRepositoryCollection : DiskRepositoryCollectionBase<CacheEntity, string>, ICacheRepositoryCollection
{
    public CacheRepositoryCollection(IMongoDbServiceFactory mongoDbServiceFactory, ILogger<CacheRepositoryCollection> logger, DatabaseContext databaseContext)
        : base(mongoDbServiceFactory, logger, databaseContext)
    {
    }
}