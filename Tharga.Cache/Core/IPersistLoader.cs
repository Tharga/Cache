namespace Tharga.Cache.Core;

internal interface IPersistLoader
{
    IPersist GetPersist(Type persistType);
}