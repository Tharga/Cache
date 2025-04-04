namespace Tharga.Cache;

public enum EvictionPolicy
{
    ///// <summary>
    ///// LRU (Least Recently Used)
    ///// + Simple, effective.
    ///// - Can evict frequently used cold items.
    ///// </summary>
    //LeastRecentlyUsed,

    ///// <summary>
    ///// LFU (Least Frequently Used)
    ///// + Great for hot data.
    ///// - Harder to implement efficiently.
    ///// </summary>
    //LeastFrequentlyUsed,

    ///// <summary>
    ///// FIFO (First-In, First-Out)
    ///// + Predictable, simple.
    ///// - Ignores usage patterns.
    ///// </summary>
    //FirstInFirstOut,

    /// <summary>
    /// RR (Random Replacement)
    /// + Ultra-low overhead.
    /// - Unpredictable, lower hit rate.
    /// </summary>
    RandomReplacement,

    ///// <summary>
    ///// MRU (Most Recently Used)
    ///// + Niche use cases.
    ///// - Often counterproductive.
    ///// </summary>
    //MostRecentlyUsed,

    ///// <summary>
    ///// 2Q (Two Queue Algorithm)
    ///// + Reduces noise/pollution.
    ///// - More complex to implement.
    ///// </summary>
    //TwoQueue,

    ///// <summary>
    ///// ARC (Adaptive Replacement Cache)
    ///// + Auto-tunes based on behavior.
    ///// - Higher complexity, more metadata.
    ///// </summary>
    //AdaptiveReplacement,
}