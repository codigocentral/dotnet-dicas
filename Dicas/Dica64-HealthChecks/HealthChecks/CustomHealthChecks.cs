using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;
using System.Text.Json;

namespace Dica64.HealthChecks.HealthChecks;

// Health check personalizado para verificar uso de memória
public class MemoryHealthCheck : IHealthCheck
{
    private readonly ILogger<MemoryHealthCheck> _logger;
    private readonly MemoryHealthCheckOptions _options;

    public MemoryHealthCheck(ILogger<MemoryHealthCheck> logger, IOptions<MemoryHealthCheckOptions> options)
    {
        _logger = logger;
        _options = options.Value;
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var process = Process.GetCurrentProcess();
            var memoryUsedMB = process.WorkingSet64 / 1024 / 1024;

            _logger.LogDebug("Verificação de memória: {MemoryUsedMB}MB usado, limite: {ThresholdMB}MB", 
                memoryUsedMB, _options.ThresholdMB);

            var data = new Dictionary<string, object>
            {
                { "MemoryUsedMB", memoryUsedMB },
                { "ThresholdMB", _options.ThresholdMB },
                { "ProcessId", process.Id },
                { "ProcessName", process.ProcessName }
            };

            if (memoryUsedMB > _options.ThresholdMB)
            {
                var message = $"Uso de memória muito alto: {memoryUsedMB}MB (limite: {_options.ThresholdMB}MB)";
                _logger.LogWarning(message);
                return Task.FromResult(HealthCheckResult.Degraded(message, data: data));
            }

            if (memoryUsedMB > _options.ThresholdMB * 0.8)
            {
                var message = $"Uso de memória elevado: {memoryUsedMB}MB (limite: {_options.ThresholdMB}MB)";
                _logger.LogWarning(message);
                return Task.FromResult(HealthCheckResult.Degraded(message, data: data));
            }

            var successMessage = $"Uso de memória normal: {memoryUsedMB}MB";
            return Task.FromResult(HealthCheckResult.Healthy(successMessage, data));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao verificar uso de memória");
            return Task.FromResult(HealthCheckResult.Unhealthy("Erro ao verificar memória", ex));
        }
    }
}

public class MemoryHealthCheckOptions
{
    public long ThresholdMB { get; set; } = 500;
}

// Health check para verificar espaço em disco
public class DiskSpaceHealthCheck : IHealthCheck
{
    private readonly ILogger<DiskSpaceHealthCheck> _logger;
    private readonly DiskSpaceHealthCheckOptions _options;

    public DiskSpaceHealthCheck(ILogger<DiskSpaceHealthCheck> logger, IOptions<DiskSpaceHealthCheckOptions> options)
    {
        _logger = logger;
        _options = options.Value;
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var drives = DriveInfo.GetDrives().Where(d => d.IsReady && d.DriveType == DriveType.Fixed);
            var results = new List<object>();
            var overallHealthy = true;
            var messages = new List<string>();

            foreach (var drive in drives)
            {
                var freeSpaceMB = drive.AvailableFreeSpace / 1024 / 1024;
                var totalSpaceMB = drive.TotalSize / 1024 / 1024;
                var usedPercentage = ((double)(totalSpaceMB - freeSpaceMB) / totalSpaceMB) * 100;

                var driveData = new
                {
                    Drive = drive.Name,
                    FreeSpaceMB = freeSpaceMB,
                    TotalSpaceMB = totalSpaceMB,
                    UsedPercentage = Math.Round(usedPercentage, 2),
                    ThresholdMB = _options.ThresholdMB
                };

                results.Add(driveData);

                _logger.LogDebug("Drive {Drive}: {FreeSpaceMB}MB livres de {TotalSpaceMB}MB ({UsedPercentage}%)", 
                    drive.Name, freeSpaceMB, totalSpaceMB, usedPercentage);

                if (freeSpaceMB < _options.ThresholdMB)
                {
                    overallHealthy = false;
                    messages.Add($"Drive {drive.Name}: Pouco espaço livre ({freeSpaceMB}MB)");
                }
                else if (usedPercentage > 90)
                {
                    messages.Add($"Drive {drive.Name}: Uso alto ({usedPercentage:F1}%)");
                }
            }

            var data = new Dictionary<string, object> { { "Drives", results } };

            if (!overallHealthy)
            {
                var message = string.Join("; ", messages);
                _logger.LogWarning("Problemas de espaço em disco detectados: {Message}", message);
                return Task.FromResult(HealthCheckResult.Unhealthy(message, data: data));
            }

            if (messages.Any())
            {
                var message = string.Join("; ", messages);
                _logger.LogWarning("Avisos de espaço em disco: {Message}", message);
                return Task.FromResult(HealthCheckResult.Degraded(message, data: data));
            }

            return Task.FromResult(HealthCheckResult.Healthy("Espaço em disco adequado", data));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao verificar espaço em disco");
            return Task.FromResult(HealthCheckResult.Unhealthy("Erro ao verificar espaço em disco", ex));
        }
    }
}

