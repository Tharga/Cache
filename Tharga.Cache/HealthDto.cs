namespace Tharga.Cache;

public record HealthDto
{
    public required bool Success { get; init; }
    public required string Message { get; init; }
}