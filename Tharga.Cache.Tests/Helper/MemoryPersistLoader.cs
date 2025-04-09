using Tharga.Cache.Core;
using Tharga.Cache.Persist;

namespace Tharga.Cache.Tests.Helper;

internal class MemoryPersistLoader : IPersistLoader
{
    private Memory _memory;

    public IPersist GetPersist(TypeOptions options)
    {
        return _memory ??= new Memory();
    }
}