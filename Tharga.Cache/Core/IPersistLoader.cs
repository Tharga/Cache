namespace Tharga.Cache.Core;

public interface IPersistLoader
{
    IPersist GetPersist(TypeOptions typeOptions);
}