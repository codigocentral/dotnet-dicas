using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Dica64.HealthChecks.HealthChecks;
using Dica64.HealthChecks.Services;

namespace Dica64.HealthChecks;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("=== Dica 64: Health Checks ===\n");

        // Criar o application builder
        var builder = Host.CreateApplicationBuilder(args);

        // Configurar serviços
        ConfigureServices(builder.Services, builder.Configuration);

        var host = builder.Build();

        try
        {
            // Executar demonstração
            var demoService = host.Services.GetRequiredService<HealthCheckDemoService>();
            await demoService.RunDemoAsync();

            // Gerar relatórios
            await GenerateHealthReportsAsync(host.Services);

            // Demonstrar uso programático dos health checks
            await DemonstrateHealthCheckUsageAsync(host.Services);
        }
        catch (Exception ex)
        {
            var logger = host.Services.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "Erro durante a execução da demonstração");
        }

        Console.WriteLine("\nPressione qualquer tecla para sair...");
        Console.ReadKey();
    }

    static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // === LOGGING ===
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Information);
        });

        // === CONFIGURAÇÕES TIPADAS ===
        services.Configure<MemoryHealthCheckOptions>(options =>
        {
            options.ThresholdMB = configuration.GetValue<long>("HealthChecks:MemoryThresholdMB", 500);
        });

        services.Configure<DiskSpaceHealthCheckOptions>(options =>
        {
            options.ThresholdMB = configuration.GetValue<long>("HealthChecks:DiskSpaceThresholdMB", 1000);
        });

        services.Configure<ExternalServiceHealthCheckOptions>(options =>
        {
            options.ServiceUrl = configuration.GetValue<string>("ExternalServices:ApiUrl") ?? "https://jsonplaceholder.typicode.com/posts/1";
            options.TimeoutMs = configuration.GetValue<int>("HealthChecks:ApiTimeout", 5000);
        });

        // === HTTP CLIENT ===
        services.AddHttpClient<ExternalServiceHealthCheck>(client =>
        {
            var timeout = configuration.GetValue<int>("HealthChecks:ApiTimeout", 5000);
            client.Timeout = TimeSpan.FromMilliseconds(timeout);
        });

        // === HEALTH CHECKS ===
        services.AddHealthChecks()
            
            // Health checks customizados
            .AddCheck<MemoryHealthCheck>(
                name: "memory", 
                tags: new[] { "system", "critical" })
            
            .AddCheck<DiskSpaceHealthCheck>(
                name: "disk-space", 
                tags: new[] { "system", "infrastructure" })
            
            .AddCheck<ApplicationDependenciesHealthCheck>(
                name: "app-dependencies", 
                tags: new[] { "application", "critical" })
            
            .AddCheck<ExternalServiceHealthCheck>(
                name: "external-api", 
                tags: new[] { "external", "api" })

            // Simular múltiplos bancos de dados
            .AddCheck("primary-database", () =>
            {
                // Simular verificação de conexão com banco primário
                var isHealthy = Random.Shared.Next(1, 11) > 1; // 90% de chance de estar saudável
                var message = isHealthy ? "Banco primário conectado" : "Falha na conexão com banco primário";
                return isHealthy ? HealthCheckResult.Healthy(message) : HealthCheckResult.Unhealthy(message);
            }, tags: new[] { "database", "critical" })
            
            .AddCheck("secondary-database", () => 
            {
                // Simular verificação de conexão com banco secundário
                var isHealthy = Random.Shared.Next(1, 11) > 2; // 80% de chance de estar saudável
                var message = isHealthy ? "Banco secundário conectado" : "Falha na conexão com banco secundário";
                return isHealthy ? HealthCheckResult.Healthy(message) : HealthCheckResult.Degraded(message);
            }, tags: new[] { "database", "infrastructure" })

            // Health checks usando bibliotecas externas (simulados)
            .AddCheck("simulated-redis", () =>
            {
                // Simular verificação do Redis
                var isHealthy = Random.Shared.Next(1, 11) > 2; // 80% de chance de estar saudável
                var message = isHealthy ? "Redis está funcionando" : "Redis não está respondendo";
                var status = isHealthy ? HealthCheckResult.Healthy(message) : HealthCheckResult.Unhealthy(message);
                return status;
            }, new[] { "cache", "infrastructure" })

            .AddCheck("simulated-message-queue", () =>
            {
                // Simular verificação de fila de mensagens
                var responseTime = Random.Shared.Next(50, 300);
                var data = new Dictionary<string, object> { { "ResponseTimeMs", responseTime } };
                
                var status = responseTime < 200 
                    ? HealthCheckResult.Healthy($"Message queue OK ({responseTime}ms)", data)
                    : HealthCheckResult.Degraded($"Message queue slow ({responseTime}ms)", data: data);
                
                return status;
            }, new[] { "messaging", "infrastructure" })

            // Health check de exemplo que falha ocasionalmente
            .AddCheck("flaky-service", () =>
            {
                var isHealthy = Random.Shared.Next(1, 6) > 1; // 80% de chance de estar saudável
                var message = isHealthy ? "Serviço instável OK" : "Serviço instável falhando";
                var status = isHealthy ? HealthCheckResult.Healthy(message) : HealthCheckResult.Unhealthy(message);
                return status;
            }, new[] { "external", "flaky" });

        // === SERVIÇOS DA APLICAÇÃO ===
        services.AddSingleton<HealthCheckDemoService>();
        services.AddSingleton<HealthReportService>();
    }

    static async Task GenerateHealthReportsAsync(IServiceProvider serviceProvider)
    {
        var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
        var reportService = serviceProvider.GetRequiredService<HealthReportService>();

        logger.LogInformation("\n=== Gerando Relatórios de Saúde ===");

        try
        {
            // Gerar relatório em texto
            logger.LogInformation("Gerando relatório em texto...");
            var textReport = await reportService.GenerateTextReportAsync();
            logger.LogInformation("Relatório em texto gerado com sucesso ({Length} caracteres)", textReport.Length);

            // Gerar relatório em JSON
            logger.LogInformation("Gerando relatório em JSON...");
            var jsonReport = await reportService.GenerateJsonReportAsync();
            logger.LogInformation("Relatório em JSON gerado com sucesso ({Length} caracteres)", jsonReport.Length);

            // Exibir parte do relatório em texto
            logger.LogInformation("\n=== Prévia do Relatório em Texto ===");
            var lines = textReport.Split('\n');
            var previewLines = lines.Take(Math.Min(15, lines.Length));
            foreach (var line in previewLines)
            {
                logger.LogInformation(line.TrimEnd());
            }
            if (lines.Length > 15)
            {
                logger.LogInformation("... (relatório truncado)");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao gerar relatórios de saúde");
        }
    }

    static async Task DemonstrateHealthCheckUsageAsync(IServiceProvider serviceProvider)
    {
        var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
        var healthCheckService = serviceProvider.GetRequiredService<HealthCheckService>();

        logger.LogInformation("\n=== Demonstrando Uso Programático dos Health Checks ===");

        try
        {
            // Verificar saúde geral da aplicação
            logger.LogInformation("1. Verificando saúde geral da aplicação...");
            var overallHealth = await healthCheckService.CheckHealthAsync();
            logger.LogInformation("Status geral: {Status} em {Duration}ms", 
                overallHealth.Status, overallHealth.TotalDuration.TotalMilliseconds);

            // Verificar apenas sistemas críticos
            logger.LogInformation("\n2. Verificando apenas sistemas críticos...");
            var criticalHealth = await healthCheckService.CheckHealthAsync(check => 
                check.Tags.Contains("critical"));
            logger.LogInformation("Status crítico: {Status} ({CheckCount} verificações)", 
                criticalHealth.Status, criticalHealth.Entries.Count);

            // Verificar sistemas externos
            logger.LogInformation("\n3. Verificando sistemas externos...");
            var externalHealth = await healthCheckService.CheckHealthAsync(check => 
                check.Tags.Contains("external"));
            logger.LogInformation("Status externo: {Status} ({CheckCount} verificações)", 
                externalHealth.Status, externalHealth.Entries.Count);

            // Simular decisão baseada em health checks
            logger.LogInformation("\n4. Simulando decisões baseadas em health checks...");
            
            if (overallHealth.Status == HealthStatus.Healthy)
            {
                logger.LogInformation("✓ Sistema saudável - Todas as operações podem prosseguir");
            }
            else if (overallHealth.Status == HealthStatus.Degraded)
            {
                logger.LogWarning("⚠ Sistema degradado - Algumas funcionalidades podem estar limitadas");
                
                // Verificar se sistemas críticos estão OK
                if (criticalHealth.Status == HealthStatus.Healthy)
                {
                    logger.LogInformation("✓ Sistemas críticos OK - Operações essenciais podem continuar");
                }
                else
                {
                    logger.LogError("✗ Sistemas críticos afetados - Operações limitadas");
                }
            }
            else
            {
                logger.LogError("✗ Sistema não saudável - Operações podem estar comprometidas");
            }

            // Exibir estatísticas detalhadas
            logger.LogInformation("\n5. Estatísticas detalhadas:");
            var healthyCount = overallHealth.Entries.Count(e => e.Value.Status == HealthStatus.Healthy);
            var degradedCount = overallHealth.Entries.Count(e => e.Value.Status == HealthStatus.Degraded);
            var unhealthyCount = overallHealth.Entries.Count(e => e.Value.Status == HealthStatus.Unhealthy);

            logger.LogInformation("- Total de verificações: {Total}", overallHealth.Entries.Count);
            logger.LogInformation("- Saudáveis: {Healthy} ({HealthyPercent:F1}%)", 
                healthyCount, (double)healthyCount / overallHealth.Entries.Count * 100);
            logger.LogInformation("- Degradados: {Degraded} ({DegradedPercent:F1}%)", 
                degradedCount, (double)degradedCount / overallHealth.Entries.Count * 100);
            logger.LogInformation("- Não saudáveis: {Unhealthy} ({UnhealthyPercent:F1}%)", 
                unhealthyCount, (double)unhealthyCount / overallHealth.Entries.Count * 100);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro durante demonstração de uso programático");
        }
    }
}
