using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Dica64.HealthChecks.Services;

// Serviço para demonstrar o uso dos health checks
public class HealthCheckDemoService
{
    private readonly HealthCheckService _healthCheckService;
    private readonly ILogger<HealthCheckDemoService> _logger;

    public HealthCheckDemoService(HealthCheckService healthCheckService, ILogger<HealthCheckDemoService> logger)
    {
        _healthCheckService = healthCheckService;
        _logger = logger;
    }

    public async Task RunDemoAsync()
    {
        _logger.LogInformation("=== Iniciando Demonstração de Health Checks ===");

        try
        {
            await DemonstrateBasicHealthCheckAsync();
            await DemonstrateDetailedHealthReportAsync();
            await DemonstrateFilteredHealthChecksAsync();
            await DemonstrateContinuousMonitoringAsync();

            _logger.LogInformation("=== Demonstração Concluída com Sucesso ===");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro durante a demonstração de health checks");
            throw;
        }
    }

    private async Task DemonstrateBasicHealthCheckAsync()
    {
        _logger.LogInformation("\n1. === Verificação Básica de Saúde ===");

        var healthReport = await _healthCheckService.CheckHealthAsync();
        
        _logger.LogInformation("Status geral da aplicação: {OverallStatus}", healthReport.Status);
        _logger.LogInformation("Tempo total de verificação: {TotalTime}ms", healthReport.TotalDuration.TotalMilliseconds);

        foreach (var entry in healthReport.Entries)
        {
            _logger.LogInformation("- {CheckName}: {Status} ({Duration}ms)", 
                entry.Key, entry.Value.Status, entry.Value.Duration.TotalMilliseconds);

            if (!string.IsNullOrEmpty(entry.Value.Description))
            {
                _logger.LogInformation("  Descrição: {Description}", entry.Value.Description);
            }

            if (entry.Value.Exception != null)
            {
                _logger.LogWarning("  Erro: {Error}", entry.Value.Exception.Message);
            }
        }
    }

    private async Task DemonstrateDetailedHealthReportAsync()
    {
        _logger.LogInformation("\n2. === Relatório Detalhado de Saúde ===");

        var healthReport = await _healthCheckService.CheckHealthAsync();
        
        foreach (var entry in healthReport.Entries)
        {
            _logger.LogInformation("\n=== Health Check: {CheckName} ===", entry.Key);
            _logger.LogInformation("Status: {Status}", entry.Value.Status);
            _logger.LogInformation("Duração: {Duration}ms", entry.Value.Duration.TotalMilliseconds);
            
            if (!string.IsNullOrEmpty(entry.Value.Description))
            {
                _logger.LogInformation("Descrição: {Description}", entry.Value.Description);
            }

            // Exibir dados adicionais se disponíveis
            if (entry.Value.Data.Any())
            {
                _logger.LogInformation("Dados adicionais:");
                foreach (var data in entry.Value.Data)
                {
                    var valueStr = data.Value switch
                    {
                        string s => s,
                        null => "null",
                        _ => JsonSerializer.Serialize(data.Value, new JsonSerializerOptions { WriteIndented = false })
                    };
                    _logger.LogInformation("  {Key}: {Value}", data.Key, valueStr);
                }
            }

            // Exibir tags se disponíveis
            if (entry.Value.Tags.Any())
            {
                _logger.LogInformation("Tags: {Tags}", string.Join(", ", entry.Value.Tags));
            }
        }
    }

