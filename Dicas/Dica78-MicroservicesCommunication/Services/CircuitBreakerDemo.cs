using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;

namespace Dica78.MicroservicesCommunication.Services;

public class CircuitBreakerDemo
{
    private readonly ILogger<CircuitBreakerDemo> _logger;
    private readonly ResiliencePipeline _circuitBreakerPipeline;
    private int _failureCount = 0;
    private readonly Random _random = new();

    public CircuitBreakerDemo(ILogger<CircuitBreakerDemo> logger)
    {
        _logger = logger;
        
        // Configurar Circuit Breaker com Polly v8
        _circuitBreakerPipeline = new ResiliencePipelineBuilder()
            .AddCircuitBreaker(new CircuitBreakerStrategyOptions
            {
                ShouldHandle = new PredicateBuilder().Handle<Exception>(),
                FailureRatio = 0.5, // 50% de falhas
                MinimumThroughput = 3, // Mínimo 3 requests
                SamplingDuration = TimeSpan.FromSeconds(30), // Janela de amostragem
                BreakDuration = TimeSpan.FromSeconds(10), // Tempo no estado Open
                OnOpened = args =>
                {
                    _logger.LogWarning("🔴 Circuit Breaker OPENED - Falhas detectadas");
                    Console.WriteLine("🔴 Circuit Breaker OPENED - Serviço indisponível");
                    return ValueTask.CompletedTask;
                },
                OnClosed = args =>
                {
                    _logger.LogInformation("🟢 Circuit Breaker CLOSED - Serviço recuperado");
                    Console.WriteLine("🟢 Circuit Breaker CLOSED - Serviço funcionando");
                    return ValueTask.CompletedTask;
                },
                OnHalfOpened = args =>
                {
                    _logger.LogInformation("🟡 Circuit Breaker HALF-OPEN - Testando recuperação");
                    Console.WriteLine("🟡 Circuit Breaker HALF-OPEN - Testando serviço");
                    return ValueTask.CompletedTask;
                }
            })
            .Build();
    }

    public async Task DemonstrateCircuitBreakerPattern()
    {
        Console.WriteLine("⚡ Circuit Breaker Pattern Demonstration");
        Console.WriteLine("=======================================");

        await DemonstrateNormalOperation();
        await DemonstrateFailureDetection();
        await DemonstrateCircuitOpen();
        await DemonstrateRecovery();
    }

    private async Task DemonstrateNormalOperation()
    {
        Console.WriteLine("\n🟢 1. Operação Normal");
        Console.WriteLine("--------------------");

        for (int i = 1; i <= 5; i++)
        {
            try
            {
                var result = await _circuitBreakerPipeline.ExecuteAsync(async _ =>
                {
                    return await SimulateServiceCall(successRate: 0.9); // 90% sucesso
                });

                Console.WriteLine($"✅ Request {i}: {result}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Request {i}: {ex.Message}");
            }

            await Task.Delay(500);
        }
    }

    private async Task DemonstrateFailureDetection()
    {
        Console.WriteLine("\n🟡 2. Detecção de Falhas");
        Console.WriteLine("-----------------------");

        for (int i = 1; i <= 6; i++)
        {
            try
            {
                var result = await _circuitBreakerPipeline.ExecuteAsync(async _ =>
                {
                    return await SimulateServiceCall(successRate: 0.2); // 20% sucesso
                });

                Console.WriteLine($"✅ Request {i}: {result}");
            }
            catch (BrokenCircuitException)
            {
                Console.WriteLine($"🔴 Request {i}: Circuit Breaker OPEN - Falha rápida");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Request {i}: {ex.Message}");
            }

            await Task.Delay(500);
        }
    }

    private async Task DemonstrateCircuitOpen()
    {
        Console.WriteLine("\n🔴 3. Circuit Breaker Aberto");
        Console.WriteLine("---------------------------");

        for (int i = 1; i <= 5; i++)
        {
            try
            {
                var result = await _circuitBreakerPipeline.ExecuteAsync(async _ =>
                {
                    return await SimulateServiceCall(successRate: 0.9); // Serviço funcionando
                });

                Console.WriteLine($"✅ Request {i}: {result}");
            }
            catch (BrokenCircuitException)
            {
                Console.WriteLine($"🔴 Request {i}: Circuit Breaker OPEN - Bloqueado");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Request {i}: {ex.Message}");
            }

            await Task.Delay(1000);
        }
    }

