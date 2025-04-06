namespace Tharga.Cache;

public class DataDropEventArgs : EventArgs
{
    public DataDropEventArgs(Key key, object data)
    {
        Key = key;
        Data = data;
    }

    public Key Key { get; }
    public object Data { get; }
}