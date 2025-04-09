namespace Tharga.Cache.Core;

public interface IPersistLoader
{
    IPersist GetPersist(CacheTypeOptions typeOptions);
}