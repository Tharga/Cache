namespace Tharga.Cache.Core;

internal class PersistLoader : IPersistLoader
{
    private readonly IServiceProvider _serviceProvider;

    public PersistLoader(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IPersist GetPersist(Type persistType)
    {
        var t = _serviceProvider.GetService(persistType);
        if (t == null) throw new InvalidOperationException($"Cannot create type '{persistType.Name}'.");
        var persist = t as IPersist;
        if (persist == null) throw new InvalidOperationException($"Type {persistType.Name} does not implement the '{nameof(IPersist)}' interface.");
        return persist;
    }
}