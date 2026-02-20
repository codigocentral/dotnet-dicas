using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Dica81_OptionsPattern.Configuration;

namespace Dica81_OptionsPattern.Services;

/// <summary>
/// Serviço que demonstra o uso de IOptions&lt;T&gt; (Singleton)
/// IOptions é um singleton e não reflete mudanças na configuração
/// </summary>
public class DatabaseService
{
    private readonly DatabaseSettings _settings;
    private readonly ILogger<DatabaseService> _logger;

    public DatabaseService(IOptions<DatabaseSettings> options, ILogger<DatabaseService> logger)
    {
        _settings = options.Value;
        _logger = logger;
    }

    public async Task<string> GetConnectionInfoAsync()
    {
        _logger.LogInformation("Obtendo informações de conexão do banco");
        
        // Simula uma operação async
        await Task.Delay(100);
        
        var maskedConnectionString = MaskConnectionString(_settings.ConnectionString);
        
        return $"""
               Configuração do Banco de Dados (IOptions - Singleton):
               - Connection: {maskedConnectionString}
               - Timeout: {_settings.CommandTimeout}s
               - Retry: {(_settings.EnableRetryOnFailure ? "Habilitado" : "Desabilitado")}
               - Max Retries: {_settings.MaxRetryCount}
               """;
    }

    private static string MaskConnectionString(string connectionString)
    {
        // Máscara básica para não expor credenciais
        if (connectionString.Length <= 20) return "***";
        return connectionString[..10] + "***" + connectionString[^7..];
    }
}

/// <summary>
/// Serviço que demonstra o uso de IOptionsSnapshot&lt;T&gt; (Scoped)
/// IOptionsSnapshot é recriado a cada request e reflete mudanças na configuração
/// </summary>
public class EmailService
{
    private readonly IOptionsSnapshot<EmailSettings> _optionsSnapshot;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptionsSnapshot<EmailSettings> optionsSnapshot, ILogger<EmailService> logger)
    {
        _optionsSnapshot = optionsSnapshot;
        _logger = logger;
    }

    public async Task<string> SendEmailAsync(string to, string subject, string body)
    {
        // Obtém a configuração atual (pode ter mudado desde a inicialização)
        var settings = _optionsSnapshot.Value;
        
        _logger.LogInformation("Enviando email usando configuração atual");
        
        // Simula envio de email
        await Task.Delay(200);
        
        return $"""
               Email Enviado (IOptionsSnapshot - Scoped):
               - Para: {to}
               - Assunto: {subject}
               - SMTP: {settings.SmtpServer}:{settings.Port}
               - SSL: {(settings.EnableSsl ? "Habilitado" : "Desabilitado")}
               - De: {settings.FromName} <{settings.FromEmail}>
               - Autenticação: {settings.Username}
               """;
    }
}

/// <summary>
/// Serviço que demonstra o uso de IOptionsMonitor&lt;T&gt; (Singleton com recarregamento)
/// IOptionsMonitor permite monitorar mudanças em tempo real na configuração
/// </summary>
public class ApiService : IDisposable
{
    private readonly IOptionsMonitor<ApiSettings> _optionsMonitor;
    private readonly ILogger<ApiService> _logger;
    private readonly IDisposable? _optionsChangeToken;

    public ApiService(IOptionsMonitor<ApiSettings> optionsMonitor, ILogger<ApiService> logger)
    {
        _optionsMonitor = optionsMonitor;
        _logger = logger;

        // Registra callback para mudanças na configuração
        _optionsChangeToken = _optionsMonitor.OnChange(OnApiSettingsChanged);
    }

    public async Task<string> CallApiAsync(string endpoint)
    {
        // Sempre obtém a configuração mais atual
        var settings = _optionsMonitor.CurrentValue;
        
        _logger.LogInformation("Chamando API com configuração atual");
        
        // Simula chamada de API
        await Task.Delay(150);
        
        var featuresStatus = $"""
                            Caching: {(settings.Features.EnableCaching ? "✅" : "❌")}
                            Logging: {(settings.Features.EnableLogging ? "✅" : "❌")}
                            Retries: {(settings.Features.EnableRetries ? "✅" : "❌")}
                            """;
        
        return $"""
               Chamada de API (IOptionsMonitor - Singleton com reload):
               - URL: {settings.BaseUrl}/{endpoint}
               - Timeout: {settings.TimeoutInSeconds}s
               - API Key: {MaskApiKey(settings.ApiKey)}
               - Features:
               {featuresStatus}
               - Hosts Permitidos: {string.Join(", ", settings.AllowedHosts)}
               """;
    }

    private void OnApiSettingsChanged(ApiSettings newSettings, string? name)
    {
        _logger.LogInformation("⚡ Configuração da API foi alterada! Novas configurações carregadas.");
        _logger.LogInformation("Nova BaseUrl: {BaseUrl}", newSettings.BaseUrl);
        _logger.LogInformation("Novo Timeout: {Timeout}s", newSettings.TimeoutInSeconds);
    }

    private static string MaskApiKey(string apiKey)
    {
        if (apiKey.Length <= 8) return "***";
        return apiKey[..4] + "***" + apiKey[^4..];
    }

    public void Dispose()
    {
        _optionsChangeToken?.Dispose();
    }
}

/// <summary>
/// Serviço que demonstra configuração com objetos complexos aninhados
/// </summary>
public class CacheService
{
    private readonly CacheSettings _settings;
    private readonly ILogger<CacheService> _logger;

    public CacheService(IOptions<CacheSettings> options, ILogger<CacheService> logger)
    {
        _settings = options.Value;
        _logger = logger;
    }

    public async Task<string> GetCacheInfoAsync()
    {
        _logger.LogInformation("Obtendo informações do cache");
        
        await Task.Delay(50);
        
        var enabledProviders = _settings.Providers
            .Where(p => p.Enabled)
            .OrderBy(p => p.Priority)
            .ToList();
        
        var providersInfo = enabledProviders
            .Select(p => $"   {p.Priority}. {p.Name}" + 
                        (p.ConnectionString != null ? $" ({MaskConnectionString(p.ConnectionString)})" : ""))
            .ToList();
        
        return $"""
               Configuração do Cache (Objetos Aninhados):
               - Expiração Padrão: {_settings.DefaultExpirationMinutes} minutos
               - Memória Máxima: {_settings.MaxMemoryMB} MB
               - Sliding Expiration: {(_settings.SlidingExpiration ? "Habilitado" : "Desabilitado")}
               - Providers Habilitados ({enabledProviders.Count}):
               {string.Join("\n", providersInfo)}
               """;
    }

    private static string MaskConnectionString(string connectionString)
    {
        if (connectionString.Length <= 10) return "***";
        return connectionString[..5] + "***" + connectionString[^5..];
    }
}
