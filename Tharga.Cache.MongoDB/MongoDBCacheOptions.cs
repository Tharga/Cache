namespace Tharga.Cache.MongoDB;

public record MongoDBCacheOptions
{
    public string CollectionName { get; set; } = "Cache";
    public string ConfigurationName { get; set; }
}