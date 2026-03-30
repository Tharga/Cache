namespace Tharga.Cache.Redis;

/// <summary>
/// Should only be used when running a single instance, since data is stored in memory.
/// </summary>
[Obsolete("IMemoryWithRedis couples two storage concerns. Use IRedis or IMemory explicitly instead.")]
public interface IMemoryWithRedis : IPersist;