public class DiskSpaceHealthCheckOptions
{
    public long ThresholdMB { get; set; } = 1000;
}

// Health check para verificar serviços externos
public class ExternalServiceHealthCheck : IHealthCheck
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ExternalServiceHealthCheck> _logger;
    private readonly ExternalServiceHealthCheckOptions _options;

    public ExternalServiceHealthCheck(
        HttpClient httpClient, 
        ILogger<ExternalServiceHealthCheck> logger,
        IOptions<ExternalServiceHealthCheckOptions> options)
    {
        _httpClient = httpClient;
        _logger = logger;
        _options = options.Value;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Verificando serviço externo: {ServiceUrl}", _options.ServiceUrl);

            var stopwatch = Stopwatch.StartNew();
            var response = await _httpClient.GetAsync(_options.ServiceUrl, cancellationToken);
            stopwatch.Stop();

            var data = new Dictionary<string, object>
            {
                { "ServiceUrl", _options.ServiceUrl },
                { "StatusCode", (int)response.StatusCode },
                { "ResponseTimeMs", stopwatch.ElapsedMilliseconds },
                { "TimeoutMs", _options.TimeoutMs }
            };

            if (response.IsSuccessStatusCode)
            {
                var responseTime = stopwatch.ElapsedMilliseconds;
                if (responseTime > _options.TimeoutMs)
                {
                    var message = $"Serviço externo respondeu lentamente: {responseTime}ms (limite: {_options.TimeoutMs}ms)";
                    _logger.LogWarning(message);
                    return HealthCheckResult.Degraded(message, data: data);
                }

                var successMessage = $"Serviço externo OK: {response.StatusCode} em {responseTime}ms";
                return HealthCheckResult.Healthy(successMessage, data);
            }
            else
            {
                var message = $"Serviço externo retornou erro: {response.StatusCode}";
                _logger.LogError(message);
                return HealthCheckResult.Unhealthy(message, data: data);
            }
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            var message = $"Timeout ao acessar serviço externo: {_options.ServiceUrl}";
            _logger.LogError(ex, message);
            return HealthCheckResult.Unhealthy(message, ex);
        }
        catch (Exception ex)
        {
            var message = $"Erro ao verificar serviço externo: {_options.ServiceUrl}";
            _logger.LogError(ex, message);
            return HealthCheckResult.Unhealthy(message, ex);
        }
    }
}

public class ExternalServiceHealthCheckOptions
{
    public string ServiceUrl { get; set; } = string.Empty;
    public int TimeoutMs { get; set; } = 5000;
}

// Health check customizado para verificar dependências de aplicação
public class ApplicationDependenciesHealthCheck : IHealthCheck
{
    private readonly ILogger<ApplicationDependenciesHealthCheck> _logger;
    private readonly IServiceProvider _serviceProvider;

