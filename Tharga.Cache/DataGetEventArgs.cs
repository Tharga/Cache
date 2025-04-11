namespace Tharga.Cache;

public class RequestEvictEventArgs : EventArgs
{

}

public class DataGetEventArgs : EventArgs
{
    public DataGetEventArgs(Key key)
    {
        Key = key;
    }

    public Key Key { get; }
}