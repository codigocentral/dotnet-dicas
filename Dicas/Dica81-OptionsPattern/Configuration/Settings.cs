using System.ComponentModel.DataAnnotations;

namespace Dica81_OptionsPattern.Configuration;

/// <summary>
/// Configurações do banco de dados com validação
/// </summary>
public class DatabaseSettings
{
    public const string SectionName = "DatabaseSettings";

    [Required(ErrorMessage = "ConnectionString é obrigatória")]
    [MinLength(10, ErrorMessage = "ConnectionString deve ter pelo menos 10 caracteres")]
    public string ConnectionString { get; set; } = string.Empty;

    [Range(1, 300, ErrorMessage = "CommandTimeout deve estar entre 1 e 300 segundos")]
    public int CommandTimeout { get; set; } = 30;

    public bool EnableRetryOnFailure { get; set; } = true;

    [Range(0, 10, ErrorMessage = "MaxRetryCount deve estar entre 0 e 10")]
    public int MaxRetryCount { get; set; } = 3;
}

/// <summary>
/// Configurações de email com validação complexa
/// </summary>
public class EmailSettings
{
    public const string SectionName = "EmailSettings";

    [Required(ErrorMessage = "SmtpServer é obrigatório")]
    [RegularExpression(@"^[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", 
        ErrorMessage = "SmtpServer deve ser um servidor válido")]
    public string SmtpServer { get; set; } = string.Empty;

    [Range(1, 65535, ErrorMessage = "Port deve estar entre 1 e 65535")]
    public int Port { get; set; } = 587;

    [Required(ErrorMessage = "Username é obrigatório")]
    [EmailAddress(ErrorMessage = "Username deve ser um email válido")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password é obrigatória")]
    [MinLength(6, ErrorMessage = "Password deve ter pelo menos 6 caracteres")]
    public string Password { get; set; } = string.Empty;

    public bool EnableSsl { get; set; } = true;

    [Required(ErrorMessage = "FromEmail é obrigatório")]
    [EmailAddress(ErrorMessage = "FromEmail deve ser um email válido")]
    public string FromEmail { get; set; } = string.Empty;

    [Required(ErrorMessage = "FromName é obrigatório")]
    [MaxLength(100, ErrorMessage = "FromName não pode ter mais de 100 caracteres")]
    public string FromName { get; set; } = string.Empty;
}

/// <summary>
/// Configurações de API com objetos aninhados
/// </summary>
public class ApiSettings
{
    public const string SectionName = "ApiSettings";

    [Required(ErrorMessage = "BaseUrl é obrigatória")]
    [Url(ErrorMessage = "BaseUrl deve ser uma URL válida")]
    public string BaseUrl { get; set; } = string.Empty;

    [Range(1, 300, ErrorMessage = "TimeoutInSeconds deve estar entre 1 e 300")]
    public int TimeoutInSeconds { get; set; } = 30;

    [Required(ErrorMessage = "ApiKey é obrigatória")]
    [MinLength(10, ErrorMessage = "ApiKey deve ter pelo menos 10 caracteres")]
    public string ApiKey { get; set; } = string.Empty;

    [Required(ErrorMessage = "Features é obrigatório")]
    public ApiFeatures Features { get; set; } = new();

    [Required(ErrorMessage = "AllowedHosts é obrigatório")]
    [MinLength(1, ErrorMessage = "Deve ter pelo menos um host permitido")]
    public List<string> AllowedHosts { get; set; } = new();
}

/// <summary>
/// Configurações de features da API
/// </summary>
public class ApiFeatures
{
    public bool EnableCaching { get; set; } = true;
    public bool EnableLogging { get; set; } = true;
    public bool EnableRetries { get; set; } = false;
}

/// <summary>
/// Configurações de cache com providers complexos
/// </summary>
public class CacheSettings
{
    public const string SectionName = "CacheSettings";

    [Range(1, 1440, ErrorMessage = "DefaultExpirationMinutes deve estar entre 1 e 1440 (24h)")]
    public int DefaultExpirationMinutes { get; set; } = 15;

    [Range(1, 1024, ErrorMessage = "MaxMemoryMB deve estar entre 1 e 1024")]
    public int MaxMemoryMB { get; set; } = 100;

    public bool SlidingExpiration { get; set; } = true;

    [Required(ErrorMessage = "Providers é obrigatório")]
    [MinLength(1, ErrorMessage = "Deve ter pelo menos um provider")]
    public List<CacheProvider> Providers { get; set; } = new();
}

/// <summary>
/// Configuração de um provider de cache
/// </summary>
public class CacheProvider
{
    [Required(ErrorMessage = "Name é obrigatório")]
    [RegularExpression(@"^[A-Za-z][A-Za-z0-9]*$", 
        ErrorMessage = "Name deve conter apenas letras e números, começando com letra")]
    public string Name { get; set; } = string.Empty;

    public bool Enabled { get; set; } = true;

    [Range(1, 10, ErrorMessage = "Priority deve estar entre 1 e 10")]
    public int Priority { get; set; } = 1;

    public string? ConnectionString { get; set; }
}

/// <summary>
/// Classe para demonstrar validação customizada
/// </summary>
public class CustomValidatedSettings
{
    public const string SectionName = "CustomSettings";

    [CustomValidation(typeof(CustomValidatedSettings), nameof(ValidateEnvironment))]
    public string Environment { get; set; } = "Development";

    [Range(1, 100)]
    public int MaxConcurrency { get; set; } = 10;

    /// <summary>
    /// Método de validação customizada
    /// </summary>
    public static ValidationResult? ValidateEnvironment(string environment, ValidationContext context)
    {
        var validEnvironments = new[] { "Development", "Staging", "Production" };
        
        if (!validEnvironments.Contains(environment, StringComparer.OrdinalIgnoreCase))
        {
            return new ValidationResult(
                $"Environment deve ser um dos valores: {string.Join(", ", validEnvironments)}",
                new[] { context.MemberName ?? "Environment" });
        }

        return ValidationResult.Success;
    }
}