    public ApplicationDependenciesHealthCheck(
        ILogger<ApplicationDependenciesHealthCheck> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var checks = new List<object>();
            var isHealthy = true;
            var messages = new List<string>();

            // Verificar se serviços críticos estão registrados
            var criticalServices = new[]
            {
                typeof(IServiceProvider),
                typeof(IConfiguration),
                typeof(IHostEnvironment)
            };

            foreach (var serviceType in criticalServices)
            {
                try
                {
                    var service = _serviceProvider.GetService(serviceType);
                    var available = service != null;
                    
                    checks.Add(new
                    {
                        Service = serviceType.Name,
                        Available = available,
                        Status = available ? "OK" : "Missing"
                    });

                    if (!available)
                    {
                        isHealthy = false;
                        messages.Add($"Serviço crítico não disponível: {serviceType.Name}");
                    }

                    _logger.LogDebug("Serviço {ServiceType}: {Status}", serviceType.Name, available ? "OK" : "Missing");
                }
                catch (Exception ex)
                {
                    isHealthy = false;
                    messages.Add($"Erro ao verificar serviço {serviceType.Name}: {ex.Message}");
                    _logger.LogError(ex, "Erro ao verificar serviço {ServiceType}", serviceType.Name);
                }
            }

            // Verificar configurações essenciais
            try
            {
                var configuration = _serviceProvider.GetService(typeof(Microsoft.Extensions.Configuration.IConfiguration)) as Microsoft.Extensions.Configuration.IConfiguration;
                var hasConfiguration = configuration != null;
                
                checks.Add(new
                {
                    Service = "Configuration",
                    Available = hasConfiguration,
                    Status = hasConfiguration ? "OK" : "Missing"
                });

                if (!hasConfiguration)
                {
                    isHealthy = false;
                    messages.Add("Sistema de configuração não disponível");
                }
            }
            catch (Exception ex)
            {
                isHealthy = false;
                messages.Add($"Erro ao verificar configuração: {ex.Message}");
                _logger.LogError(ex, "Erro ao verificar configuração");
            }

            var data = new Dictionary<string, object> { { "DependencyChecks", checks } };

            if (!isHealthy)
            {
                var message = string.Join("; ", messages);
                return Task.FromResult(HealthCheckResult.Unhealthy(message, data: data));
            }

            return Task.FromResult(HealthCheckResult.Healthy("Todas as dependências estão disponíveis", data));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao verificar dependências da aplicação");
            return Task.FromResult(HealthCheckResult.Unhealthy("Erro ao verificar dependências", ex));
        }
    }
}

// Health check que simula verificação de banco de dados
public class DatabaseConnectionHealthCheck : IHealthCheck
{
    private readonly ILogger<DatabaseConnectionHealthCheck> _logger;
    private readonly DatabaseHealthCheckOptions _options;

    public DatabaseConnectionHealthCheck(
        ILogger<DatabaseConnectionHealthCheck> logger,
        IOptions<DatabaseHealthCheckOptions> options)
    {
        _logger = logger;
        _options = options.Value;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Verificando conexão com banco de dados: {DatabaseName}", _options.DatabaseName);

            // Simular verificação de banco de dados
            var stopwatch = Stopwatch.StartNew();
            await Task.Delay(Random.Shared.Next(50, 200), cancellationToken); // Simular latência
            stopwatch.Stop();

            var connectionTime = stopwatch.ElapsedMilliseconds;
            var data = new Dictionary<string, object>
            {
                { "DatabaseName", _options.DatabaseName },
                { "ConnectionString", MaskConnectionString(_options.ConnectionString) },
                { "ConnectionTimeMs", connectionTime },
                { "TimeoutMs", _options.TimeoutMs }
            };

            // Simular falha ocasional (5% de chance)
            if (Random.Shared.Next(1, 21) == 1)
            {
                var message = $"Falha simulada na conexão com {_options.DatabaseName}";
                _logger.LogError(message);
                return HealthCheckResult.Unhealthy(message, data: data);
            }

            if (connectionTime > _options.TimeoutMs)
            {
                var message = $"Conexão com banco lenta: {connectionTime}ms (limite: {_options.TimeoutMs}ms)";
                _logger.LogWarning(message);
                return HealthCheckResult.Degraded(message, data: data);
            }

            var successMessage = $"Banco {_options.DatabaseName} conectado em {connectionTime}ms";
            return HealthCheckResult.Healthy(successMessage, data);
        }
        catch (Exception ex)
        {
            var message = $"Erro ao conectar com banco {_options.DatabaseName}";
            _logger.LogError(ex, message);
            return HealthCheckResult.Unhealthy(message, ex);
        }
    }

    private static string MaskConnectionString(string connectionString)
    {
        // Mascarar informações sensíveis na connection string
        if (string.IsNullOrEmpty(connectionString))
            return "***";

        var parts = connectionString.Split(';');
        var maskedParts = parts.Select(part =>
        {
            if (part.Contains("Password", StringComparison.OrdinalIgnoreCase) ||
                part.Contains("Pwd", StringComparison.OrdinalIgnoreCase))
            {
                var index = part.IndexOf('=');
                return index > 0 ? $"{part[..index]}=***" : part;
            }
            return part;
        });

        return string.Join(";", maskedParts);
    }
}

public class DatabaseHealthCheckOptions
{
    public string DatabaseName { get; set; } = "Default";
    public string ConnectionString { get; set; } = string.Empty;
    public int TimeoutMs { get; set; } = 5000;
}
