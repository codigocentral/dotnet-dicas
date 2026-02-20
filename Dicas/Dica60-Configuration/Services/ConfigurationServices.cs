using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Dica60.Configuration.Models;

namespace Dica60.Configuration.Services;

// Demonstra o uso do IOptions<T>
public class DatabaseService
{
    private readonly DatabaseSettings _settings;
    private readonly ILogger<DatabaseService> _logger;

    public DatabaseService(IOptions<DatabaseSettings> options, ILogger<DatabaseService> logger)
    {
        _settings = options.Value;
        _logger = logger;
    }

    public void Connect()
    {
        _logger.LogInformation("Conectando ao banco de dados usando o provider: {Provider}", _settings.Provider);
        _logger.LogInformation("Timeout de conexão: {ConnectionTimeout}s", _settings.ConnectionTimeout);
        _logger.LogInformation("Retry habilitado: {EnableRetryOnFailure}", _settings.EnableRetryOnFailure);
        
        if (_settings.EnableRetryOnFailure)
        {
            _logger.LogInformation("Máximo de tentativas: {MaxRetryCount}", _settings.MaxRetryCount);
            _logger.LogInformation("Delay máximo entre tentativas: {MaxRetryDelay}", _settings.MaxRetryDelay);
        }
    }
}

// Demonstra o uso do IOptionsMonitor<T> para atualizações em tempo real
public class CacheService
{
    private readonly IOptionsMonitor<CacheSettings> _optionsMonitor;
    private readonly ILogger<CacheService> _logger;
    private readonly IDisposable? _optionsChangeToken;

    public CacheService(IOptionsMonitor<CacheSettings> optionsMonitor, ILogger<CacheService> logger)
    {
        _optionsMonitor = optionsMonitor;
        _logger = logger;

        // Registra callback para mudanças na configuração
        _optionsChangeToken = _optionsMonitor.OnChange(OnCacheSettingsChanged);
    }

    public void Initialize()
    {
        var settings = _optionsMonitor.CurrentValue;
        _logger.LogInformation("Inicializando cache com expiração padrão: {DefaultExpiration} minutos", 
            settings.DefaultExpirationMinutes);
        _logger.LogInformation("Tamanho máximo da memória: {MaxMemorySize}", settings.MaxMemorySize);
        _logger.LogInformation("Percentual de compactação: {CompactionPercentage:P}", settings.CompactionPercentage);
    }

    private void OnCacheSettingsChanged(CacheSettings newSettings)
    {
        _logger.LogInformation("Configurações de cache foram atualizadas!");
        _logger.LogInformation("Nova expiração padrão: {DefaultExpiration} minutos", 
            newSettings.DefaultExpirationMinutes);
        
        // Aqui você poderia implementar a lógica para reconfigurar o cache
        ReconfigureCache(newSettings);
    }

    private void ReconfigureCache(CacheSettings newSettings)
    {
        _logger.LogInformation("Reconfigurando cache com as novas configurações...");
        // Implementação da reconfiguração do cache
    }

    public void Dispose()
    {
        _optionsChangeToken?.Dispose();
    }
}

// Demonstra o uso do IOptionsSnapshot<T> para configurações por request/scope
public class EmailService
{
    private readonly EmailSettings _settings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptionsSnapshot<EmailSettings> optionsSnapshot, ILogger<EmailService> logger)
    {
        _settings = optionsSnapshot.Value;
        _logger = logger;
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        _logger.LogInformation("Enviando email para: {To}", to);
        _logger.LogInformation("Usando servidor SMTP: {SmtpServer}:{Port}", _settings.SmtpServer, _settings.Port);
        _logger.LogInformation("SSL habilitado: {EnableSsl}", _settings.EnableSsl);
        _logger.LogInformation("Remetente: {DisplayName}", _settings.DisplayName);

        // Simulação do envio de email
        await Task.Delay(100);
        _logger.LogInformation("Email enviado com sucesso!");
    }
}

