namespace Tharga.Cache;

public record Key
{
    private readonly string _value;

    internal Key(string value)
    {
        _value = value;
        KeyParts = [];
    }

    internal Key(string value, Dictionary<string, string> keyParts)
    {
        _value = value;
        KeyParts = keyParts ?? [];
    }

    private Key(KeyDefinition definition)
    {
        _value = definition.ToHash();
        KeyParts = definition.Keys.ToDictionary();
    }

    public string Value => _value;

    public static implicit operator string(Key key) => key.Value;
    public static implicit operator Key(string key) => new(key);
    public static implicit operator Key(KeyDefinition definition) => new(definition);

    public override string ToString() => Value;
    public Dictionary<string, string> KeyParts { get; }

    public virtual bool Equals(Key other)
    {
        return string.Equals(Value, other?.Value, StringComparison.OrdinalIgnoreCase);
    }

    public override int GetHashCode()
    {
        return (_value != null ? _value.GetHashCode() : 0);
    }
}

//public record Key
//{
//    private readonly string _base;

//    private Key(string value)
//    {
//        _base = value;
//        KeyParts = [];
//    }

//    private Key(KeyDefinition definition)
//    {
//        _base = definition.ToHash();
//        KeyParts = definition.Keys.ToDictionary();
//    }

//    internal Key(string key, string typeName, Dictionary<string, string> parts)
//    {
//        _base = key;
//        TypeName = typeName;
//        KeyParts = parts;
//    }

//    public string Base => _base;
//    public string Value => TypeName == null ? Base : $"{TypeName}.{Base}";
//    public string TypeName { get; }
//    public Dictionary<string, string> KeyParts { get; }

//    public static implicit operator string(Key key) => key.Value;
//    public static implicit operator Key(string key) => new(key);
//    //public static implicit operator Key(KeyDefinition definition) => new(definition);
//}