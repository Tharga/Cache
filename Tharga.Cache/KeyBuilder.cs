using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;

namespace Tharga.Cache;

public static class KeyBuilder
{
    public static string SetTypeKey<T>(string key)
    {
        var typeName = typeof(T).Name;
        if (key.StartsWith($"{typeName}.")) return key;
        return $"{typeName}.{key}";
    }

    //internal static Key SetTypeKey<T>(this Key key)
    //{
    //    //var typeName = typeof(T).Name;
    //    //if (key.Value.StartsWith($"{typeName}.")) return key;
    //    //return new Key(key, typeName, key.KeyParts);
    //    throw new NotImplementedException();
    //}

    public static KeyDefinition Add(string name, string value)
    {
        var definition = new KeyDefinition { Keys = new ConcurrentDictionary<string, string>() };
        return Add(definition, name, value);
    }

    public static KeyDefinition Add(this KeyDefinition definition, string name, string value)
    {
        if (!definition.Keys.TryAdd(name, value)) throw new InvalidOperationException($"Cannot add key '{name}' with value '{value}'.");
        return definition;
    }

    internal static string ToHash(this KeyDefinition definition)
    {
        definition = new KeyDefinition { Keys = new ConcurrentDictionary<string, string>(definition.Keys.OrderBy(x => x.Key)) };
        var rawString = System.Text.Json.JsonSerializer.Serialize(definition);

        using var md5 = MD5.Create();
        var bytes = Encoding.UTF8.GetBytes(rawString);
        var hashBytes = md5.ComputeHash(bytes);

        return Convert.ToBase64String(hashBytes);
    }
}