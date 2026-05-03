using Tharga.Mcp;

namespace Tharga.Cache.Mcp;

/// <summary>
/// Extension methods for <see cref="IThargaMcpBuilder"/> that register Tharga.Cache MCP providers.
/// </summary>
public static class ThargaMcpBuilderExtensions
{
    /// <summary>
    /// Registers <see cref="CacheResourceProvider"/> and <see cref="CacheToolProvider"/>, exposing
    /// cache types, items, persistence health, fetch queue depth, and clear actions on the System scope.
    /// </summary>
    public static IThargaMcpBuilder AddCache(this IThargaMcpBuilder builder)
    {
        builder.AddResourceProvider<CacheResourceProvider>();
        builder.AddToolProvider<CacheToolProvider>();
        return builder;
    }
}
