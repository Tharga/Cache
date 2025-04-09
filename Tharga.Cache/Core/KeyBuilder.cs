namespace Tharga.Cache.Core;

internal static class KeyBuilder
{
    public static string BuildKey<T>(string key)
    {
        var typeName = typeof(T).Name;
        if (key.StartsWith($"{typeName}.")) return key;
        return $"{typeName}.{key}";
    }
}