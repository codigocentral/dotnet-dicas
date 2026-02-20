using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Scrutor;
using Dica61.DependencyInjection.Interfaces;
using Dica61.DependencyInjection.Services;

namespace Dica61.DependencyInjection;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("=== Dica 61: Dependency Injection ===\n");

        // Criar o application builder
        var builder = Host.CreateApplicationBuilder(args);

        // Configurar serviços
        ConfigureServices(builder.Services, builder.Configuration);

        var host = builder.Build();

        try
        {
            // Executar demonstração
            var demoService = host.Services.GetRequiredService<DependencyInjectionDemoService>();
            await demoService.RunDemoAsync();

            // Demonstrar análise de container
            AnalyzeDependencyContainer(host.Services);
        }
        catch (Exception ex)
        {
            var logger = host.Services.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "Erro durante a execução da demonstração");
        }

        Console.WriteLine("\nPressione qualquer tecla para sair...");
        Console.ReadKey();
    }

    static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // === CONFIGURAÇÃO DE LOGGING ===
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Information);
        });

        // === REPOSITÓRIOS ===
        // Registro básico
        services.AddSingleton<IUserRepository, InMemoryUserRepository>();

        // === SERVIÇOS DE INFRAESTRUTURA ===
        // Cache
        services.AddSingleton<ICacheService, InMemoryCacheService>();
        
        // Audit Logger
        services.AddSingleton<IAuditLogger, DatabaseAuditLogger>();
        
        // Notification Service
        services.AddSingleton<INotificationService, EmailNotificationService>();
        
        // Business Rule Validator
        services.AddScoped<IBusinessRuleValidator, BusinessRuleValidator>();
        
        // Factory
        services.AddSingleton<IUserFactory, UserFactory>();

        // === DEMONSTRAÇÃO DE LIFETIMES ===
        services.AddSingleton<ISingletonService, SingletonService>();
        services.AddScoped<IScopedService, ScopedService>();
        services.AddTransient<ITransientService, TransientService>();

        // === SERVIÇOS DE DOMÍNIO ===
        // Registro básico do UserService
        services.AddScoped<UserService>();

        // === PADRÃO DECORATOR ===
        // Registrar o UserService decorado com logging
        services.AddScoped<IUserService>(provider =>
        {
            var userService = provider.GetRequiredService<UserService>();
            var logger = provider.GetRequiredService<ILogger<LoggingUserServiceDecorator>>();
            return new LoggingUserServiceDecorator(userService, logger);
        });

        // === HTTP CLIENT ===
        services.AddHttpClient<IExternalApiService, ExternalApiService>(client =>
        {
            var baseUrl = configuration["ExternalApiSettings:BaseUrl"];
            if (!string.IsNullOrEmpty(baseUrl))
            {
                client.BaseAddress = new Uri(baseUrl);
            }
            
            var timeout = configuration["ExternalApiSettings:Timeout"];
            if (TimeSpan.TryParse(timeout, out var timeoutValue))
            {
                client.Timeout = timeoutValue;
            }
        });

        // === PADRÃO STRATEGY ===
        // Registrar múltiplas implementações de IProcessingStrategy
        services.AddTransient<IProcessingStrategy, FastProcessingStrategy>();
        services.AddTransient<IProcessingStrategy, SlowProcessingStrategy>();

        // === HANDLERS GENÉRICOS ===
        services.AddScoped<IHandler<int, User?>, UserQueryHandler>();

        // === SERVIÇOS EM BACKGROUND ===
        services.AddSingleton<IBackgroundTaskService, BackgroundTaskService>();

        // === REGISTRO AUTOMÁTICO COM SCRUTOR ===
        // Registrar automaticamente todos os serviços que implementam interfaces específicas
        services.Scan(scan => scan
            .FromAssemblyOf<Program>()
            .AddClasses(classes => classes.Where(type => type.Name.EndsWith("Service") && !type.Name.Contains("Demo")))
            .UsingRegistrationStrategy(RegistrationStrategy.Skip) // Pular se já registrado
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        // === REGISTRO CONDICIONAL ===
        // Registrar serviço apenas se configuração específica estiver presente
        var enableDetailedLogging = configuration.GetValue<bool>("ServiceSettings:EnableDetailedLogging");
        if (enableDetailedLogging)
        {
            services.AddSingleton<DetailedLoggingService>();
        }

        // === FACTORY PATTERN AVANÇADO ===
        // Registrar factory usando delegate
        services.AddSingleton<Func<string, IProcessingStrategy>>(provider =>
        {
            return strategyName => strategyName.ToLower() switch
            {
                "fast" => provider.GetRequiredService<FastProcessingStrategy>(),
                "slow" => provider.GetRequiredService<SlowProcessingStrategy>(),
                _ => throw new ArgumentException($"Estratégia desconhecida: {strategyName}")
            };
        });

        // === CONFIGURAÇÃO DE OPTIONS ===
        services.Configure<ServiceSettings>(configuration.GetSection("ServiceSettings"));

        // === VALIDAÇÃO DE CONTAINER ===
        // Adicionar serviço para validar o container
        services.AddSingleton<ContainerValidationService>();

        // === SERVIÇO PRINCIPAL DE DEMONSTRAÇÃO ===
        services.AddSingleton<DependencyInjectionDemoService>();

        // === REGISTRO DE DECORATORS MÚLTIPLOS ===
        // Exemplo de como registrar múltiplos decorators
        services.Decorate<IUserService, CachingUserServiceDecorator>();
        services.Decorate<IUserService, TimingUserServiceDecorator>();
    }

    static void AnalyzeDependencyContainer(IServiceProvider serviceProvider)
    {
        var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
        
        logger.LogInformation("\n=== Análise do Container de DI ===");

        // Verificar se o container foi construído corretamente
        var validationService = serviceProvider.GetService<ContainerValidationService>();
        if (validationService != null)
        {
            validationService.ValidateContainer(serviceProvider);
        }

        // Mostrar estatísticas do container
        if (serviceProvider is ServiceProvider sp)
        {
            logger.LogInformation("Container de DI configurado com sucesso");
            
            // Demonstrar resolução de múltiplos serviços
            var strategies = serviceProvider.GetServices<IProcessingStrategy>().ToList();
            logger.LogInformation("Estratégias registradas: {Count}", strategies.Count);
            
            // Verificar singleton
            var singleton = serviceProvider.GetRequiredService<ISingletonService>();
            logger.LogInformation("Singleton service ID: {InstanceId}, criado em: {CreatedAt}", 
                singleton.InstanceId, singleton.CreatedAt);
        }
    }
}

