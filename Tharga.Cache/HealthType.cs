namespace Tharga.Cache;

public record HealthType
{
    public required string Type { get; init; }
    public required Func<Task<HealthDto>> GetHealthAsync;
}