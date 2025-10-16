namespace Tharga.Cache.Redis;

public record RedisCacheOptions
{
    public Func<IServiceProvider, string> ConnectionStringLoader;
}