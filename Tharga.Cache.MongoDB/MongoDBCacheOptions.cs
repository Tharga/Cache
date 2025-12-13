namespace Tharga.Cache.MongoDB;

public record MongoDBCacheOptions
{
    public string CollectionName { get; set; } = "_cache";
    public string ConfigurationName { get; set; }
}