using System.Text.Json.Serialization;

namespace Dica43_Polly.Models;

/// <summary>
/// Modelo para post da API externa
/// </summary>
public class Post
{
    [JsonPropertyName("userId")]
    public int UserId { get; set; }
    
    [JsonPropertyName("id")]
    public int Id { get; set; }
    
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;
    
    [JsonPropertyName("body")]
    public string Body { get; set; } = string.Empty;
}

/// <summary>
/// Modelo para comentário da API externa
/// </summary>
public class Comment
{
    [JsonPropertyName("postId")]
    public int PostId { get; set; }
    
    [JsonPropertyName("id")]
    public int Id { get; set; }
    
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;
    
    [JsonPropertyName("body")]
    public string Body { get; set; } = string.Empty;
}

/// <summary>
/// Modelo para resposta de pagamento
/// </summary>
public class PaymentResponse
{
    public bool Success { get; set; }
    public string TransactionId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime ProcessedAt { get; set; }
}

/// <summary>
/// Modelo para resultado de operação resiliente
/// </summary>
public class OperationResult<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? ErrorMessage { get; set; }
    public int AttemptCount { get; set; }
    public TimeSpan Duration { get; set; }
    public string PolicyApplied { get; set; } = string.Empty;
    
    public static OperationResult<T> SuccessResult(T data, int attemptCount, TimeSpan duration, string policy)
    {
        return new OperationResult<T>
        {
            Success = true,
            Data = data,
            AttemptCount = attemptCount,
            Duration = duration,
            PolicyApplied = policy
        };
    }
    
    public static OperationResult<T> FailureResult(string error, int attemptCount, TimeSpan duration, string policy)
    {
        return new OperationResult<T>
        {
            Success = false,
            ErrorMessage = error,
            AttemptCount = attemptCount,
            Duration = duration,
            PolicyApplied = policy
        };
    }
}
