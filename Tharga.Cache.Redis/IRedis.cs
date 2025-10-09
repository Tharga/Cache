namespace Tharga.Cache.Redis;

public interface IRedis : IPersist, IAsyncDisposable, IDisposable
{
    Task<(bool Success, string Message)> CanConnectAsync();
}