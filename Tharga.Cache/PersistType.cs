namespace Tharga.Cache;

public enum PersistType
{
    /// <summary>
    /// Stores data in local memory.
    /// + Fast.
    /// - Not distributed over different machines.
    /// </summary>
    Memory,

    /// <summary>
    /// Stores data in a redis cache server.
    /// + Can be shared between different machines.
    /// - Slow
    /// </summary>
    Redis,

    /// <summary>
    /// Stores data in local memory and in redis cache server.
    /// + Fast reads and distibuted over different machines.
    /// - Slower writes and possible out of sync issues over different machines.
    /// </summary>
    MemoryWithRedis,

    ///// <summary>
    ///// Writes to the cache, and immediately writes to the underlying database as well.
    ///// + Ensures cache is always up to date.
    ///// - Write latency increases because both cache and DB must be written to synchronously.
    ///// Good for read-heavy systems where strong consistency is important.
    ///// </summary>
    //WriteThrough,

    ///// <summary>
    ///// The application writes only to the cache. The cache writes to the database asynchronously, typically with a delay or batch.
    ///// + Faster writes, because you avoid immediate DB writes.
    ///// - Higher risk: if cache fails before persisting, data can be lost.
    ///// Useful for high-throughput systems with tolerant consistency needs.
    ///// </summary>
    //WriteBehind,

    ///// <summary>
    ///// Checking the cache first. If not found (a cache miss), loading from the database, and then populating the cache with that data.
    ///// Cache is populated on-demand (lazy).
    ///// You control the logic.
    ///// Simple and very common pattern.
    ///// Stale data possible, unless explicitly evicted or expired.
    ///// </summary>
    //CacheAside,

    ///// <summary>
    ///// Application reads from the cache. If there’s a cache miss, the cache itself fetches from the DB and populates itself.
    ///// You don’t write DB fetch logic yourself — the cache does it.
    ///// Slightly more complex infrastructure.
    ///// Good for centralized cache management.
    ///// </summary>
    //ReadThrough,
}