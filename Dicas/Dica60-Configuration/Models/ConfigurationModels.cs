using System.ComponentModel.DataAnnotations;

namespace Dica60.Configuration.Models;

// Modelo para configurações de banco de dados
public class DatabaseSettings
{
    public const string SectionName = "DatabaseSettings";

    [Required]
    public string Provider { get; set; } = string.Empty;

    [Range(1, 300)]
    public int ConnectionTimeout { get; set; } = 30;

    [Range(1, 3600)]
    public int CommandTimeout { get; set; } = 60;

    public bool EnableRetryOnFailure { get; set; } = true;

    [Range(0, 10)]
    public int MaxRetryCount { get; set; } = 3;

    public TimeSpan MaxRetryDelay { get; set; } = TimeSpan.FromSeconds(30);
}

// Modelo para configurações de cache
public class CacheSettings
{
    public const string SectionName = "CacheSettings";

    [Range(1, 1440)]
    public int DefaultExpirationMinutes { get; set; } = 60;

    [Range(1, 240)]
    public int SlidingExpirationMinutes { get; set; } = 15;

    [Required]
    public string MaxMemorySize { get; set; } = "100MB";

    [Range(0.1, 0.9)]
    public double CompactionPercentage { get; set; } = 0.2;
}

// Modelo para configurações de email
public class EmailSettings
{
    public const string SectionName = "EmailSettings";

    [Required]
    public string SmtpServer { get; set; } = string.Empty;

    [Range(1, 65535)]
    public int Port { get; set; } = 587;

    public bool EnableSsl { get; set; } = true;

    [Required]
    [EmailAddress]
    public string FromEmail { get; set; } = string.Empty;

    [Required]
    public string FromName { get; set; } = string.Empty;

    // Exemplo de propriedade computada
    public string DisplayName => $"{FromName} <{FromEmail}>";
}

// Modelo para configurações de API
public class ApiSettings
{
    public const string SectionName = "ApiSettings";

    [Required]
    [Url]
    public string BaseUrl { get; set; } = string.Empty;

    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);

    [Range(0, 10)]
    public int MaxRetries { get; set; } = 3;

    [Range(1, 10000)]
    public int RateLimitPerMinute { get; set; } = 100;
}

// Modelo para feature flags
public class FeatureFlags
{
    public const string SectionName = "FeatureFlags";

    public bool EnableNewDashboard { get; set; } = false;
    public bool EnableAdvancedSearch { get; set; } = true;
    public bool EnableBetaFeatures { get; set; } = false;

    // Método helper para verificar se todas as features estão habilitadas
    public bool AllFeaturesEnabled => EnableNewDashboard && EnableAdvancedSearch && EnableBetaFeatures;
}

// Modelo para configurações de segurança
public class SecuritySettings
{
    public const string SectionName = "SecuritySettings";

    [Required]
    [MinLength(32)]
    public string JwtSecretKey { get; set; } = string.Empty;

    [Range(1, 24 * 7)] // Máximo de 1 semana
    public int JwtExpirationHours { get; set; } = 24;

    public bool RequireHttps { get; set; } = true;

    public List<string> AllowedHosts { get; set; } = new();
}

// Modelo para seção customizada com aninhamento
public class CustomSection
{
    public const string SectionName = "CustomSection";

    public string Setting1 { get; set; } = string.Empty;
    public int Setting2 { get; set; }
    public bool Setting3 { get; set; }
    public NestedSettings NestedSettings { get; set; } = new();
}

public class NestedSettings
{
    public string NestedSetting1 { get; set; } = string.Empty;
    public List<string> NestedSetting2 { get; set; } = new();
}

// Modelo para aplicação personalizada (do arquivo custom-config.json)
public class CustomApplicationSettings
{
    public const string SectionName = "CustomApplication";

    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Version { get; set; } = string.Empty;

    [Required]
    public string Environment { get; set; } = string.Empty;

    public Features Features { get; set; } = new();
    public Thresholds Thresholds { get; set; } = new();
}

public class Features
{
    public bool Analytics { get; set; }
    public bool Monitoring { get; set; }
    public bool Debugging { get; set; }
}

public class Thresholds
{
    [Range(1, 100000)]
    public int MaxUsers { get; set; }

    [Range(1, 10000)]
    public int MaxConnections { get; set; }

    [Range(0.0, 100.0)]
    public double WarningLevel { get; set; }
}
