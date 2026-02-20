namespace Dica70_BackgroundServices.Models;

public record JobInfo
{
    public string Id { get; init; } = Guid.NewGuid().ToString();
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public JobStatus Status { get; init; } = JobStatus.Pending;
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime? StartedAt { get; init; }
    public DateTime? CompletedAt { get; init; }
    public TimeSpan? Duration => CompletedAt - StartedAt;
    public string? Error { get; init; }
    public Dictionary<string, object> Metadata { get; init; } = new();
}

public enum JobStatus
{
    Pending,
    Running,
    Completed,
    Failed,
    Cancelled
}

public record QueueItem<T>
{
    public string Id { get; init; } = Guid.NewGuid().ToString();
    public T Data { get; init; } = default!;
    public int Priority { get; init; } = 0;
    public int RetryCount { get; init; } = 0;
    public int MaxRetries { get; init; } = 3;
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime? ProcessedAt { get; init; }
    public string? Error { get; init; }
}

public record EmailNotification
{
    public string To { get; init; } = string.Empty;
    public string Subject { get; init; } = string.Empty;
    public string Body { get; init; } = string.Empty;
    public EmailPriority Priority { get; init; } = EmailPriority.Normal;
}

public enum EmailPriority
{
    Low = 0,
    Normal = 1,
    High = 2,
    Critical = 3
}

public record DataProcessingJob
{
    public string Source { get; init; } = string.Empty;
    public string Destination { get; init; } = string.Empty;
    public string ProcessingType { get; init; } = string.Empty;
    public int RecordCount { get; init; }
    public Dictionary<string, object> Parameters { get; init; } = new();
}

public record HealthCheckResult
{
    public string ServiceName { get; init; } = string.Empty;
    public bool IsHealthy { get; init; }
    public DateTime CheckedAt { get; init; } = DateTime.UtcNow;
    public TimeSpan ResponseTime { get; init; }
    public string? Details { get; init; }
}

public record ServiceStatus
{
    public string ServiceName { get; init; } = string.Empty;
    public bool IsRunning { get; init; }
    public DateTime LastActivity { get; init; }
    public long ProcessedItems { get; init; }
    public long ErrorCount { get; init; }
    public TimeSpan Uptime { get; init; }
    public Dictionary<string, object> Metrics { get; init; } = new();
}

public record ApiResponse<T>
{
    public bool Success { get; init; }
    public T? Data { get; init; }
    public string? Message { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public Dictionary<string, object>? Metadata { get; init; }
}
