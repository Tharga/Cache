using System.Collections.Concurrent;
using System.Text;

namespace Tharga.Cache;

public static class KeyBuilder
{
    public static string BuildKey<T>(string key)
    {
        var typeName = typeof(T).Name;
        if (key.StartsWith($"{typeName}.")) return key;
        return $"{typeName}.{key}";
    }

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

    //TODO: Build tests for this
    public static Key Build(this KeyDefinition definition)
    {
        definition = new KeyDefinition { Keys = new ConcurrentDictionary<string, string>(definition.Keys.OrderBy(x => x.Key)) };
        var raw = System.Text.Json.JsonSerializer.Serialize(definition);
        var base64Encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(raw));
        return new Key(base64Encoded);
    }

    //TODO: Build tests for this
    public static KeyDefinition ToKeyDefinition(this Key key)
    {
        var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(key.Value));
        var definition = System.Text.Json.JsonSerializer.Deserialize<KeyDefinition>(decoded);
        return definition;
    }
}