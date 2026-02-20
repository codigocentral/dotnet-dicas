using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Http.Resilience;
using Polly;
using Polly.Extensions.Http;
using Dica78.MicroservicesCommunication.Services;
using System.Net;

var builder = Host.CreateApplicationBuilder(args);

// Configuração de logging
builder.Services.AddLogging(logging =>
{
    logging.ClearProviders();
    logging.AddConsole();
    logging.SetMinimumLevel(LogLevel.Information);
});

// Configuração de HttpClient com resilience patterns
builder.Services.AddHttpClient("resilient-client", client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Add("User-Agent", "MicroservicesCommunication/1.0");
})
.AddStandardResilienceHandler(options =>
{
    // Retry configuration
    options.Retry.MaxRetryAttempts = 3;
    options.Retry.BackoffType = Polly.DelayBackoffType.Exponential;
    options.Retry.UseJitter = true;
    
    // Circuit breaker configuration
    options.CircuitBreaker.FailureRatio = 0.5;
    options.CircuitBreaker.MinimumThroughput = 5;
    options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(30);
    options.CircuitBreaker.BreakDuration = TimeSpan.FromSeconds(15);
    
    // Timeout configuration
    options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(10);
    options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(5);
});

// Registrar serviços de microservices
builder.Services.AddSingleton<HttpCommunicationService>();
builder.Services.AddSingleton<MessageBroker>();
builder.Services.AddSingleton<MessageQueueService>();
builder.Services.AddSingleton<EventStore>();
builder.Services.AddSingleton<EventSourcingService>();
builder.Services.AddSingleton<CircuitBreakerDemo>();
builder.Services.AddSingleton<ServiceRegistry>();
builder.Services.AddSingleton<ServiceDiscovery>();
builder.Services.AddSingleton<HealthCheckService>();

var host = builder.Build();

// Demonstração dos padrões de comunicação
var logger = host.Services.GetRequiredService<ILogger<Program>>();

Console.WriteLine("🚀 Dica 78: Microservices Communication Patterns");
Console.WriteLine("================================================");
Console.WriteLine();

try
{
    // 1. HTTP Communication Patterns
    Console.WriteLine("🌐 PARTE 1: HTTP Communication Patterns");
    Console.WriteLine("========================================");
    var httpService = host.Services.GetRequiredService<HttpCommunicationService>();
    await httpService.DemonstrateHttpPatterns();
    
    await WaitForKeyPress("Pressione qualquer tecla para continuar com Message Queue Patterns...");
    
    // 2. Message Queue Patterns
    Console.WriteLine("\n� PARTE 2: Message Queue Patterns");
    Console.WriteLine("==================================");
    var messageQueueService = host.Services.GetRequiredService<MessageQueueService>();
    await messageQueueService.DemonstrateMessagePatterns();
    
    await WaitForKeyPress("Pressione qualquer tecla para continuar com Event Sourcing...");
    
    // 3. Event Sourcing Patterns
    Console.WriteLine("\n� PARTE 3: Event Sourcing Patterns");
    Console.WriteLine("==================================");
    var eventSourcingService = host.Services.GetRequiredService<EventSourcingService>();
    await eventSourcingService.DemonstrateEventSourcing();
    
    await WaitForKeyPress("Pressione qualquer tecla para continuar com Circuit Breaker...");
    
    // 4. Circuit Breaker Patterns
    Console.WriteLine("\n⚡ PARTE 4: Circuit Breaker Patterns");
    Console.WriteLine("===================================");
    var circuitBreakerDemo = host.Services.GetRequiredService<CircuitBreakerDemo>();
    await circuitBreakerDemo.DemonstrateCircuitBreakerPattern();
    
    await WaitForKeyPress("Pressione qualquer tecla para continuar com Service Discovery...");
    
    // 5. Service Discovery Patterns
    Console.WriteLine("\n🔍 PARTE 5: Service Discovery Patterns");
    Console.WriteLine("=====================================");
    var serviceDiscovery = host.Services.GetRequiredService<ServiceDiscovery>();
    await serviceDiscovery.DemonstrateServiceDiscovery();
    
    await WaitForKeyPress("Pressione qualquer tecla para continuar com Health Checks...");
    
    // 6. Health Check Patterns
    Console.WriteLine("\n🏥 PARTE 6: Health Check Patterns");
    Console.WriteLine("=================================");
    var healthCheckService = host.Services.GetRequiredService<HealthCheckService>();
    await healthCheckService.DemonstrateHealthChecks();
}
catch (Exception ex)
{
    logger.LogError(ex, "Erro durante demonstração");
    Console.WriteLine($"❌ Erro: {ex.Message}");
}

Console.WriteLine("\n🎉 Demonstração Completa!");
Console.WriteLine("=========================");
Console.WriteLine("✅ HTTP Communication Patterns - Resilience com Polly");
Console.WriteLine("✅ Message Queue Patterns - Pub/Sub, Request/Reply, Event-Driven");
Console.WriteLine("✅ Event Sourcing - Store de eventos, Projeções, Replay");
Console.WriteLine("✅ Circuit Breaker - Falha rápida, Recuperação automática");
Console.WriteLine("✅ Service Discovery - Registro, Lookup, Load Balancing");
Console.WriteLine("✅ Health Checks - Monitoramento, Métricas, Alertas");
Console.WriteLine("\nPressione qualquer tecla para sair...");
Console.ReadKey();

static async Task WaitForKeyPress(string message)
{
    Console.WriteLine($"\n{message}");
    Console.ReadKey();
    Console.WriteLine();
    await Task.Delay(100);
}