    private async Task DemonstrateFilteredHealthChecksAsync()
    {
        _logger.LogInformation("\n3. === Health Checks Filtrados ===");

        // Verificar apenas health checks críticos
        _logger.LogInformation("Verificando apenas health checks críticos...");
        var criticalHealthReport = await _healthCheckService.CheckHealthAsync(check => 
            check.Tags.Contains("critical"));

        LogHealthReport("Critical", criticalHealthReport);

        // Verificar apenas health checks de infraestrutura
        _logger.LogInformation("\nVerificando apenas health checks de infraestrutura...");
        var infrastructureHealthReport = await _healthCheckService.CheckHealthAsync(check => 
            check.Tags.Contains("infrastructure"));

        LogHealthReport("Infrastructure", infrastructureHealthReport);

        // Verificar apenas health checks externos
        _logger.LogInformation("\nVerificando apenas health checks externos...");
        var externalHealthReport = await _healthCheckService.CheckHealthAsync(check => 
            check.Tags.Contains("external"));

        LogHealthReport("External", externalHealthReport);
    }

    private async Task DemonstrateContinuousMonitoringAsync()
    {
        _logger.LogInformation("\n4. === Monitoramento Contínuo ===");

        var monitoringDuration = TimeSpan.FromSeconds(10);
        var checkInterval = TimeSpan.FromSeconds(2);
        var endTime = DateTime.Now.Add(monitoringDuration);

        _logger.LogInformation("Iniciando monitoramento contínuo por {Duration} segundos...", 
            monitoringDuration.TotalSeconds);

        var checkCount = 0;
        var healthyCount = 0;
        var degradedCount = 0;
        var unhealthyCount = 0;

        while (DateTime.Now < endTime)
        {
            checkCount++;
            var healthReport = await _healthCheckService.CheckHealthAsync();

            switch (healthReport.Status)
            {
                case HealthStatus.Healthy:
                    healthyCount++;
                    _logger.LogDebug("Check #{CheckCount}: Saudável ({Duration}ms)", 
                        checkCount, healthReport.TotalDuration.TotalMilliseconds);
                    break;
                case HealthStatus.Degraded:
                    degradedCount++;
                    _logger.LogWarning("Check #{CheckCount}: Degradado ({Duration}ms)", 
                        checkCount, healthReport.TotalDuration.TotalMilliseconds);
                    break;
                case HealthStatus.Unhealthy:
                    unhealthyCount++;
                    _logger.LogError("Check #{CheckCount}: Não saudável ({Duration}ms)", 
                        checkCount, healthReport.TotalDuration.TotalMilliseconds);
                    break;
            }

            await Task.Delay(checkInterval);
        }

        _logger.LogInformation("\n=== Estatísticas do Monitoramento ===");
        _logger.LogInformation("Total de verificações: {TotalChecks}", checkCount);
        _logger.LogInformation("Saudável: {HealthyCount} ({HealthyPercentage:F1}%)", 
            healthyCount, (double)healthyCount / checkCount * 100);
        _logger.LogInformation("Degradado: {DegradedCount} ({DegradedPercentage:F1}%)", 
            degradedCount, (double)degradedCount / checkCount * 100);
        _logger.LogInformation("Não saudável: {UnhealthyCount} ({UnhealthyPercentage:F1}%)", 
            unhealthyCount, (double)unhealthyCount / checkCount * 100);
    }

    private void LogHealthReport(string category, HealthReport healthReport)
    {
        _logger.LogInformation("{Category} Health Status: {Status} ({Duration}ms)", 
            category, healthReport.Status, healthReport.TotalDuration.TotalMilliseconds);

        if (healthReport.Entries.Any())
        {
            foreach (var entry in healthReport.Entries)
            {
                _logger.LogInformation("  - {CheckName}: {Status}", entry.Key, entry.Value.Status);
            }
        }
        else
        {
            _logger.LogInformation("  Nenhum health check encontrado para esta categoria");
        }
    }
}

// Serviço para gerar relatórios de health check
public class HealthReportService
{
    private readonly HealthCheckService _healthCheckService;
    private readonly ILogger<HealthReportService> _logger;

    public HealthReportService(HealthCheckService healthCheckService, ILogger<HealthReportService> logger)
    {
        _healthCheckService = healthCheckService;
        _logger = logger;
    }

