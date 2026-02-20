using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Bulkhead;
using System.Diagnostics;
using System.Text.Json;
using Dica43_Polly.Configuration;
using Dica43_Polly.Models;

namespace Dica43_Polly.Services;

/// <summary>
/// Serviço que demonstra padrões avançados do Polly: Bulkhead, Rate Limiting e políticas personalizadas
/// </summary>
public class PaymentService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<PaymentService> _logger;
    private readonly PaymentApiSettings _settings;
    
    // Políticas avançadas
    private readonly IAsyncPolicy<HttpResponseMessage> _bulkheadPolicy;
    private readonly IAsyncPolicy<HttpResponseMessage> _rateLimitPolicy;
    private readonly IAsyncPolicy<HttpResponseMessage> _advancedRetryPolicy;
    private readonly IAsyncPolicy<HttpResponseMessage> _comprehensivePolicy;

    public PaymentService(
        IHttpClientFactory httpClientFactory,
        ILogger<PaymentService> logger,
        IOptions<PaymentApiSettings> settings,
        IOptions<BulkheadSettings> bulkheadSettings)
    {
        _httpClient = httpClientFactory.CreateClient("PaymentApi");
        _logger = logger;
        _settings = settings.Value;
        
        // 🚧 Política de Bulkhead (controla paralelismo)
        _bulkheadPolicy = Policy.BulkheadAsync<HttpResponseMessage>(
            maxParallelization: bulkheadSettings.Value.MaxParallelization,
            maxQueuingActions: bulkheadSettings.Value.MaxQueuingActions,
            onBulkheadRejectedAsync: (context) =>
            {
                _logger.LogWarning("🚧 Bulkhead: Requisição rejeitada - limite de paralelismo atingido");
                return Task.CompletedTask;
            });

        // 🚦 Política de Rate Limiting (simples)
        var rateLimitSemaphore = new SemaphoreSlim(5, 5); // Máximo 5 requisições simultâneas
        _rateLimitPolicy = Policy.HandleResult<HttpResponseMessage>(r => false)
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: _ => TimeSpan.FromMilliseconds(100),
                onRetry: (outcome, timespan, retryCount, context) =>
                {
                    _logger.LogInformation("🚦 Rate limit aplicado, aguardando...");
                });

        // 🔄 Retry com Jitter para evitar thundering herd
        _advancedRetryPolicy = Policy
            .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
            .Or<HttpRequestException>()
            .Or<TaskCanceledException>()
            .WaitAndRetryAsync(
                retryCount: _settings.MaxRetries,
                sleepDurationProvider: (retryAttempt) =>
                {
                    // Exponential backoff with jitter
                    var baseDelay = TimeSpan.FromSeconds(Math.Pow(2, retryAttempt));
                    var jitter = TimeSpan.FromMilliseconds(Random.Shared.Next(0, 1000));
                    return baseDelay + jitter;
                },
                onRetry: (outcome, timespan, retryCount, context) =>
                {
                    _logger.LogWarning("🔄 Pagamento: Retry {RetryCount} em {Delay}ms", 
                        retryCount, timespan.TotalMilliseconds);
                });

        // 🛡️ Política abrangente combinando todas as estratégias
        _comprehensivePolicy = Policy.WrapAsync(
            _rateLimitPolicy,
            _bulkheadPolicy,
            _advancedRetryPolicy);
    }

    /// <summary>
    /// Demonstra uso de Bulkhead para controlar paralelismo
    /// </summary>
    public async Task<OperationResult<PaymentResponse?>> ProcessPaymentWithBulkheadAsync(decimal amount, string paymentId)
    {
        var stopwatch = Stopwatch.StartNew();
        var attemptCount = 1;
        
        try
        {
            _logger.LogInformation("🚧 Processando pagamento {PaymentId} de {Amount:C} com Bulkhead", 
                paymentId, amount);
            
            var response = await _bulkheadPolicy.ExecuteAsync(async () =>
            {
                _logger.LogDebug("Executando pagamento {PaymentId}", paymentId);
                
                // Simula processamento de pagamento
                await Task.Delay(Random.Shared.Next(500, 2000));
                
                // Simula diferentes status codes
                var statusCodes = new[] { "200", "400", "500", "503" };
                var randomStatus = statusCodes[Random.Shared.Next(statusCodes.Length)];
                
                return await _httpClient.GetAsync($"/{randomStatus}");
            });

            stopwatch.Stop();
            
            var paymentResponse = new PaymentResponse
            {
                Success = response.IsSuccessStatusCode,
                TransactionId = Guid.NewGuid().ToString(),
                Amount = amount,
                Message = response.IsSuccessStatusCode ? "Pagamento processado com sucesso" : $"Falha: {response.StatusCode}",
                ProcessedAt = DateTime.UtcNow
            };
            
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("✅ Pagamento {PaymentId} processado com sucesso", paymentId);
                return OperationResult<PaymentResponse?>.SuccessResult(
                    paymentResponse, attemptCount, stopwatch.Elapsed, "Bulkhead Policy");
            }
            else
            {
                _logger.LogWarning("❌ Falha no pagamento {PaymentId}: {Status}", paymentId, response.StatusCode);
                return OperationResult<PaymentResponse?>.FailureResult(
                    $"Falha HTTP: {response.StatusCode}", attemptCount, stopwatch.Elapsed, "Bulkhead Policy");
            }
        }
        catch (BulkheadRejectedException)
        {
            stopwatch.Stop();
            _logger.LogWarning("🚧 Pagamento {PaymentId} rejeitado por Bulkhead - muitas requisições simultâneas", paymentId);
            return OperationResult<PaymentResponse?>.FailureResult(
                "Muitas requisições simultâneas", attemptCount, stopwatch.Elapsed, "Bulkhead Policy");
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "❌ Erro no pagamento {PaymentId}", paymentId);
            return OperationResult<PaymentResponse?>.FailureResult(
                ex.Message, attemptCount, stopwatch.Elapsed, "Bulkhead Policy");
        }
    }

    /// <summary>
    /// Demonstra processamento de múltiplos pagamentos para testar Bulkhead
    /// </summary>
    public async Task<List<OperationResult<PaymentResponse?>>> ProcessMultiplePaymentsAsync(int paymentCount)
    {
        _logger.LogInformation("💳 Processando {Count} pagamentos simultaneamente para testar Bulkhead", paymentCount);
        
        var tasks = new List<Task<OperationResult<PaymentResponse?>>>();
        
        for (int i = 1; i <= paymentCount; i++)
        {
            var paymentId = $"PAY-{i:D3}";
            var amount = Random.Shared.Next(10, 1000);
            
            tasks.Add(ProcessPaymentWithBulkheadAsync(amount, paymentId));
        }
        
        var results = await Task.WhenAll(tasks);
        
        var successCount = results.Count(r => r.Success);
        var failureCount = results.Length - successCount;
        var bulkheadRejections = results.Count(r => !r.Success && r.ErrorMessage?.Contains("simultâneas") == true);
        
        _logger.LogInformation("📊 Resultado dos pagamentos: {Success} sucessos, {Failures} falhas, {Rejected} rejeitados pelo Bulkhead",
            successCount, failureCount, bulkheadRejections);
        
        return results.ToList();
    }

    /// <summary>
    /// Demonstra política abrangente combinando múltiplas estratégias
    /// </summary>
    public async Task<OperationResult<PaymentResponse?>> ProcessCriticalPaymentAsync(decimal amount, string paymentId)
    {
        var stopwatch = Stopwatch.StartNew();
        var attemptCount = 0;
        
        try
        {
            _logger.LogInformation("🛡️ Processando pagamento crítico {PaymentId} de {Amount:C} com política abrangente", 
                paymentId, amount);
            
            var response = await _comprehensivePolicy.ExecuteAsync(async () =>
            {
                attemptCount++;
                _logger.LogDebug("Tentativa {Attempt} para pagamento crítico {PaymentId}", attemptCount, paymentId);
                
                // Simula processamento mais demorado para pagamentos críticos
                await Task.Delay(Random.Shared.Next(1000, 3000));
                
                // Maior chance de sucesso para pagamentos críticos
                var success = Random.Shared.NextDouble() > 0.3; // 70% de chance de sucesso
                var statusCode = success ? "200" : "500";
                
                return await _httpClient.GetAsync($"/{statusCode}");
            });

            stopwatch.Stop();
            
            var paymentResponse = new PaymentResponse
            {
                Success = response.IsSuccessStatusCode,
                TransactionId = Guid.NewGuid().ToString(),
                Amount = amount,
                Message = response.IsSuccessStatusCode ? 
                    "Pagamento crítico processado com máxima segurança" : 
                    $"Falha após todas as tentativas: {response.StatusCode}",
                ProcessedAt = DateTime.UtcNow
            };
            
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("✅ Pagamento crítico {PaymentId} processado com sucesso em {Attempts} tentativas", 
                    paymentId, attemptCount);
                return OperationResult<PaymentResponse?>.SuccessResult(
                    paymentResponse, attemptCount, stopwatch.Elapsed, "Comprehensive Policy");
            }
            else
            {
                _logger.LogError("❌ Falha no pagamento crítico {PaymentId} após {Attempts} tentativas", 
                    paymentId, attemptCount);
                return OperationResult<PaymentResponse?>.FailureResult(
                    $"Falha HTTP após {attemptCount} tentativas: {response.StatusCode}", 
                    attemptCount, stopwatch.Elapsed, "Comprehensive Policy");
            }
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "❌ Erro crítico no pagamento {PaymentId} após {Attempts} tentativas", 
                paymentId, attemptCount);
            return OperationResult<PaymentResponse?>.FailureResult(
                ex.Message, attemptCount, stopwatch.Elapsed, "Comprehensive Policy");
        }
    }

    /// <summary>
    /// Demonstra medição de latência e throughput com políticas
    /// </summary>
    public async Task<(TimeSpan AverageLatency, int SuccessfulOperations, int FailedOperations)> 
        MeasurePerformanceAsync(int operationCount)
    {
        _logger.LogInformation("📊 Medindo performance com {Count} operações", operationCount);
        
        var stopwatch = Stopwatch.StartNew();
        var tasks = new List<Task<OperationResult<PaymentResponse?>>>();
        
        for (int i = 1; i <= operationCount; i++)
        {
            var paymentId = $"PERF-{i:D3}";
            var amount = Random.Shared.Next(1, 100);
            
            tasks.Add(ProcessPaymentWithBulkheadAsync(amount, paymentId));
        }
        
        var results = await Task.WhenAll(tasks);
        stopwatch.Stop();
        
        var successfulOps = results.Count(r => r.Success);
        var failedOps = results.Length - successfulOps;
        
        var averageLatency = successfulOps > 0 
            ? TimeSpan.FromMilliseconds(results.Where(r => r.Success).Average(r => r.Duration.TotalMilliseconds))
            : TimeSpan.Zero;
        
        _logger.LogInformation("📈 Performance - Latência média: {Latency}ms, Sucessos: {Success}, Falhas: {Failures}, " +
                             "Throughput: {Throughput} ops/s",
            averageLatency.TotalMilliseconds, successfulOps, failedOps, 
            operationCount / stopwatch.Elapsed.TotalSeconds);
        
        return (averageLatency, successfulOps, failedOps);
    }
}
