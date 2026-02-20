using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Extensions.Http;
using System.Net;
using Dica43_Polly.Configuration;
using Dica43_Polly.Services;

namespace Dica43_Polly;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("🎯 Dica 43: Polly - Padrões de Resiliência em .NET");
        Console.WriteLine("====================================================");
        
        // Configura o host
        var host = Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                // Registra configurações
                services.Configure<ExternalApiSettings>(
                    context.Configuration.GetSection(ExternalApiSettings.SectionName));
                services.Configure<PaymentApiSettings>(
                    context.Configuration.GetSection(PaymentApiSettings.SectionName));
                services.Configure<CircuitBreakerSettings>(
                    context.Configuration.GetSection(CircuitBreakerSettings.SectionName));
                services.Configure<BulkheadSettings>(
                    context.Configuration.GetSection(BulkheadSettings.SectionName));
                services.Configure<TimeoutSettings>(
                    context.Configuration.GetSection(TimeoutSettings.SectionName));
                
                // Valida configurações na inicialização
                services.AddOptions<ExternalApiSettings>()
                    .Bind(context.Configuration.GetSection(ExternalApiSettings.SectionName))
                    .ValidateDataAnnotations()
                    .ValidateOnStart();
                
                services.AddOptions<PaymentApiSettings>()
                    .Bind(context.Configuration.GetSection(PaymentApiSettings.SectionName))
                    .ValidateDataAnnotations()
                    .ValidateOnStart();
                
                services.AddOptions<CircuitBreakerSettings>()
                    .Bind(context.Configuration.GetSection(CircuitBreakerSettings.SectionName))
                    .ValidateDataAnnotations()
                    .ValidateOnStart();
                
                // Configura HttpClient para External API com políticas do Polly
                services.AddHttpClient("ExternalApi", (serviceProvider, client) =>
                {
                    var externalApiSettings = context.Configuration
                        .GetSection(ExternalApiSettings.SectionName)
                        .Get<ExternalApiSettings>()!;
                    
                    client.BaseAddress = new Uri(externalApiSettings.BaseUrl);
                    client.Timeout = externalApiSettings.Timeout;
                    client.DefaultRequestHeaders.Add("User-Agent", "Dica43-Polly-Demo/1.0");
                })
                .AddPolicyHandler(GetRetryPolicy())
                .AddPolicyHandler(GetCircuitBreakerPolicy());
                
                // Configura HttpClient para Payment API
                services.AddHttpClient("PaymentApi", (serviceProvider, client) =>
                {
                    var paymentApiSettings = context.Configuration
                        .GetSection(PaymentApiSettings.SectionName)
                        .Get<PaymentApiSettings>()!;
                    
                    client.BaseAddress = new Uri(paymentApiSettings.BaseUrl);
                    client.Timeout = paymentApiSettings.Timeout;
                    client.DefaultRequestHeaders.Add("User-Agent", "Dica43-Polly-Payment/1.0");
                })
                .AddPolicyHandler(GetAdvancedRetryPolicy())
                .AddPolicyHandler(GetTimeoutPolicy());
                
                // Registra serviços
                services.AddScoped<ExternalApiService>();
                services.AddScoped<PaymentService>();
                services.AddHostedService<PollyDemoHostedService>();
            })
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddConsole();
                logging.SetMinimumLevel(LogLevel.Information);
            })
            .Build();
        
        try
        {
            // Valida configurações na inicialização
            using var scope = host.Services.CreateScope();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
            
            logger.LogInformation("🔍 Validando configurações...");
            
            // Força a validação das configurações
            scope.ServiceProvider.GetRequiredService<IOptionsMonitor<ExternalApiSettings>>();
            scope.ServiceProvider.GetRequiredService<IOptionsMonitor<PaymentApiSettings>>();
            scope.ServiceProvider.GetRequiredService<IOptionsMonitor<CircuitBreakerSettings>>();
            
            logger.LogInformation("✅ Todas as configurações são válidas!");
            
            // Executa a aplicação
            await host.RunAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Erro durante execução: {ex.Message}");
            Environment.Exit(1);
        }
    }
    
    /// <summary>
    /// Política de retry básica para HttpClientFactory
    /// </summary>
    private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError() // HttpRequestException e HttpStatusCode >= 500
            .OrResult(msg => !msg.IsSuccessStatusCode)
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt => 
                    TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), // Exponential backoff
                onRetry: (outcome, timespan, retryCount, context) =>
                {
                    Console.WriteLine($"🔄 HttpClient Retry {retryCount} em {timespan.TotalSeconds}s");
                });
    }
    
    /// <summary>
    /// Política de circuit breaker para HttpClientFactory
    /// </summary>
    private static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: 3,
                durationOfBreak: TimeSpan.FromSeconds(30),
                onBreak: (exception, timespan) =>
                {
                    Console.WriteLine($"⚡ HttpClient Circuit Breaker ABERTO por {timespan.TotalSeconds}s");
                },
                onReset: () =>
                {
                    Console.WriteLine("⚡ HttpClient Circuit Breaker FECHADO");
                });
    }
    
    /// <summary>
    /// Política de retry avançada com jitter
    /// </summary>
    private static IAsyncPolicy<HttpResponseMessage> GetAdvancedRetryPolicy()
    {
        var jitterer = new Random();
        
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt =>
                {
                    // Exponential backoff with jitter
                    var exponentialDelay = TimeSpan.FromSeconds(Math.Pow(2, retryAttempt));
                    var jitter = TimeSpan.FromMilliseconds(jitterer.Next(0, 1000));
                    return exponentialDelay + jitter;
                },
                onRetry: (outcome, timespan, retryCount, context) =>
                {
                    Console.WriteLine($"🔄 Advanced Retry {retryCount} em {timespan.TotalMilliseconds}ms");
                });
    }
    
    /// <summary>
    /// Política de timeout
    /// </summary>
    private static IAsyncPolicy<HttpResponseMessage> GetTimeoutPolicy()
    {
        return Policy.TimeoutAsync<HttpResponseMessage>(
            timeout: TimeSpan.FromSeconds(10),
            onTimeoutAsync: (context, timespan, task) =>
            {
                Console.WriteLine($"⏰ HttpClient Timeout de {timespan.TotalSeconds}s atingido");
                return Task.CompletedTask;
            });
    }
}
