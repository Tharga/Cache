namespace Tharga.Cache;

public class DataDropEventArgs : EventArgs
{
    public DataDropEventArgs(Key key)
    {
        Key = key;
    }

    public Key Key { get; }
}