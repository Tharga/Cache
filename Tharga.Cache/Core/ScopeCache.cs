using Microsoft.Extensions.Logging;
using Tharga.Cache.Persist;

namespace Tharga.Cache.Core;

/// <summary>
/// The cache behind <see cref="IScopeCache"/>. Each instance owns its own in-memory store, monitor and fetch queue,
/// so a value is only seen within the DI scope that cached it and is removed when the scope is disposed.
/// Data never expires within the scope.
/// </summary>
internal sealed class ScopeCache : CacheBase, IScopeCache, IDisposable
{
    private readonly Memory _store;

    public ScopeCache(CacheOptions options, ILoggerFactory loggerFactory = null)
        : this(new ScopeStore(options, loggerFactory), options, loggerFactory?.CreateLogger("Tharga.Cache.Cache"))
    {
    }

    private ScopeCache(ScopeStore scopeStore, CacheOptions options, ILogger logger)
        : base(scopeStore.Monitor, scopeStore, scopeStore.FetchQueue, options, logger)
    {
        _store = scopeStore.Store;
    }

    protected override TimeSpan GetDefaultFreshSpan<T>() => TimeSpan.MaxValue;

    public void Dispose()
    {
        _store.Clear();
    }

    /// <summary>
    /// Every type cached in a scope goes to the scope's own in-memory store, whatever persist the type is configured for.
    /// </summary>
    private sealed class ScopeStore : IPersistLoader
    {
        public ScopeStore(CacheOptions options, ILoggerFactory loggerFactory)
        {
            Monitor = new CacheMonitor(this, options);
            Store = new Memory(Monitor);
            FetchQueue = new FetchQueue(Monitor, options, loggerFactory?.CreateLogger<FetchQueue>());
        }

        public CacheMonitor Monitor { get; }
        public Memory Store { get; }
        public FetchQueue FetchQueue { get; }

        public IPersist GetPersist(Type persistType) => Store;
    }
}