    private async Task DemonstrateRecovery()
    {
        Console.WriteLine("\n🟢 4. Recuperação do Serviço");
        Console.WriteLine("---------------------------");

        Console.WriteLine("⏰ Aguardando período de break (10 segundos)...");
        await Task.Delay(11000); // Aguarda o break duration + margem

        for (int i = 1; i <= 8; i++)
        {
            try
            {
                var result = await _circuitBreakerPipeline.ExecuteAsync(async _ =>
                {
                    return await SimulateServiceCall(successRate: 0.9); // Serviço recuperado
                });

                Console.WriteLine($"✅ Request {i}: {result}");
            }
            catch (BrokenCircuitException)
            {
                Console.WriteLine($"🔴 Request {i}: Circuit Breaker ainda OPEN");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Request {i}: {ex.Message}");
            }

            await Task.Delay(1000);
        }
    }

    private async Task<string> SimulateServiceCall(double successRate = 0.8)
    {
        await Task.Delay(100 + _random.Next(0, 200)); // Simula latência variável

        if (_random.NextDouble() > successRate)
        {
            var errorTypes = new[]
            {
                "TimeoutException: Request timed out",
                "HttpRequestException: Service unavailable",
                "TaskCanceledException: Operation cancelled",
                "InvalidOperationException: Service overloaded"
            };

            throw new Exception(errorTypes[_random.Next(errorTypes.Length)]);
        }

        var responses = new[]
        {
            "Dados processados com sucesso",
            "Operação concluída",
            "Request processado",
            "Resposta do serviço remoto",
            "Dados recuperados da API"
        };

        return responses[_random.Next(responses.Length)];
    }
}

public class CircuitBreakerMetrics
{
    private readonly ILogger<CircuitBreakerMetrics> _logger;
    private int _totalRequests = 0;
    private int _successfulRequests = 0;
    private int _failedRequests = 0;
    private int _circuitBreakerActivations = 0;
    private DateTime _lastResetTime = DateTime.UtcNow;

    public CircuitBreakerMetrics(ILogger<CircuitBreakerMetrics> logger)
    {
        _logger = logger;
    }

    public void RecordRequest(bool successful)
    {
        Interlocked.Increment(ref _totalRequests);
        
        if (successful)
            Interlocked.Increment(ref _successfulRequests);
        else
            Interlocked.Increment(ref _failedRequests);
    }

    public void RecordCircuitBreakerActivation()
    {
        Interlocked.Increment(ref _circuitBreakerActivations);
    }

    public CircuitBreakerStats GetStats()
    {
        var total = _totalRequests;
        var successful = _successfulRequests;
        var failed = _failedRequests;
        var activations = _circuitBreakerActivations;
        var uptime = DateTime.UtcNow - _lastResetTime;

        return new CircuitBreakerStats
        {
            TotalRequests = total,
            SuccessfulRequests = successful,
            FailedRequests = failed,
            SuccessRate = total > 0 ? Math.Round((double)successful / total * 100, 2) : 0,
            FailureRate = total > 0 ? Math.Round((double)failed / total * 100, 2) : 0,
            CircuitBreakerActivations = activations,
            UptimeHours = Math.Round(uptime.TotalHours, 2)
        };
    }

    public void Reset()
    {
        _totalRequests = 0;
        _successfulRequests = 0;
        _failedRequests = 0;
        _circuitBreakerActivations = 0;
        _lastResetTime = DateTime.UtcNow;
        
        _logger.LogInformation("📊 Circuit Breaker metrics reset");
    }

    public void DisplayStats()
    {
        var stats = GetStats();
        
        Console.WriteLine("\n📊 Circuit Breaker Statistics");
        Console.WriteLine("=============================");
        Console.WriteLine($"Total Requests: {stats.TotalRequests}");
        Console.WriteLine($"Successful: {stats.SuccessfulRequests} ({stats.SuccessRate}%)");
        Console.WriteLine($"Failed: {stats.FailedRequests} ({stats.FailureRate}%)");
        Console.WriteLine($"Circuit Breaker Activations: {stats.CircuitBreakerActivations}");
        Console.WriteLine($"Uptime: {stats.UptimeHours} hours");
    }
}

