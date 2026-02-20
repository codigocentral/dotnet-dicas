using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Dica81_OptionsPattern.Configuration;
using Dica81_OptionsPattern.Services;

namespace Dica81_OptionsPattern;

/// <summary>
/// 🎯 Dica 81: Options Pattern e Configuration em .NET
/// 
/// Demonstra como usar IOptions, IOptionsSnapshot, IOptionsMonitor
/// para gerenciar configurações de forma tipada e validada.
/// </summary>
class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("🎯 Dica 81: Options Pattern e Configuration em .NET");
        Console.WriteLine("====================================================");
        
        try
        {
            var host = CreateHostBuilder(args).Build();
            
            // Demonstra validação durante a inicialização
            await ValidateConfigurationOnStartup(host);
            
            // Executa a demonstração
            await host.RunAsync();
        }
        catch (OptionsValidationException ex)
        {
            Console.WriteLine("❌ Erro de validação na configuração:");
            foreach (var failure in ex.Failures)
            {
                Console.WriteLine($"   - {failure}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Erro inesperado: {ex.Message}");
        }
    }

    static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((context, config) =>
            {
                // Configura as fontes de configuração
                config.SetBasePath(Directory.GetCurrentDirectory())
                      .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                      .AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", 
                                 optional: true, reloadOnChange: true)
                      .AddEnvironmentVariables()
                      .AddCommandLine(args ?? Array.Empty<string>());
            })
            .ConfigureServices((context, services) =>
            {
                var configuration = context.Configuration;
                
                // 🔧 Configura Options Pattern com validação
                ConfigureOptionsPattern(services, configuration);
                
                // 📋 Registra serviços
                RegisterServices(services);
                
                // 🎯 Registra o serviço principal de demonstração
                services.AddHostedService<DemoHostedService>();
            })
            .ConfigureLogging((context, logging) =>
            {
                logging.ClearProviders();
                logging.AddConsole();
                logging.AddDebug();
                logging.SetMinimumLevel(LogLevel.Information);
            });

    /// <summary>
    /// Configura o Options Pattern com diferentes estratégias
    /// </summary>
    static void ConfigureOptionsPattern(IServiceCollection services, IConfiguration configuration)
    {
        // 1. 📝 IOptions - Configuração básica (Singleton)
        services.Configure<DatabaseSettings>(
            configuration.GetSection(DatabaseSettings.SectionName));

        // 2. 🔍 IOptions com validação automática
        services.AddOptions<EmailSettings>()
            .Bind(configuration.GetSection(EmailSettings.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart(); // Valida na inicialização da aplicação

        // 3. 🌐 IOptions com validação customizada
        services.AddOptions<ApiSettings>()
            .Bind(configuration.GetSection(ApiSettings.SectionName))
            .ValidateDataAnnotations()
            .Validate(settings =>
            {
                // Validação customizada adicional
                if (settings.TimeoutInSeconds > 60 && settings.Features.EnableRetries)
                {
                    return false; // Timeout alto com retries pode causar problemas
                }
                return true;
            }, "Timeout alto com retries habilitados não é recomendado")
            .ValidateOnStart();

        // 4. 💾 IOptions com objetos complexos aninhados
        services.AddOptions<CacheSettings>()
            .Bind(configuration.GetSection(CacheSettings.SectionName))
            .ValidateDataAnnotations()
            .Validate(settings =>
            {
                // Validação específica para providers
                var enabledProviders = settings.Providers.Where(p => p.Enabled).ToList();
                if (!enabledProviders.Any())
                {
                    return false; // Deve ter pelo menos um provider habilitado
                }
                
                // Verifica se não há providers com mesma prioridade
                var priorities = enabledProviders.Select(p => p.Priority).ToList();
                return priorities.Count == priorities.Distinct().Count();
            }, "Deve ter pelo menos um provider habilitado e prioridades únicas")
            .ValidateOnStart();

        // 5. 🛠️ IValidateOptions para validação complexa
        services.AddSingleton<IValidateOptions<CacheSettings>, CacheSettingsValidator>();

        // 6. 📊 Post-configure para modificar configurações após bind
        services.PostConfigure<ApiSettings>(settings =>
        {
            // Exemplo: força HTTPS em produção
            if (settings.BaseUrl.StartsWith("http://"))
            {
                Console.WriteLine("⚠️  Convertendo HTTP para HTTPS por segurança");
                settings.BaseUrl = settings.BaseUrl.Replace("http://", "https://");
            }
        });
    }

    /// <summary>
    /// Registra todos os serviços da aplicação
    /// </summary>
    static void RegisterServices(IServiceCollection services)
    {
        // Serviços que demonstram diferentes tipos de Options
        services.AddScoped<DatabaseService>();   // IOptions
        services.AddScoped<EmailService>();      // IOptionsSnapshot  
        services.AddSingleton<ApiService>();     // IOptionsMonitor
        services.AddSingleton<CacheService>();   // IOptions com objetos complexos
        services.AddSingleton<ValidationService>(); // Validação manual
    }

    /// <summary>
    /// Valida as configurações durante a inicialização
    /// </summary>
    static async Task ValidateConfigurationOnStartup(IHost host)
    {
        using var scope = host.Services.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        
        try
        {
            logger.LogInformation("🔍 Validando configurações na inicialização...");
            
            // Força a validação das configurações obtendo as instâncias
            var databaseSettings = scope.ServiceProvider.GetRequiredService<IOptions<DatabaseSettings>>();
            var emailSettings = scope.ServiceProvider.GetRequiredService<IOptions<EmailSettings>>();
            var apiSettings = scope.ServiceProvider.GetRequiredService<IOptions<ApiSettings>>();
            var cacheSettings = scope.ServiceProvider.GetRequiredService<IOptions<CacheSettings>>();

            // Acessa os valores para forçar a validação
            _ = databaseSettings.Value;
            _ = emailSettings.Value;
            _ = apiSettings.Value;
            _ = cacheSettings.Value;

            logger.LogInformation("✅ Todas as configurações são válidas!");
            
            await Task.Delay(500); // Simula validação assíncrona
        }
        catch (OptionsValidationException ex)
        {
            logger.LogError("❌ Erro de validação nas configurações:");
            foreach (var failure in ex.Failures)
            {
                logger.LogError("   - {Failure}", failure);
            }
            throw;
        }
    }
}

/// <summary>
/// Validador customizado para CacheSettings
/// </summary>
public class CacheSettingsValidator : IValidateOptions<CacheSettings>
{
    public ValidateOptionsResult Validate(string? name, CacheSettings options)
    {
        var failures = new List<string>();

        // Validação específica para cache
        if (options.DefaultExpirationMinutes <= 0)
        {
            failures.Add("DefaultExpirationMinutes deve ser maior que zero");
        }

        if (options.MaxMemoryMB > 512) // Limite de 512MB
        {
            failures.Add("MaxMemoryMB não deve exceder 512MB para evitar problemas de memória");
        }

        // Validação dos providers
        var enabledProviders = options.Providers.Where(p => p.Enabled).ToList();
        if (enabledProviders.Count == 0)
        {
            failures.Add("Pelo menos um provider de cache deve estar habilitado");
        }

        // Verifica se Redis está configurado corretamente
        var redisProvider = enabledProviders.FirstOrDefault(p => 
            p.Name.Equals("Redis", StringComparison.OrdinalIgnoreCase));
        
        if (redisProvider != null && string.IsNullOrWhiteSpace(redisProvider.ConnectionString))
        {
            failures.Add("Provider Redis está habilitado mas ConnectionString não foi informada");
        }

        return failures.Count > 0 
            ? ValidateOptionsResult.Fail(failures)
            : ValidateOptionsResult.Success;
    }
}
