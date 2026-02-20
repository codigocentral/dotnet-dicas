namespace Dica66_RateLimiting.Models;

public record ApiResponse<T>
{
    public bool Success { get; init; }
    public T? Data { get; init; }
    public string? Message { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public RateLimitInfo? RateLimit { get; init; }
}

public record RateLimitInfo
{
    public int? Limit { get; init; }
    public int? Remaining { get; init; }
    public DateTime? ResetTime { get; init; }
    public string? Policy { get; init; }
}

public record WeatherForecast
{
    public DateOnly Date { get; init; }
    public int TemperatureC { get; init; }
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
    public string? Summary { get; init; }
}

public record UserInfo
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public UserTier Tier { get; init; }
}

public enum UserTier
{
    Free,
    Premium,
    Enterprise
}
