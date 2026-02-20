using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Dica43_Polly.Services;
using Dica43_Polly.Models;

namespace Dica43_Polly.Services;

/// <summary>
/// Serviço hospedado que demonstra todas as funcionalidades do Polly
/// </summary>
public class PollyDemoHostedService : BackgroundService
{
    private readonly ILogger<PollyDemoHostedService> _logger;
    private readonly ExternalApiService _externalApiService;
    private readonly PaymentService _paymentService;

    public PollyDemoHostedService(
        ILogger<PollyDemoHostedService> logger,
        ExternalApiService externalApiService,
        PaymentService paymentService)
    {
        _logger = logger;
        _externalApiService = externalApiService;
        _paymentService = paymentService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            _logger.LogInformation("🚀 Iniciando demonstração do Polly - Padrões de Resiliência");
            _logger.LogInformation("=================================================================");
            
            await DemonstrateRetryPolicyAsync();
            await Task.Delay(2000, stoppingToken);
            
            await DemonstrateCircuitBreakerAsync();
            await Task.Delay(2000, stoppingToken);
            
            await DemonstrateCombinedPolicyAsync();
            await Task.Delay(2000, stoppingToken);
            
            await DemonstrateFallbackPolicyAsync();
            await Task.Delay(2000, stoppingToken);
            
            await DemonstrateBulkheadPolicyAsync();
            await Task.Delay(2000, stoppingToken);
            
            await DemonstratePerformanceMeasurementAsync();
            await Task.Delay(2000, stoppingToken);
            
            await ShowBestPracticesAsync();
            
            _logger.LogInformation("=================================================================");
            _logger.LogInformation("✅ Demonstração do Polly concluída com sucesso!");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Erro durante demonstração do Polly");
        }
    }

    private async Task DemonstrateRetryPolicyAsync()
    {
        _logger.LogInformation("📖 1. Demonstrando Retry Policy com Exponential Backoff");
        _logger.LogInformation("----------------------------------------------------------");
        
        // Testa retry com um post válido
        var result1 = await _externalApiService.GetPostWithRetryAsync(1);
        LogOperationResult("Busca de Post com Retry", result1);
        
        await Task.Delay(1000);
        
        // Testa retry com um post inválido para forçar retry
        var result2 = await _externalApiService.GetPostWithRetryAsync(999);
        LogOperationResult("Busca de Post Inválido (Retry)", result2);
    }

    private async Task DemonstrateCircuitBreakerAsync()
    {
        _logger.LogInformation("");
        _logger.LogInformation("⚡ 2. Demonstrando Circuit Breaker Pattern");
        _logger.LogInformation("------------------------------------------");
        
        // Primeiro, tenta uma operação normal
        var result1 = await _externalApiService.GetPostsWithCircuitBreakerAsync();
        LogOperationResult("Busca Normal com Circuit Breaker", result1);
        
        await Task.Delay(1000);
        
        // Agora simula falhas para abrir o circuit breaker
        _logger.LogInformation("");
        _logger.LogInformation("🧪 Simulando falhas para abrir o Circuit Breaker:");
        var failureResults = await _externalApiService.SimulateFailuresForCircuitBreakerAsync();
        
        foreach (var result in failureResults)
        {
            var status = result.Success ? "✅" : "❌";
            _logger.LogInformation("{Status} {Message} ({Duration}ms)", 
                status, result.Data ?? result.ErrorMessage, result.Duration.TotalMilliseconds);
        }
    }

    private async Task DemonstrateCombinedPolicyAsync()
    {
        _logger.LogInformation("");
        _logger.LogInformation("🛡️ 3. Demonstrando Política Combinada (Timeout + Circuit Breaker + Retry)");
        _logger.LogInformation("--------------------------------------------------------------------------");
        
        var result = await _externalApiService.GetCommentWithCombinedPolicyAsync(1);
        LogOperationResult("Busca de Comentário com Política Combinada", result);
    }

    private async Task DemonstrateFallbackPolicyAsync()
    {
        _logger.LogInformation("");
        _logger.LogInformation("🔄 4. Demonstrando Fallback Policy");
        _logger.LogInformation("-----------------------------------");
        
        var result = await _externalApiService.GetPostWithFallbackAsync(1);
        LogOperationResult("Busca com Fallback (forçando erro)", result);
        
        if (result.Success && result.Data != null)
        {
            _logger.LogInformation("📄 Dados do Fallback:");
            _logger.LogInformation("   Título: {Title}", result.Data.Title);
            _logger.LogInformation("   Corpo: {Body}", result.Data.Body);
        }
    }

    private async Task DemonstrateBulkheadPolicyAsync()
    {
        _logger.LogInformation("");
        _logger.LogInformation("🚧 5. Demonstrando Bulkhead Policy (Controle de Paralelismo)");
        _logger.LogInformation("--------------------------------------------------------------");
        
        // Processa múltiplos pagamentos para testar o bulkhead
        var results = await _paymentService.ProcessMultiplePaymentsAsync(15);
        
        _logger.LogInformation("");
        _logger.LogInformation("📊 Resultados dos Pagamentos em Lote:");
        
        var successCount = results.Count(r => r.Success);
        var failureCount = results.Count(r => !r.Success && !r.ErrorMessage?.Contains("simultâneas") == true);
        var rejectedCount = results.Count(r => r.ErrorMessage?.Contains("simultâneas") == true);
        
        _logger.LogInformation("✅ Sucessos: {Success}", successCount);
        _logger.LogInformation("❌ Falhas: {Failures}", failureCount);
        _logger.LogInformation("🚧 Rejeitados pelo Bulkhead: {Rejected}", rejectedCount);
        
        // Demonstra pagamento crítico com política abrangente
        _logger.LogInformation("");
        _logger.LogInformation("💎 Processando pagamento crítico com política abrangente:");
        var criticalResult = await _paymentService.ProcessCriticalPaymentAsync(5000.00m, "CRITICAL-001");
        LogOperationResult("Pagamento Crítico", criticalResult);
    }

    private async Task DemonstratePerformanceMeasurementAsync()
    {
        _logger.LogInformation("");
        _logger.LogInformation("📊 6. Demonstrando Medição de Performance");
        _logger.LogInformation("------------------------------------------");
        
        var (averageLatency, successful, failed) = await _paymentService.MeasurePerformanceAsync(20);
        
        _logger.LogInformation("📈 Métricas de Performance:");
        _logger.LogInformation("   Latência Média: {Latency}ms", averageLatency.TotalMilliseconds);
        _logger.LogInformation("   Operações Bem-sucedidas: {Success}", successful);
        _logger.LogInformation("   Operações Falhadas: {Failed}", failed);
        _logger.LogInformation("   Taxa de Sucesso: {Rate:P1}", (double)successful / (successful + failed));
    }

    private async Task ShowBestPracticesAsync()
    {
        _logger.LogInformation("");
        _logger.LogInformation("🎯 7. Melhores Práticas do Polly");
        _logger.LogInformation("----------------------------------");
        
        _logger.LogInformation("");
        _logger.LogInformation("🌟 Resumo das Políticas de Resiliência:");
        _logger.LogInformation("");
        
        _logger.LogInformation("📌 Retry Policy:");
        _logger.LogInformation("   ✅ Use para falhas transitórias (network timeouts, HTTP 5xx)");
        _logger.LogInformation("   ✅ Implemente Exponential Backoff com Jitter");
        _logger.LogInformation("   ✅ Limite o número de tentativas (3-5)");
        _logger.LogInformation("   💡 Exemplo: Chamadas de API externa, operações de I/O");
        _logger.LogInformation("");
        
        _logger.LogInformation("📌 Circuit Breaker:");
        _logger.LogInformation("   ✅ Protege against cascading failures");
        _logger.LogInformation("   ✅ Permite recuperação automática");
        _logger.LogInformation("   ✅ Monitora health do serviço downstream");
        _logger.LogInformation("   💡 Exemplo: Serviços críticos, databases, APIs de terceiros");
        _logger.LogInformation("");
        
        _logger.LogInformation("📌 Timeout Policy:");
        _logger.LogInformation("   ✅ Define limites de tempo claros");
        _logger.LogInformation("   ✅ Evita operações infinitas");
        _logger.LogInformation("   ✅ Use timeouts diferentes por operação");
        _logger.LogInformation("   💡 Exemplo: HTTP requests, database queries");
        _logger.LogInformation("");
        
        _logger.LogInformation("📌 Bulkhead Policy:");
        _logger.LogInformation("   ✅ Isola recursos críticos");
        _logger.LogInformation("   ✅ Controla paralelismo");
        _logger.LogInformation("   ✅ Evita resource starvation");
        _logger.LogInformation("   💡 Exemplo: Thread pools, connection pools");
        _logger.LogInformation("");
        
        _logger.LogInformation("📌 Fallback Policy:");
        _logger.LogInformation("   ✅ Fornece resposta alternativa");
        _logger.LogInformation("   ✅ Mantém funcionalidade degradada");
        _logger.LogInformation("   ✅ Melhora experiência do usuário");
        _logger.LogInformation("   💡 Exemplo: Cache, dados padrão, mensagens amigáveis");
        _logger.LogInformation("");
        
        _logger.LogInformation("🏗️ Arquitetura Recomendada:");
        _logger.LogInformation("   1. Use HttpClientFactory com Polly");
        _logger.LogInformation("   2. Configure políticas via DI");
        _logger.LogInformation("   3. Monitore métricas de resiliência");
        _logger.LogInformation("   4. Teste cenários de falha");
        _logger.LogInformation("   5. Documente comportamento esperado");
        _logger.LogInformation("");
        
        _logger.LogInformation("⚠️  Cuidados Importantes:");
        _logger.LogInformation("   ❌ Não use retry para erros não transitórios (4xx)");
        _logger.LogInformation("   ❌ Não configure timeouts muito baixos");
        _logger.LogInformation("   ❌ Não abuse de circuit breakers");
        _logger.LogInformation("   ❌ Não ignore logs e métricas");
        _logger.LogInformation("   ❌ Não esqueça de testar em produção");
        
        await Task.CompletedTask;
    }

    private void LogOperationResult<T>(string operation, OperationResult<T> result)
    {
        var status = result.Success ? "✅" : "❌";
        _logger.LogInformation("{Status} {Operation}:", status, operation);
        _logger.LogInformation("   Sucesso: {Success}", result.Success);
        _logger.LogInformation("   Tentativas: {Attempts}", result.AttemptCount);
        _logger.LogInformation("   Duração: {Duration}ms", result.Duration.TotalMilliseconds);
        _logger.LogInformation("   Política: {Policy}", result.PolicyApplied);
        
        if (!result.Success)
        {
            _logger.LogInformation("   Erro: {Error}", result.ErrorMessage);
        }
        
        if (result.Success && result.Data != null && typeof(T).Name != "String")
        {
            var type = typeof(T);
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            {
                var count = ((System.Collections.IList)result.Data).Count;
                _logger.LogInformation("   Itens retornados: {Count}", count);
            }
        }
    }
}
