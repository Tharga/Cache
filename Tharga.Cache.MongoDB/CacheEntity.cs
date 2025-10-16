using MongoDB.Bson.Serialization.Attributes;
using Tharga.MongoDB;

namespace Tharga.Cache.MongoDB;

public record CacheEntity : EntityBase<string>
{
    public required string Type { get; init; }
    public required string Data { get; init; }
    public required DateTime CreateTime { get; init; }

    [BsonIgnoreIfDefault]
    public DateTime? UpdateTime { get; init; }

    [BsonIgnoreIfDefault]
    public TimeSpan? FreshSpan { get; init; }

    [BsonIgnoreIfDefault]
    public required bool StaleWhileRevalidate { get; init; }
}