public class CircuitBreakerStats
{
    public int TotalRequests { get; set; }
    public int SuccessfulRequests { get; set; }
    public int FailedRequests { get; set; }
    public double SuccessRate { get; set; }
    public double FailureRate { get; set; }
    public int CircuitBreakerActivations { get; set; }
    public double UptimeHours { get; set; }
}

public class AdvancedCircuitBreakerService
{
    private readonly Dictionary<string, ResiliencePipeline> _circuitBreakers = new();
    private readonly ILogger<AdvancedCircuitBreakerService> _logger;

    public AdvancedCircuitBreakerService(ILogger<AdvancedCircuitBreakerService> logger)
    {
        _logger = logger;
        InitializeCircuitBreakers();
    }

    private void InitializeCircuitBreakers()
    {
        // Circuit Breaker para serviços críticos (mais restritivo)
        _circuitBreakers["critical"] = new ResiliencePipelineBuilder()
            .AddCircuitBreaker(new CircuitBreakerStrategyOptions
            {
                ShouldHandle = new PredicateBuilder().Handle<Exception>(),
                FailureRatio = 0.3, // 30% falhas
                MinimumThroughput = 5,
                SamplingDuration = TimeSpan.FromSeconds(60),
                BreakDuration = TimeSpan.FromSeconds(30)
            })
            .Build();

        // Circuit Breaker para serviços não-críticos (mais permissivo)
        _circuitBreakers["non-critical"] = new ResiliencePipelineBuilder()
            .AddCircuitBreaker(new CircuitBreakerStrategyOptions
            {
                ShouldHandle = new PredicateBuilder().Handle<Exception>(),
                FailureRatio = 0.7, // 70% falhas
                MinimumThroughput = 3,
                SamplingDuration = TimeSpan.FromSeconds(30),
                BreakDuration = TimeSpan.FromSeconds(10)
            })
            .Build();

        // Circuit Breaker para serviços externos (balanceado)
        _circuitBreakers["external"] = new ResiliencePipelineBuilder()
            .AddCircuitBreaker(new CircuitBreakerStrategyOptions
            {
                ShouldHandle = new PredicateBuilder().Handle<Exception>(),
                FailureRatio = 0.5, // 50% falhas
                MinimumThroughput = 4,
                SamplingDuration = TimeSpan.FromSeconds(45),
                BreakDuration = TimeSpan.FromSeconds(20)
            })
            .Build();
    }

    public async Task<T> ExecuteWithCircuitBreaker<T>(string serviceType, Func<Task<T>> operation)
    {
        if (!_circuitBreakers.TryGetValue(serviceType, out var circuitBreaker))
        {
            throw new ArgumentException($"Circuit breaker '{serviceType}' não encontrado");
        }

        return await circuitBreaker.ExecuteAsync(async _ => await operation());
    }

    public async Task DemonstrateMultipleCircuitBreakers()
    {
        Console.WriteLine("🔄 Multiple Circuit Breakers Demo");
        Console.WriteLine("=================================");

        var tasks = new List<Task>
        {
            DemonstrateServiceType("critical", "💎 Critical Service"),
            DemonstrateServiceType("non-critical", "📊 Analytics Service"),
            DemonstrateServiceType("external", "🌐 External API")
        };

        await Task.WhenAll(tasks);
    }

    private async Task DemonstrateServiceType(string serviceType, string serviceName)
    {
        Console.WriteLine($"\n{serviceName} ({serviceType}):");
        
        for (int i = 1; i <= 5; i++)
        {
            try
            {
                var result = await ExecuteWithCircuitBreaker(serviceType, async () =>
                {
                    // Simula diferentes taxas de falha por tipo de serviço
                    var successRate = serviceType switch
                    {
                        "critical" => 0.8,
                        "non-critical" => 0.6,
                        "external" => 0.4,
                        _ => 0.5
                    };

                    await Task.Delay(Random.Shared.Next(50, 200));
                    
                    if (Random.Shared.NextDouble() > successRate)
                        throw new Exception($"{serviceName} failure");
                    
                    return $"{serviceName} response {i}";
                });

                Console.WriteLine($"  ✅ {result}");
            }
            catch (BrokenCircuitException)
            {
                Console.WriteLine($"  🔴 {serviceName}: Circuit OPEN");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  ❌ {serviceName}: {ex.Message}");
            }

            await Task.Delay(1000);
        }
    }
}