    public async Task<HealthReportSummary> GenerateHealthReportSummaryAsync()
    {
        var healthReport = await _healthCheckService.CheckHealthAsync();
        
        return new HealthReportSummary
        {
            OverallStatus = healthReport.Status,
            TotalDurationMs = (int)healthReport.TotalDuration.TotalMilliseconds,
            CheckCount = healthReport.Entries.Count,
            HealthyCount = healthReport.Entries.Count(e => e.Value.Status == HealthStatus.Healthy),
            DegradedCount = healthReport.Entries.Count(e => e.Value.Status == HealthStatus.Degraded),
            UnhealthyCount = healthReport.Entries.Count(e => e.Value.Status == HealthStatus.Unhealthy),
            Timestamp = DateTime.UtcNow,
            Checks = healthReport.Entries.Select(e => new HealthCheckSummary
            {
                Name = e.Key,
                Status = e.Value.Status,
                Description = e.Value.Description ?? string.Empty,
                DurationMs = (int)e.Value.Duration.TotalMilliseconds,
                Tags = e.Value.Tags.ToList(),
                Data = e.Value.Data.ToDictionary(d => d.Key, d => d.Value?.ToString() ?? string.Empty),
                Exception = e.Value.Exception?.Message
            }).ToList()
        };
    }

    public async Task<string> GenerateJsonReportAsync()
    {
        var summary = await GenerateHealthReportSummaryAsync();
        
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        return JsonSerializer.Serialize(summary, options);
    }

    public async Task<string> GenerateTextReportAsync()
    {
        var summary = await GenerateHealthReportSummaryAsync();
        
        var report = new System.Text.StringBuilder();
        
        report.AppendLine("=== RELATÓRIO DE SAÚDE DA APLICAÇÃO ===");
        report.AppendLine($"Timestamp: {summary.Timestamp:yyyy-MM-dd HH:mm:ss} UTC");
        report.AppendLine($"Status Geral: {summary.OverallStatus}");
        report.AppendLine($"Duração Total: {summary.TotalDurationMs}ms");
        report.AppendLine($"Total de Verificações: {summary.CheckCount}");
        report.AppendLine($"Saudáveis: {summary.HealthyCount}");
        report.AppendLine($"Degradados: {summary.DegradedCount}");
        report.AppendLine($"Não Saudáveis: {summary.UnhealthyCount}");
        report.AppendLine();

        foreach (var check in summary.Checks)
        {
            report.AppendLine($"=== {check.Name} ===");
            report.AppendLine($"Status: {check.Status}");
            report.AppendLine($"Duração: {check.DurationMs}ms");
            
            if (!string.IsNullOrEmpty(check.Description))
            {
                report.AppendLine($"Descrição: {check.Description}");
            }

            if (check.Tags.Any())
            {
                report.AppendLine($"Tags: {string.Join(", ", check.Tags)}");
            }

            if (check.Data.Any())
            {
                report.AppendLine("Dados:");
                foreach (var data in check.Data)
                {
                    report.AppendLine($"  {data.Key}: {data.Value}");
                }
            }

            if (!string.IsNullOrEmpty(check.Exception))
            {
                report.AppendLine($"Erro: {check.Exception}");
            }

            report.AppendLine();
        }

        return report.ToString();
    }
}

// Modelos para relatórios
public class HealthReportSummary
{
    public HealthStatus OverallStatus { get; set; }
    public int TotalDurationMs { get; set; }
    public int CheckCount { get; set; }
    public int HealthyCount { get; set; }
    public int DegradedCount { get; set; }
    public int UnhealthyCount { get; set; }
    public DateTime Timestamp { get; set; }
    public List<HealthCheckSummary> Checks { get; set; } = new();
}

public class HealthCheckSummary
{
    public string Name { get; set; } = string.Empty;
    public HealthStatus Status { get; set; }
    public string Description { get; set; } = string.Empty;
    public int DurationMs { get; set; }
    public List<string> Tags { get; set; } = new();
    public Dictionary<string, string> Data { get; set; } = new();
    public string? Exception { get; set; }
}
