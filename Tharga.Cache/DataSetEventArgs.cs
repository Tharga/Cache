namespace Tharga.Cache;

public class DataSetEventArgs : EventArgs
{
    public DataSetEventArgs(Key key, object data)
    {
        Key = key;
        Data = data;
    }

    public Key Key { get; }
    public object Data { get; }
}