// Configurações tipadas
public class ServiceSettings
{
    public string DatabaseConnectionString { get; set; } = string.Empty;
    public int CacheExpirationMinutes { get; set; }
    public int MaxRetryCount { get; set; }
    public bool EnableDetailedLogging { get; set; }
}

// Serviço condicional
public class DetailedLoggingService
{
    private readonly ILogger<DetailedLoggingService> _logger;

    public DetailedLoggingService(ILogger<DetailedLoggingService> logger)
    {
        _logger = logger;
        _logger.LogInformation("DetailedLoggingService foi habilitado via configuração");
    }

    public void LogDetailed(string message, object? data = null)
    {
        _logger.LogDebug("DETAILED: {Message} | Data: {@Data}", message, data);
    }
}

// Decorator adicional para cache
public class CachingUserServiceDecorator : IUserService
{
    private readonly IUserService _inner;
    private readonly ICacheService _cache;
    private readonly ILogger<CachingUserServiceDecorator> _logger;

    public CachingUserServiceDecorator(IUserService inner, ICacheService cache, ILogger<CachingUserServiceDecorator> logger)
    {
        _inner = inner;
        _cache = cache;
        _logger = logger;
    }

    public async Task<User?> GetUserAsync(int id)
    {
        var cacheKey = $"user_decorator:{id}";
        var cached = await _cache.GetAsync<User>(cacheKey);
        if (cached != null)
        {
            _logger.LogDebug("CACHE DECORATOR: Cache hit para usuário {UserId}", id);
            return cached;
        }

        _logger.LogDebug("CACHE DECORATOR: Cache miss para usuário {UserId}", id);
        var user = await _inner.GetUserAsync(id);
        if (user != null)
        {
            await _cache.SetAsync(cacheKey, user, TimeSpan.FromMinutes(5));
        }
        return user;
    }

    public Task<User> CreateUserAsync(string name, string email) => _inner.CreateUserAsync(name, email);
    public Task<User> UpdateUserAsync(int id, string name, string email) => _inner.UpdateUserAsync(id, name, email);
    public Task DeleteUserAsync(int id) => _inner.DeleteUserAsync(id);
    public Task<IEnumerable<User>> GetActiveUsersAsync() => _inner.GetActiveUsersAsync();
}

// Decorator adicional para timing
public class TimingUserServiceDecorator : IUserService
{
    private readonly IUserService _inner;
    private readonly ILogger<TimingUserServiceDecorator> _logger;

    public TimingUserServiceDecorator(IUserService inner, ILogger<TimingUserServiceDecorator> logger)
    {
        _inner = inner;
        _logger = logger;
    }

    public async Task<User?> GetUserAsync(int id)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            return await _inner.GetUserAsync(id);
        }
        finally
        {
            sw.Stop();
            _logger.LogDebug("TIMING DECORATOR: GetUserAsync levou {ElapsedMs}ms", sw.ElapsedMilliseconds);
        }
    }

    public async Task<User> CreateUserAsync(string name, string email)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            return await _inner.CreateUserAsync(name, email);
        }
        finally
        {
            sw.Stop();
            _logger.LogDebug("TIMING DECORATOR: CreateUserAsync levou {ElapsedMs}ms", sw.ElapsedMilliseconds);
        }
    }

    public Task<User> UpdateUserAsync(int id, string name, string email) => _inner.UpdateUserAsync(id, name, email);
    public Task DeleteUserAsync(int id) => _inner.DeleteUserAsync(id);
    public Task<IEnumerable<User>> GetActiveUsersAsync() => _inner.GetActiveUsersAsync();
}

// Serviço para validar o container
public class ContainerValidationService
{
    private readonly ILogger<ContainerValidationService> _logger;

    public ContainerValidationService(ILogger<ContainerValidationService> logger)
    {
        _logger = logger;
    }

    public void ValidateContainer(IServiceProvider serviceProvider)
    {
        _logger.LogInformation("Validando configuração do container...");

        // Verificar serviços críticos
        var criticalServices = new[]
        {
            typeof(IUserService),
            typeof(IUserRepository),
            typeof(ICacheService),
            typeof(IAuditLogger),
            typeof(INotificationService)
        };

        foreach (var serviceType in criticalServices)
        {
            var service = serviceProvider.GetService(serviceType);
            if (service != null)
            {
                _logger.LogDebug("✓ {ServiceType} registrado corretamente", serviceType.Name);
            }
            else
            {
                _logger.LogError("✗ {ServiceType} NÃO está registrado!", serviceType.Name);
            }
        }

        // Verificar múltiplas implementações
        var strategies = serviceProvider.GetServices<IProcessingStrategy>().ToList();
        _logger.LogInformation("Estratégias de processamento registradas: {Count}", strategies.Count);

        _logger.LogInformation("Validação do container concluída");
    }
}
