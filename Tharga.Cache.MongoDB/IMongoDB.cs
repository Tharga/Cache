namespace Tharga.Cache.MongoDB;

public interface IMongoDB : IPersist, IAsyncDisposable, IDisposable
{
    Task<(bool Success, string Message)> CanConnectAsync();
}