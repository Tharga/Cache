using Microsoft.Extensions.DependencyInjection;
using Tharga.Cache.Persist;

namespace Tharga.Cache.Core;

internal class PersistLoader : IPersistLoader
{
    private readonly IServiceProvider _serviceProvider;

    public PersistLoader(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IPersist GetPersist(CacheTypeOptions typeOptions)
    {
        switch (typeOptions.PersistType)
        {
            case PersistType.Memory:
                return _serviceProvider.GetService<IMemory>();
            case PersistType.Redis:
                return _serviceProvider.GetService<IRedis>();
            default:
                throw new ArgumentOutOfRangeException(nameof(PersistType), $"Unknown {nameof(PersistLoader)} {typeOptions.PersistType}.");
        }
    }
}