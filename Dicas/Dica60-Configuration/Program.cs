using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Dica60.Configuration.Models;
using Dica60.Configuration.Services;

namespace Dica60.Configuration;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("=== Dica 60: Configuration & Options Pattern ===\n");

        // Criar o application builder com configuração completa
        var builder = Host.CreateApplicationBuilder(args);

        // Configurar fontes de configuração
        ConfigureConfiguration(builder.Configuration, builder.Environment, args);

        // Configurar serviços
        ConfigureServices(builder.Services, builder.Configuration);

        var host = builder.Build();

        try
        {
            // Obter o serviço de demonstração
            var demoService = host.Services.GetRequiredService<ConfigurationDemoService>();
            
            // Executar a demonstração
            await demoService.RunDemoAsync(host.Services);

            // Demonstrar configuração em tempo de execução
            await DemonstrateRuntimeConfigurationAsync(host.Services);

            // Demonstrar diferentes fontes de configuração
            DemonstrateConfigurationSources(host.Services);
        }
        catch (Exception ex)
        {
            var logger = host.Services.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "Erro durante a execução da demonstração");
        }

        Console.WriteLine("\nPressione qualquer tecla para sair...");
        Console.ReadKey();
    }

    static void ConfigureConfiguration(ConfigurationManager configuration, IHostEnvironment environment, string[] args)
    {
        // Limpar configurações padrão e construir do zero
        configuration.Sources.Clear();

        // 1. Configuração base (appsettings.json)
        configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

        // 2. Configuração específica do ambiente
        configuration.AddJsonFile($"appsettings.{environment.EnvironmentName}.json", optional: true, reloadOnChange: true);

        // 3. Arquivo de configuração personalizada
        configuration.AddJsonFile("custom-config.json", optional: true, reloadOnChange: true);

        // 4. Variáveis de ambiente
        configuration.AddEnvironmentVariables("DICA60_");

        // 5. Argumentos da linha de comando
        if (args.Length > 0)
        {
            configuration.AddCommandLine(args);
        }

        // 6. User Secrets (apenas em desenvolvimento)
        if (environment.IsDevelopment())
        {
            configuration.AddUserSecrets<Program>();
        }

        // 7. Configuração em memória (para demonstração)
        var memoryConfig = new Dictionary<string, string?>
        {
            ["MemorySettings:DemoKey"] = "DemoValue",
            ["MemorySettings:CreatedAt"] = DateTime.Now.ToString("O"),
            ["FeatureFlags:EnableMemoryDemo"] = "true"
        };
        configuration.AddInMemoryCollection(memoryConfig);
    }

    static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // Registrar configurações tipadas com validação
        services.Configure<DatabaseSettings>(configuration.GetSection(DatabaseSettings.SectionName));
        services.Configure<CacheSettings>(configuration.GetSection(CacheSettings.SectionName));
        services.Configure<EmailSettings>(configuration.GetSection(EmailSettings.SectionName));
        services.Configure<ApiSettings>(configuration.GetSection(ApiSettings.SectionName));
        services.Configure<FeatureFlags>(configuration.GetSection(FeatureFlags.SectionName));
        services.Configure<SecuritySettings>(configuration.GetSection(SecuritySettings.SectionName));
        services.Configure<CustomSection>(configuration.GetSection(CustomSection.SectionName));
        services.Configure<CustomApplicationSettings>(configuration.GetSection(CustomApplicationSettings.SectionName));

        // Registrar validação de configurações
        services.AddOptions<DatabaseSettings>()
            .Bind(configuration.GetSection(DatabaseSettings.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<CacheSettings>()
            .Bind(configuration.GetSection(CacheSettings.SectionName))
            .ValidateDataAnnotations()
            .Validate(settings => settings.DefaultExpirationMinutes > settings.SlidingExpirationMinutes,
                "A expiração padrão deve ser maior que a expiração deslizante")
            .ValidateOnStart();

        services.AddOptions<EmailSettings>()
            .Bind(configuration.GetSection(EmailSettings.SectionName))
            .ValidateDataAnnotations()
            .Validate(settings => !string.IsNullOrEmpty(settings.SmtpServer),
                "Servidor SMTP é obrigatório")
            .ValidateOnStart();

        services.AddOptions<ApiSettings>()
            .Bind(configuration.GetSection(ApiSettings.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<SecuritySettings>()
            .Bind(configuration.GetSection(SecuritySettings.SectionName))
            .ValidateDataAnnotations()
            .Validate(settings => settings.JwtSecretKey.Length >= 32,
                "A chave secreta JWT deve ter pelo menos 32 caracteres")
            .ValidateOnStart();

        services.AddOptions<CustomApplicationSettings>()
            .Bind(configuration.GetSection(CustomApplicationSettings.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        // Registrar serviços
        services.AddSingleton<DatabaseService>();
        services.AddSingleton<CacheService>();
        services.AddScoped<EmailService>();
        services.AddSingleton<ApiService>();
        services.AddSingleton<CustomApplicationService>();
        services.AddSingleton<ConfigurationValidationService>();
        services.AddSingleton<ConfigurationDemoService>();

        // Adicionar logging
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Information);
        });
    }

    static async Task DemonstrateRuntimeConfigurationAsync(IServiceProvider services)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        var configuration = services.GetRequiredService<IConfiguration>();

        logger.LogInformation("\n=== Demonstrando Acesso à Configuração em Tempo de Execução ===");

        // Acessar valores individuais
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        logger.LogInformation("Connection String: {ConnectionString}", connectionString);

        // Acessar seções
        var loggingSection = configuration.GetSection("Logging");
        var defaultLogLevel = loggingSection["LogLevel:Default"];
        logger.LogInformation("Log Level Padrão: {DefaultLogLevel}", defaultLogLevel);

        // Verificar se uma chave existe
        var hasRedisConnection = configuration.GetConnectionString("RedisConnection") != null;
        logger.LogInformation("Possui configuração Redis: {HasRedisConnection}", hasRedisConnection);

        // Acessar arrays
        var allowedHosts = configuration.GetSection("SecuritySettings:AllowedHosts").Get<string[]>();
        if (allowedHosts != null)
        {
            logger.LogInformation("Hosts permitidos: {AllowedHosts}", string.Join(", ", allowedHosts));
        }

        // Demonstrar configuração de memória
        var memoryDemo = configuration["MemorySettings:DemoKey"];
        var memoryCreatedAt = configuration["MemorySettings:CreatedAt"];
        logger.LogInformation("Configuração em memória - DemoKey: {DemoKey}, CreatedAt: {CreatedAt}", 
            memoryDemo, memoryCreatedAt);

        await Task.CompletedTask;
    }

    static void DemonstrateConfigurationSources(IServiceProvider services)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        var configuration = services.GetRequiredService<IConfiguration>();

        logger.LogInformation("\n=== Demonstrando Fontes de Configuração ===");

        if (configuration is IConfigurationRoot configRoot)
        {
            logger.LogInformation("Fontes de configuração carregadas:");
            
            foreach (var provider in configRoot.Providers)
            {
                var providerType = provider.GetType().Name;
                logger.LogInformation("- {ProviderType}", providerType);
            }

            // Demonstrar debug de configuração
            logger.LogInformation("\nValores de configuração (debug):");
            
            foreach (var kvp in configRoot.AsEnumerable())
            {
                if (!string.IsNullOrEmpty(kvp.Value) && 
                    !kvp.Key.Contains("Secret", StringComparison.OrdinalIgnoreCase) &&
                    !kvp.Key.Contains("Password", StringComparison.OrdinalIgnoreCase))
                {
                    logger.LogDebug("{Key} = {Value}", kvp.Key, kvp.Value);
                }
            }
        }

        // Demonstrar precedência de configuração
        logger.LogInformation("\n=== Demonstrando Precedência de Configuração ===");
        
        // Esta configuração pode vir de diferentes fontes
        var enableNewDashboard = configuration["FeatureFlags:EnableNewDashboard"];
        logger.LogInformation("FeatureFlags:EnableNewDashboard = {Value}", enableNewDashboard);
        
        // Verificar se veio de variável de ambiente
        var envValue = Environment.GetEnvironmentVariable("DICA60_FeatureFlags__EnableNewDashboard");
        if (!string.IsNullOrEmpty(envValue))
        {
            logger.LogInformation("Valor da variável de ambiente: {EnvValue}", envValue);
        }
    }
}
