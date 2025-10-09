using Microsoft.Extensions.DependencyInjection;

namespace Tharga.Cache;

public interface ITimeToLiveCache : ITimeCache;

public interface IPersistRegistration
{
    void Register(IServiceCollection services);
    //void Register<T>(IServiceCollection services, Action<T> options = null) where T : CacheOptions;
}