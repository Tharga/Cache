namespace Tharga.Cache.Core;

public interface IWatchDogService
{
    void Start(CancellationToken cancellationToken = default);
}