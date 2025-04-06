namespace Tharga.Cache;

public enum ExpirationPolicies
{
    /// <summary>
    /// TTL (Time To Live)
    /// Data expires a fixed time after insertion. Most common.
    /// </summary>
    TimeToLive,

    /// <summary>
    /// TTI (Time To Idle)
    /// Each time a cached item is accessed, its expiration clock is reset.
    /// </summary>
    TimeToIdle,

    /// <summary>
    /// Data expires at a fixed point in time regardless of access.
    /// </summary>
    Absolute,

    /// <summary>
    /// Data never expires unless explicitly removed.
    /// </summary>
    Eternal,
}