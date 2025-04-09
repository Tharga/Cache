namespace Tharga.Cache.Core;

public interface IPersistLoader
{
    IPersist GetPersist(PersistType persistType);
}