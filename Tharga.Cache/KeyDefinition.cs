using System.Collections.Concurrent;

namespace Tharga.Cache;

public record KeyDefinition
{
    public required ConcurrentDictionary<string,string> Keys { get; init; }

    //public static implicit operator string(KeyDefinition key) => (Key)key;
}