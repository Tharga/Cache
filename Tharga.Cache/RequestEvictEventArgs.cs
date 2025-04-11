namespace Tharga.Cache;

public class RequestEvictEventArgs : EventArgs
{
    public RequestEvictEventArgs(Type type, Key key)
    {
        Type = type;
        Key = key;
    }

    public Type Type { get; }
    public Key Key { get; }
}