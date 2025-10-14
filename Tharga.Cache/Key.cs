using System.Net.Mime;

namespace Tharga.Cache;

public record Key
{
    private readonly string _value;

    private Key(string value)
    {
        _value = value;
        KeyParts = [];
    }

    private Key(KeyDefinition definition)
    {
        _value = definition.ToHash();
        KeyParts = definition.Keys.ToDictionary();
    }

    internal Key(string key, string typeName, Dictionary<string, string> parts)
    {
        _value = key;
        TypeName = typeName;
        KeyParts = parts;
    }

    public string Value => TypeName == null ? _value : $"{TypeName}.{_value}";
    public string TypeName { get; }
    public Dictionary<string, string> KeyParts { get; }

    public static implicit operator string(Key key) => key.Value;
    public static implicit operator Key(string key) => new(key);
    public static implicit operator Key(KeyDefinition definition) => new(definition);
}