// Serviço que demonstra o uso de múltiplas configurações
public class ApiService
{
    private readonly ApiSettings _apiSettings;
    private readonly SecuritySettings _securitySettings;
    private readonly FeatureFlags _featureFlags;
    private readonly ILogger<ApiService> _logger;

    public ApiService(
        IOptions<ApiSettings> apiOptions,
        IOptions<SecuritySettings> securityOptions,
        IOptions<FeatureFlags> featureFlagsOptions,
        ILogger<ApiService> logger)
    {
        _apiSettings = apiOptions.Value;
        _securitySettings = securityOptions.Value;
        _featureFlags = featureFlagsOptions.Value;
        _logger = logger;
    }

    public async Task<string> CallApiAsync(string endpoint)
    {
        _logger.LogInformation("Chamando API: {BaseUrl}/{Endpoint}", _apiSettings.BaseUrl, endpoint);
        _logger.LogInformation("Timeout configurado: {Timeout}", _apiSettings.Timeout);
        _logger.LogInformation("Rate limit: {RateLimitPerMinute} chamadas por minuto", _apiSettings.RateLimitPerMinute);

        // Verificar feature flags
        if (_featureFlags.EnableAdvancedSearch && endpoint.Contains("search"))
        {
            _logger.LogInformation("Usando busca avançada (feature flag habilitada)");
        }

        if (_featureFlags.EnableNewDashboard)
        {
            _logger.LogInformation("Dashboard novo está habilitado");
        }

        // Verificar configurações de segurança
        if (_securitySettings.RequireHttps && !_apiSettings.BaseUrl.StartsWith("https"))
        {
            throw new InvalidOperationException("HTTPS é obrigatório mas a URL da API não usa HTTPS");
        }

        // Simulação da chamada da API
        await Task.Delay((int)_apiSettings.Timeout.TotalMilliseconds / 10);
        
        return $"Resposta da API {endpoint}";
    }
}

// Serviço que demonstra configuração personalizada
public class CustomApplicationService
{
    private readonly CustomApplicationSettings _settings;
    private readonly ILogger<CustomApplicationService> _logger;

    public CustomApplicationService(
        IOptions<CustomApplicationSettings> options, 
        ILogger<CustomApplicationService> logger)
    {
        _settings = options.Value;
        _logger = logger;
    }

    public void DisplayApplicationInfo()
    {
        _logger.LogInformation("=== Informações da Aplicação ===");
        _logger.LogInformation("Nome: {Name}", _settings.Name);
        _logger.LogInformation("Versão: {Version}", _settings.Version);
        _logger.LogInformation("Ambiente: {Environment}", _settings.Environment);
        
        _logger.LogInformation("=== Features ===");
        _logger.LogInformation("Analytics: {Analytics}", _settings.Features.Analytics);
        _logger.LogInformation("Monitoring: {Monitoring}", _settings.Features.Monitoring);
        _logger.LogInformation("Debugging: {Debugging}", _settings.Features.Debugging);
        
        _logger.LogInformation("=== Thresholds ===");
        _logger.LogInformation("Máximo de usuários: {MaxUsers}", _settings.Thresholds.MaxUsers);
        _logger.LogInformation("Máximo de conexões: {MaxConnections}", _settings.Thresholds.MaxConnections);
        _logger.LogInformation("Nível de alerta: {WarningLevel}%", _settings.Thresholds.WarningLevel);
    }

    public bool ShouldShowWarning(int currentUsers, int currentConnections)
    {
        var userPercentage = (double)currentUsers / _settings.Thresholds.MaxUsers * 100;
        var connectionPercentage = (double)currentConnections / _settings.Thresholds.MaxConnections * 100;
        
        return userPercentage >= _settings.Thresholds.WarningLevel || 
               connectionPercentage >= _settings.Thresholds.WarningLevel;
    }
}
