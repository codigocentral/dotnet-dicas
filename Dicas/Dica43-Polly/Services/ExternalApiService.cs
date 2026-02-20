using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using Polly.Timeout;
using System.Text.Json;
using Dica43_Polly.Configuration;
using Dica43_Polly.Models;

namespace Dica43_Polly.Services;

/// <summary>
/// Serviço que demonstra políticas de resiliência do Polly
/// </summary>
public class ExternalApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ExternalApiService> _logger;
    private readonly ExternalApiSettings _settings;
    
    // Políticas de resiliência
    private readonly IAsyncPolicy<HttpResponseMessage> _retryPolicy;
    private readonly IAsyncPolicy<HttpResponseMessage> _circuitBreakerPolicy;
    private readonly IAsyncPolicy<HttpResponseMessage> _timeoutPolicy;
    private readonly IAsyncPolicy<HttpResponseMessage> _combinedPolicy;
    private readonly IAsyncPolicy<HttpResponseMessage> _fallbackPolicy;

    public ExternalApiService(
        IHttpClientFactory httpClientFactory,
        ILogger<ExternalApiService> logger,
        IOptions<ExternalApiSettings> settings,
        IOptions<CircuitBreakerSettings> circuitBreakerSettings,
        IOptions<TimeoutSettings> timeoutSettings)
    {
        _httpClient = httpClientFactory.CreateClient("ExternalApi");
        _logger = logger;
        _settings = settings.Value;
        
        // 🔄 Política de Retry com Exponential Backoff
        _retryPolicy = Policy
            .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
            .Or<HttpRequestException>()
            .Or<TaskCanceledException>()
            .WaitAndRetryAsync(
                retryCount: _settings.MaxRetries,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (outcome, timespan, retryCount, context) =>
                {
                    _logger.LogWarning("🔄 Tentativa {RetryCount} falhada. Tentando novamente em {Delay}s. Erro: {Error}",
                        retryCount, timespan.TotalSeconds,
                        outcome.Exception?.Message ?? outcome.Result?.StatusCode.ToString());
                });

        // ⚡ Política de Circuit Breaker
        _circuitBreakerPolicy = Policy
            .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
            .Or<HttpRequestException>()
            .Or<TaskCanceledException>()
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: circuitBreakerSettings.Value.HandledEventsAllowedBeforeBreaking,
                durationOfBreak: circuitBreakerSettings.Value.DurationOfBreak,
                onBreak: (exception, timespan) =>
                {
                    _logger.LogError("⚡ Circuit Breaker ABERTO por {Duration}s. Erro: {Error}",
                        timespan.TotalSeconds, exception.Exception?.Message ?? "Response não sucessful");
                },
                onReset: () =>
                {
                    _logger.LogInformation("⚡ Circuit Breaker FECHADO - voltando ao normal");
                },
                onHalfOpen: () =>
                {
                    _logger.LogInformation("⚡ Circuit Breaker MEIO-ABERTO - testando próxima requisição");
                });

        // ⏰ Política de Timeout
        _timeoutPolicy = Policy.TimeoutAsync<HttpResponseMessage>(
            timeout: timeoutSettings.Value.DefaultTimeout,
            timeoutStrategy: TimeoutStrategy.Pessimistic,
            onTimeoutAsync: (context, timespan, task) =>
            {
                _logger.LogWarning("⏰ Timeout de {Timeout}s atingido", timespan.TotalSeconds);
                return Task.CompletedTask;
            });

        // 🛡️ Política Combinada (Timeout + Circuit Breaker + Retry)
        _combinedPolicy = Policy.WrapAsync(_retryPolicy, _circuitBreakerPolicy, _timeoutPolicy);

        // 🔄 Política de Fallback
        _fallbackPolicy = Policy
            .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
            .Or<HttpRequestException>()
            .Or<TaskCanceledException>()
            .Or<BrokenCircuitException>()
            .Or<TimeoutRejectedException>()
            .FallbackAsync(
                fallbackAction: async (context, cancellationToken) =>
                {
                    _logger.LogWarning("🔄 Executando fallback - retornando resposta padrão");
                    
                    var fallbackContent = JsonSerializer.Serialize(new Post
                    {
                        Id = -1,
                        UserId = -1,
                        Title = "Dados indisponíveis",
                        Body = "Serviço temporariamente indisponível. Tente novamente mais tarde."
                    });
                    
                    var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                    {
                        Content = new StringContent(fallbackContent, System.Text.Encoding.UTF8, "application/json")
                    };
                    
                    return response;
                },
                onFallbackAsync: (exception, context) =>
                {
                    _logger.LogWarning("🔄 Fallback ativado devido a: {Error}",
                        exception.Exception?.Message ?? "Response não sucessful");
                    return Task.CompletedTask;
                });
    }

    /// <summary>
    /// Demonstra uso básico de retry policy
    /// </summary>
    public async Task<OperationResult<Post?>> GetPostWithRetryAsync(int postId)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var attemptCount = 0;
        
        try
        {
            _logger.LogInformation("🔄 Buscando post {PostId} com política de retry", postId);
            
            var response = await _retryPolicy.ExecuteAsync(async () =>
            {
                attemptCount++;
                _logger.LogDebug("Tentativa {Attempt} para buscar post {PostId}", attemptCount, postId);
                return await _httpClient.GetAsync($"/posts/{postId}");
            });

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var post = JsonSerializer.Deserialize<Post>(content);
                
                stopwatch.Stop();
                _logger.LogInformation("✅ Post {PostId} obtido com sucesso em {Attempts} tentativas", 
                    postId, attemptCount);
                
                return OperationResult<Post?>.SuccessResult(post, attemptCount, stopwatch.Elapsed, "Retry Policy");
            }
            
            stopwatch.Stop();
            return OperationResult<Post?>.FailureResult(
                $"Falha HTTP: {response.StatusCode}", attemptCount, stopwatch.Elapsed, "Retry Policy");
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "❌ Erro ao buscar post {PostId} após {Attempts} tentativas", postId, attemptCount);
            return OperationResult<Post?>.FailureResult(ex.Message, attemptCount, stopwatch.Elapsed, "Retry Policy");
        }
    }

    /// <summary>
    /// Demonstra uso de circuit breaker
    /// </summary>
    public async Task<OperationResult<List<Post>>> GetPostsWithCircuitBreakerAsync()
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var attemptCount = 1;
        
        try
        {
            _logger.LogInformation("⚡ Buscando posts com Circuit Breaker");
            
            var response = await _circuitBreakerPolicy.ExecuteAsync(async () =>
            {
                _logger.LogDebug("Executando requisição para /posts");
                return await _httpClient.GetAsync("/posts?_limit=5");
            });

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var posts = JsonSerializer.Deserialize<List<Post>>(content) ?? new List<Post>();
                
                stopwatch.Stop();
                _logger.LogInformation("✅ {Count} posts obtidos com Circuit Breaker", posts.Count);
                
                return OperationResult<List<Post>>.SuccessResult(posts, attemptCount, stopwatch.Elapsed, "Circuit Breaker");
            }
            
            stopwatch.Stop();
            return OperationResult<List<Post>>.FailureResult(
                $"Falha HTTP: {response.StatusCode}", attemptCount, stopwatch.Elapsed, "Circuit Breaker");
        }
        catch (BrokenCircuitException ex)
        {
            stopwatch.Stop();
            _logger.LogWarning("⚡ Circuit Breaker está ABERTO - requisição rejeitada");
            return OperationResult<List<Post>>.FailureResult(
                "Circuit Breaker está aberto", attemptCount, stopwatch.Elapsed, "Circuit Breaker");
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "❌ Erro ao buscar posts com Circuit Breaker");
            return OperationResult<List<Post>>.FailureResult(ex.Message, attemptCount, stopwatch.Elapsed, "Circuit Breaker");
        }
    }

    /// <summary>
    /// Demonstra uso de política combinada (Timeout + Circuit Breaker + Retry)
    /// </summary>
    public async Task<OperationResult<Comment?>> GetCommentWithCombinedPolicyAsync(int commentId)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var attemptCount = 0;
        
        try
        {
            _logger.LogInformation("🛡️ Buscando comentário {CommentId} com política combinada", commentId);
            
            var response = await _combinedPolicy.ExecuteAsync(async () =>
            {
                attemptCount++;
                _logger.LogDebug("Tentativa {Attempt} para buscar comentário {CommentId}", attemptCount, commentId);
                return await _httpClient.GetAsync($"/comments/{commentId}");
            });

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var comment = JsonSerializer.Deserialize<Comment>(content);
                
                stopwatch.Stop();
                _logger.LogInformation("✅ Comentário {CommentId} obtido com política combinada em {Attempts} tentativas", 
                    commentId, attemptCount);
                
                return OperationResult<Comment?>.SuccessResult(comment, attemptCount, stopwatch.Elapsed, "Combined Policy");
            }
            
            stopwatch.Stop();
            return OperationResult<Comment?>.FailureResult(
                $"Falha HTTP: {response.StatusCode}", attemptCount, stopwatch.Elapsed, "Combined Policy");
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "❌ Erro ao buscar comentário {CommentId} com política combinada", commentId);
            return OperationResult<Comment?>.FailureResult(ex.Message, attemptCount, stopwatch.Elapsed, "Combined Policy");
        }
    }

    /// <summary>
    /// Demonstra uso de fallback policy
    /// </summary>
    public async Task<OperationResult<Post?>> GetPostWithFallbackAsync(int postId)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var attemptCount = 1;
        
        try
        {
            _logger.LogInformation("🔄 Buscando post {PostId} com fallback", postId);
            
            var response = await _fallbackPolicy.ExecuteAsync(async () =>
            {
                _logger.LogDebug("Tentativa de buscar post {PostId}", postId);
                // Simula erro forçando status 500
                return await _httpClient.GetAsync($"/500");
            });

            var content = await response.Content.ReadAsStringAsync();
            var post = JsonSerializer.Deserialize<Post>(content);
            
            stopwatch.Stop();
            _logger.LogInformation("✅ Resposta obtida com fallback para post {PostId}", postId);
            
            return OperationResult<Post?>.SuccessResult(post, attemptCount, stopwatch.Elapsed, "Fallback Policy");
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "❌ Erro ao buscar post {PostId} mesmo com fallback", postId);
            return OperationResult<Post?>.FailureResult(ex.Message, attemptCount, stopwatch.Elapsed, "Fallback Policy");
        }
    }

    /// <summary>
    /// Demonstra simulação de falhas para testar circuit breaker
    /// </summary>
    public async Task<List<OperationResult<string>>> SimulateFailuresForCircuitBreakerAsync()
    {
        var results = new List<OperationResult<string>>();
        
        _logger.LogInformation("🧪 Simulando falhas para testar Circuit Breaker");
        
        for (int i = 1; i <= 6; i++)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            try
            {
                _logger.LogInformation("Tentativa {Attempt} - forçando erro 500", i);
                
                var response = await _circuitBreakerPolicy.ExecuteAsync(async () =>
                {
                    // Força erro 500 para disparar circuit breaker
                    return await _httpClient.GetAsync("/500");
                });
                
                stopwatch.Stop();
                results.Add(OperationResult<string>.SuccessResult(
                    $"Tentativa {i}: Sucesso inesperado", 1, stopwatch.Elapsed, "Circuit Breaker"));
            }
            catch (BrokenCircuitException)
            {
                stopwatch.Stop();
                _logger.LogWarning("⚡ Tentativa {Attempt}: Circuit Breaker ABERTO", i);
                results.Add(OperationResult<string>.FailureResult(
                    $"Tentativa {i}: Circuit Breaker aberto", 1, stopwatch.Elapsed, "Circuit Breaker"));
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogWarning("❌ Tentativa {Attempt}: {Error}", i, ex.Message);
                results.Add(OperationResult<string>.FailureResult(
                    $"Tentativa {i}: {ex.Message}", 1, stopwatch.Elapsed, "Circuit Breaker"));
            }
            
            // Pequena pausa entre tentativas
            await Task.Delay(100);
        }
        
        return results;
    }
}
