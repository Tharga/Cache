using Tharga.MongoDB;

namespace Tharga.Cache.MongoDB;

internal interface ICacheRepositoryCollection : IDiskRepositoryCollection<CacheEntity, string>;