namespace Tharga.Cache.Persist;

internal interface IRedis : IPersist, IAsyncDisposable, IDisposable;