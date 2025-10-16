namespace Tharga.Cache.Web.Controllers;

public record MonitorData
{
    //public required string[] Keys { get; init; }
    public required CacheInfo[] Infos { get; init; }
}

public record CacheInfo
{
    public required string Key { get; set; }
    public int AccessCount { get; set; }
}