using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using Dica60.Configuration.Models;

namespace Dica60.Configuration.Services;

// Demonstra validação customizada de configurações
public class ConfigurationValidationService
{
    private readonly ILogger<ConfigurationValidationService> _logger;

    public ConfigurationValidationService(ILogger<ConfigurationValidationService> logger)
    {
        _logger = logger;
    }

    public bool ValidateAllConfigurations(IServiceProvider serviceProvider)
    {
        var validationResults = new List<ValidationResult>();
        var isValid = true;

        // Validar todas as configurações registradas
        isValid &= ValidateConfiguration<DatabaseSettings>(serviceProvider, validationResults);
        isValid &= ValidateConfiguration<CacheSettings>(serviceProvider, validationResults);
        isValid &= ValidateConfiguration<EmailSettings>(serviceProvider, validationResults);
        isValid &= ValidateConfiguration<ApiSettings>(serviceProvider, validationResults);
        isValid &= ValidateConfiguration<SecuritySettings>(serviceProvider, validationResults);
        isValid &= ValidateConfiguration<CustomApplicationSettings>(serviceProvider, validationResults);

        if (!isValid)
        {
            _logger.LogError("Falhas de validação encontradas:");
            foreach (var result in validationResults)
            {
                _logger.LogError("- {ErrorMessage}", result.ErrorMessage);
                if (result.MemberNames.Any())
                {
                    _logger.LogError("  Propriedades: {Members}", string.Join(", ", result.MemberNames));
                }
            }
        }
        else
        {
            _logger.LogInformation("Todas as configurações foram validadas com sucesso!");
        }

        return isValid;
    }

    private bool ValidateConfiguration<T>(IServiceProvider serviceProvider, List<ValidationResult> validationResults) 
        where T : class
    {
        try
        {
            var options = serviceProvider.GetRequiredService<IOptions<T>>();
            var configuration = options.Value;
            
            var context = new ValidationContext(configuration);
            var results = new List<ValidationResult>();
            
            var isValid = Validator.TryValidateObject(configuration, context, results, true);
            
            if (!isValid)
            {
                _logger.LogWarning("Validação falhou para {ConfigurationType}", typeof(T).Name);
                validationResults.AddRange(results);
            }
            else
            {
                _logger.LogDebug("Validação bem-sucedida para {ConfigurationType}", typeof(T).Name);
            }
            
            return isValid;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao validar configuração {ConfigurationType}", typeof(T).Name);
            validationResults.Add(new ValidationResult($"Erro ao validar {typeof(T).Name}: {ex.Message}"));
            return false;
        }
    }
}

// Serviço que demonstra o padrão Options
public class ConfigurationDemoService
{
    private readonly DatabaseService _databaseService;
    private readonly CacheService _cacheService;
    private readonly EmailService _emailService;
    private readonly ApiService _apiService;
    private readonly CustomApplicationService _customApplicationService;
    private readonly ConfigurationValidationService _validationService;
    private readonly ILogger<ConfigurationDemoService> _logger;

    public ConfigurationDemoService(
        DatabaseService databaseService,
        CacheService cacheService,
        EmailService emailService,
        ApiService apiService,
        CustomApplicationService customApplicationService,
        ConfigurationValidationService validationService,
        ILogger<ConfigurationDemoService> logger)
    {
        _databaseService = databaseService;
        _cacheService = cacheService;
        _emailService = emailService;
        _apiService = apiService;
        _customApplicationService = customApplicationService;
        _validationService = validationService;
        _logger = logger;
    }

    public async Task RunDemoAsync(IServiceProvider serviceProvider)
    {
        _logger.LogInformation("=== Iniciando Demonstração do Sistema de Configuração ===");

        try
        {
            // 1. Validar todas as configurações
            _logger.LogInformation("\n1. Validando configurações...");
            var isValid = _validationService.ValidateAllConfigurations(serviceProvider);
            
            if (!isValid)
            {
                _logger.LogError("Configurações inválidas detectadas. Abortando demonstração.");
                return;
            }

            // 2. Demonstrar uso do DatabaseService (IOptions<T>)
            _logger.LogInformation("\n2. Demonstrando DatabaseService (IOptions<T>):");
            _databaseService.Connect();

            // 3. Demonstrar uso do CacheService (IOptionsMonitor<T>)
            _logger.LogInformation("\n3. Demonstrando CacheService (IOptionsMonitor<T>):");
            _cacheService.Initialize();

            // 4. Demonstrar uso do EmailService (IOptionsSnapshot<T>)
            _logger.LogInformation("\n4. Demonstrando EmailService (IOptionsSnapshot<T>):");
            await _emailService.SendEmailAsync(
                "usuario@exemplo.com", 
                "Teste de Configuração", 
                "Este é um email de teste do sistema de configuração.");

            // 5. Demonstrar uso do ApiService (múltiplas configurações)
            _logger.LogInformation("\n5. Demonstrando ApiService (múltiplas configurações):");
            var response = await _apiService.CallApiAsync("users/search");
            _logger.LogInformation("Resposta da API: {Response}", response);

            // 6. Demonstrar configuração personalizada
            _logger.LogInformation("\n6. Demonstrando configuração personalizada:");
            _customApplicationService.DisplayApplicationInfo();

            // 7. Demonstrar lógica baseada em configuração
            _logger.LogInformation("\n7. Demonstrando lógica baseada em configuração:");
            var shouldWarn = _customApplicationService.ShouldShowWarning(8500, 400);
            _logger.LogInformation("Deve mostrar alerta? {ShouldWarn}", shouldWarn);

            _logger.LogInformation("\n=== Demonstração Concluída com Sucesso ===");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro durante a demonstração");
            throw;
        }
    }
}
