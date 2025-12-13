namespace Tharga.Cache.File;

public record FileCacheOptions
{
    public string CompanyName { get; set; }
    public string AppName { get; set; }
    public Format Format { get; set; } = Format.Json;
}