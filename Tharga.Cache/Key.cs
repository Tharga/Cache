using System.Reflection.Metadata;

namespace Tharga.Cache;

public record Key
{
    private readonly string _value;
    private readonly string _typeName;

    internal Key(string value)
    {
        _value = value;
        _typeName = null;
    }

    //internal Key(string value, string typeName)
    //{
    //    _value = value;
    //    _typeName = typeName;
    //}

    public string Value => _typeName == null ? _value : $"{_typeName}.{_value}";

    public static implicit operator string(Key key) => key.Value;
    public static implicit operator Key(string key) => new(key);

    public override string ToString() => Value;
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