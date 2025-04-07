using System.Text.Json;

namespace Tharga.Cache.Core;

internal static class SizeHelper
{
    public static int ToSize(this object data)
    {
        var bytes = JsonSerializer.SerializeToUtf8Bytes(data);
        var size = bytes.Length;
        return size;
    }
}