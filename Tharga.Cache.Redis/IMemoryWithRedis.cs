namespace Tharga.Cache.Redis;

/// <summary>
/// Should only be used when running a single instance, since data is stored in memory.
/// </summary>
public interface IMemoryWithRedis : IPersist;