namespace Tharga.Cache;

public record Key(string Value)
{
    public static implicit operator string(Key key) => key.Value;
    public static implicit operator Key(string key) => new(key);
}