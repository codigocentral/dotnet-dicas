using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Dica61.DependencyInjection.Interfaces;
using Dica61.DependencyInjection.Services;

namespace Dica61.DependencyInjection;

// Serviço principal de demonstração
public class DependencyInjectionDemoService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DependencyInjectionDemoService> _logger;

    public DependencyInjectionDemoService(IServiceProvider serviceProvider, ILogger<DependencyInjectionDemoService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task RunDemoAsync()
    {
        _logger.LogInformation("=== Iniciando Demonstração de Dependency Injection ===");

        try
        {
            await DemonstrateBasicDependencyInjectionAsync();
            await DemonstrateServiceLifetimesAsync();
            await DemonstrateDecoratorPatternAsync();
            await DemonstrateFactoryPatternAsync();
            await DemonstrateStrategyPatternAsync();
            await DemonstrateGenericServicesAsync();
            DemonstrateServiceProviderUsage();
            await DemonstrateBackgroundServicesAsync();

            _logger.LogInformation("=== Demonstração Concluída com Sucesso ===");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro durante a demonstração");
            throw;
        }
    }

    private async Task DemonstrateBasicDependencyInjectionAsync()
    {
        _logger.LogInformation("\n1. === Demonstrando Injeção de Dependência Básica ===");

        // Resolver serviço através do container
        var userService = _serviceProvider.GetRequiredService<IUserService>();

        // Criar usuário
        var user = await userService.CreateUserAsync("João da Silva", "joao.silva@example.com");
        _logger.LogInformation("Usuário criado: {UserId} - {UserName}", user.Id, user.Name);

        // Buscar usuário
        var retrievedUser = await userService.GetUserAsync(user.Id);
        _logger.LogInformation("Usuário recuperado: {UserId} - {UserName}", retrievedUser?.Id, retrievedUser?.Name);

        // Listar usuários ativos
        var activeUsers = await userService.GetActiveUsersAsync();
        _logger.LogInformation("Total de usuários ativos: {Count}", activeUsers.Count());
    }

    private async Task DemonstrateServiceLifetimesAsync()
    {
        _logger.LogInformation("\n2. === Demonstrando Lifetimes de Serviços ===");

        // Demonstrar Singleton
        var singleton1 = _serviceProvider.GetRequiredService<ISingletonService>();
        var singleton2 = _serviceProvider.GetRequiredService<ISingletonService>();
        
        _logger.LogInformation("Singleton 1 ID: {Id1}, Singleton 2 ID: {Id2}, São iguais: {AreEqual}",
            singleton1.InstanceId, singleton2.InstanceId, singleton1.InstanceId == singleton2.InstanceId);

        singleton1.IncrementRequestCount();
        singleton2.IncrementRequestCount();
        _logger.LogInformation("Singleton request count: {Count}", singleton1.RequestCount);

        // Demonstrar Transient
        var transient1 = _serviceProvider.GetRequiredService<ITransientService>();
        var transient2 = _serviceProvider.GetRequiredService<ITransientService>();
        
        _logger.LogInformation("Transient 1 ID: {Id1}, Transient 2 ID: {Id2}, São diferentes: {AreDifferent}",
            transient1.InstanceId, transient2.InstanceId, transient1.InstanceId != transient2.InstanceId);

        await transient1.ProcessTransientOperationAsync();
        await transient2.ProcessTransientOperationAsync();

        // Demonstrar Scoped em diferentes scopes
        using (var scope1 = _serviceProvider.CreateScope())
        {
            var scoped1a = scope1.ServiceProvider.GetRequiredService<IScopedService>();
            var scoped1b = scope1.ServiceProvider.GetRequiredService<IScopedService>();
            
            _logger.LogInformation("Scope 1 - Scoped 1a ID: {Id1a}, Scoped 1b ID: {Id1b}, São iguais: {AreEqual}",
                scoped1a.InstanceId, scoped1b.InstanceId, scoped1a.InstanceId == scoped1b.InstanceId);

            await scoped1a.ProcessScopedOperationAsync();
        }

        using (var scope2 = _serviceProvider.CreateScope())
        {
            var scoped2 = scope2.ServiceProvider.GetRequiredService<IScopedService>();
            _logger.LogInformation("Scope 2 - Scoped ID: {Id2}", scoped2.InstanceId);
            await scoped2.ProcessScopedOperationAsync();
        }
    }

    private async Task DemonstrateDecoratorPatternAsync()
    {
        _logger.LogInformation("\n3. === Demonstrando Padrão Decorator ===");

        // O UserService está decorado com LoggingUserServiceDecorator
        var decoratedUserService = _serviceProvider.GetRequiredService<IUserService>();
        
        _logger.LogInformation("Criando usuário através do serviço decorado...");
        var user = await decoratedUserService.CreateUserAsync("Maria Decorada", "maria.decorada@example.com");
        
        _logger.LogInformation("Buscando usuário através do serviço decorado...");
        var retrievedUser = await decoratedUserService.GetUserAsync(user.Id);
    }

    private async Task DemonstrateFactoryPatternAsync()
    {
        _logger.LogInformation("\n4. === Demonstrando Padrão Factory ===");

        var userFactory = _serviceProvider.GetRequiredService<IUserFactory>();
        
        // Criar usuário usando factory
        var user1 = userFactory.CreateUser("Pedro Factory", "pedro.factory@example.com");
        _logger.LogInformation("Usuário criado via factory: {Name} - {Email}", user1.Name, user1.Email);

        // Criar usuário usando factory com DTO
        var dto = new UserDto { Name = "Ana Factory", Email = "ana.factory@example.com" };
        var user2 = userFactory.CreateUser(dto);
        _logger.LogInformation("Usuário criado via factory com DTO: {Name} - {Email}", user2.Name, user2.Email);

        await Task.CompletedTask;
    }

    private async Task DemonstrateStrategyPatternAsync()
    {
        _logger.LogInformation("\n5. === Demonstrando Padrão Strategy ===");

        // Resolver todas as estratégias registradas
        var strategies = _serviceProvider.GetServices<IProcessingStrategy>();
        
        var request = new ProcessingRequest 
        { 
            Type = "DataProcessing",
            Parameters = new Dictionary<string, object> { ["size"] = 1000 }
        };

        foreach (var strategy in strategies)
        {
            _logger.LogInformation("Executando estratégia: {StrategyName}", strategy.StrategyName);
            var result = await strategy.ProcessAsync(request);
            _logger.LogInformation("Resultado: {Result}", result.Result);
        }
    }

    private async Task DemonstrateGenericServicesAsync()
    {
        _logger.LogInformation("\n6. === Demonstrando Serviços Genéricos ===");

        // Handler genérico
        var userQueryHandler = _serviceProvider.GetRequiredService<IHandler<int, User?>>();
        
        var user = await userQueryHandler.HandleAsync(1);
        _logger.LogInformation("Handler genérico retornou usuário: {UserName}", user?.Name ?? "Não encontrado");

        // Repository genérico
        var userRepository = _serviceProvider.GetRequiredService<IUserRepository>();
        var allUsers = await userRepository.GetAllAsync();
        _logger.LogInformation("Repository genérico retornou {Count} usuários", allUsers.Count());
    }

    private void DemonstrateServiceProviderUsage()
    {
        _logger.LogInformation("\n7. === Demonstrando Uso Avançado do ServiceProvider ===");

        // Verificar se um serviço está registrado
        var hasNotificationService = _serviceProvider.GetService<INotificationService>() != null;
        _logger.LogInformation("INotificationService está registrado: {IsRegistered}", hasNotificationService);

        // Resolver múltiplas implementações
        var strategies = _serviceProvider.GetServices<IProcessingStrategy>().ToList();
        _logger.LogInformation("Encontradas {Count} estratégias de processamento", strategies.Count);
        foreach (var strategy in strategies)
        {
            _logger.LogInformation("- Estratégia: {StrategyName}", strategy.StrategyName);
        }

        // Demonstrar resolução condicional
        var scopedService = _serviceProvider.GetService<IScopedService>();
        if (scopedService != null)
        {
            _logger.LogInformation("Serviço scoped resolvido: {InstanceId}", scopedService.InstanceId);
        }
    }

    private async Task DemonstrateBackgroundServicesAsync()
    {
        _logger.LogInformation("\n8. === Demonstrando Serviços em Background ===");

        var backgroundTaskService = _serviceProvider.GetRequiredService<IBackgroundTaskService>();

        // Adicionar algumas tarefas em background
        await backgroundTaskService.QueueBackgroundWorkItemAsync(async (cancellationToken) =>
        {
            _logger.LogInformation("Executando tarefa em background 1");
            await Task.Delay(500, cancellationToken);
            _logger.LogInformation("Tarefa em background 1 concluída");
        });

        await backgroundTaskService.QueueBackgroundWorkItemAsync(async (cancellationToken) =>
        {
            _logger.LogInformation("Executando tarefa em background 2");
            await Task.Delay(300, cancellationToken);
            _logger.LogInformation("Tarefa em background 2 concluída");
        });

        await backgroundTaskService.QueueBackgroundWorkItemAsync(async (cancellationToken) =>
        {
            _logger.LogInformation("Executando tarefa em background 3");
            await Task.Delay(200, cancellationToken);
            _logger.LogInformation("Tarefa em background 3 concluída");
        });

        _logger.LogInformation("Tarefas em background foram enfileiradas");
        
        // Aguardar um pouco para ver as tarefas sendo processadas
        await Task.Delay(2000);
    }
}
