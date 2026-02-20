using System.ComponentModel.DataAnnotations;

namespace Dica43_Polly.Configuration;

/// <summary>
/// Configurações para APIs externas
/// </summary>
public class ExternalApiSettings
{
    public const string SectionName = "ExternalApi";
    
    [Required]
    [Url]
    public string BaseUrl { get; set; } = string.Empty;
    
    [Required]
    public TimeSpan Timeout { get; set; }
    
    [Range(1, 10)]
    public int MaxRetries { get; set; } = 3;
}

/// <summary>
/// Configurações para API de pagamento
/// </summary>
public class PaymentApiSettings
{
    public const string SectionName = "PaymentApi";
    
    [Required]
    [Url]
    public string BaseUrl { get; set; } = string.Empty;
    
    [Required]
    public TimeSpan Timeout { get; set; }
    
    [Range(1, 5)]
    public int MaxRetries { get; set; } = 2;
}

/// <summary>
/// Configurações do Circuit Breaker
/// </summary>
public class CircuitBreakerSettings
{
    public const string SectionName = "CircuitBreaker";
    
    [Range(1, 100)]
    public int HandledEventsAllowedBeforeBreaking { get; set; } = 3;
    
    [Required]
    public TimeSpan DurationOfBreak { get; set; }
    
    [Required]
    public TimeSpan SamplingDuration { get; set; }
    
    [Range(1, 1000)]
    public int MinimumThroughput { get; set; } = 5;
    
    [Range(0.1, 1.0)]
    public double FailureThreshold { get; set; } = 0.5;
}

/// <summary>
/// Configurações do Bulkhead
/// </summary>
public class BulkheadSettings
{
    public const string SectionName = "Bulkhead";
    
    [Range(1, 100)]
    public int MaxParallelization { get; set; } = 10;
    
    [Range(0, 1000)]
    public int MaxQueuingActions { get; set; } = 20;
}

/// <summary>
/// Configurações de Timeout
/// </summary>
public class TimeoutSettings
{
    public const string SectionName = "Timeout";
    
    [Required]
    public TimeSpan DefaultTimeout { get; set; }
    
    [Required]
    public TimeSpan LongRunningTimeout { get; set; }
}
