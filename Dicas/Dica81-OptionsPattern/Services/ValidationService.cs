using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using Dica81_OptionsPattern.Configuration;

namespace Dica81_OptionsPattern.Services;

/// <summary>
/// Serviço que demonstra validação de configuração e tratamento de erros
/// </summary>
public class ValidationService
{
    private readonly ILogger<ValidationService> _logger;

    public ValidationService(ILogger<ValidationService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Demonstra validação manual de configuração
    /// </summary>
    public ConfigurationValidationResult ValidateSettings<T>(T settings) where T : class
    {
        var context = new ValidationContext(settings);
        var results = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
        
        bool isValid = Validator.TryValidateObject(settings, context, results, true);
        
        if (!isValid)
        {
            _logger.LogError("❌ Configuração inválida encontrada:");
            foreach (var error in results)
            {
                _logger.LogError("  - {Error}", error.ErrorMessage);
            }
        }
        else
        {
            _logger.LogInformation("✅ Configuração validada com sucesso");
        }

        return new ConfigurationValidationResult
        {
            IsValid = isValid,
            Errors = results.Select(r => r.ErrorMessage ?? "Erro desconhecido").ToList()
        };
    }

    /// <summary>
    /// Demonstra validação de configuração específica
    /// </summary>
    public async Task<string> ValidateAllConfigurationsAsync(
        IOptions<DatabaseSettings> databaseOptions,
        IOptions<EmailSettings> emailOptions,
        IOptions<ApiSettings> apiOptions,
        IOptions<CacheSettings> cacheOptions)
    {
        _logger.LogInformation("🔍 Iniciando validação de todas as configurações...");
        
        var results = new List<(string Section, ConfigurationValidationResult Result)>();
        
        // Simula validação assíncrona
        await Task.Delay(100);
        
        // Valida Database Settings
        try
        {
            var dbResult = ValidateSettings(databaseOptions.Value);
            results.Add(("Database", dbResult));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao validar configurações do banco");
            results.Add(("Database", new ConfigurationValidationResult { IsValid = false, Errors = [ex.Message] }));
        }
        
        // Valida Email Settings
        try
        {
            var emailResult = ValidateSettings(emailOptions.Value);
            results.Add(("Email", emailResult));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao validar configurações de email");
            results.Add(("Email", new ConfigurationValidationResult { IsValid = false, Errors = [ex.Message] }));
        }
        
        // Valida API Settings
        try
        {
            var apiResult = ValidateSettings(apiOptions.Value);
            results.Add(("API", apiResult));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao validar configurações da API");
            results.Add(("API", new ConfigurationValidationResult { IsValid = false, Errors = [ex.Message] }));
        }
        
        // Valida Cache Settings
        try
        {
            var cacheResult = ValidateSettings(cacheOptions.Value);
            results.Add(("Cache", cacheResult));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao validar configurações do cache");
            results.Add(("Cache", new ConfigurationValidationResult { IsValid = false, Errors = [ex.Message] }));
        }
        
        // Gera relatório
        var validCount = results.Count(r => r.Result.IsValid);
        var invalidCount = results.Count - validCount;
        
        var report = $"""
                    📊 Relatório de Validação de Configurações:
                    
                    ✅ Válidas: {validCount}
                    ❌ Inválidas: {invalidCount}
                    
                    Detalhes por seção:
                    """;
        
        foreach (var (section, result) in results)
        {
            var status = result.IsValid ? "✅" : "❌";
            report += $"\n{status} {section}: " + (result.IsValid ? "OK" : $"ERRO");
            
            if (!result.IsValid)
            {
                foreach (var error in result.Errors)
                {
                    report += $"\n   - {error}";
                }
            }
        }
        
        if (invalidCount > 0)
        {
            _logger.LogWarning("⚠️  Encontradas {InvalidCount} configurações inválidas", invalidCount);
        }
        else
        {
            _logger.LogInformation("🎉 Todas as configurações são válidas!");
        }
        
        return report;
    }
}

/// <summary>
/// Resultado da validação
/// </summary>
public class ConfigurationValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
}
