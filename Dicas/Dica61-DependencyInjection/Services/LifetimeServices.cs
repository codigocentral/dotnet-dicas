using Microsoft.Extensions.Logging;
using Dica61.DependencyInjection.Interfaces;
using System.Text.Json;

namespace Dica61.DependencyInjection.Services;

// Demonstração de diferentes lifetimes

// Serviço Scoped - Uma instância por scope/request
public class ScopedService : IScopedService
{
    public Guid InstanceId { get; } = Guid.NewGuid();
    public DateTime CreatedAt { get; } = DateTime.Now;
    private readonly ILogger<ScopedService> _logger;

    public ScopedService(ILogger<ScopedService> logger)
    {
        _logger = logger;
        _logger.LogDebug("ScopedService criado: {InstanceId} em {CreatedAt}", InstanceId, CreatedAt);
    }

    public async Task<string> ProcessScopedOperationAsync()
    {
        _logger.LogInformation("Processando operação scoped: {InstanceId}", InstanceId);
        await Task.Delay(100); // Simular processamento
        return $"Operação processada pelo ScopedService {InstanceId}";
    }
}

// Serviço Singleton - Uma única instância durante toda a vida da aplicação
public class SingletonService : ISingletonService
{
    public Guid InstanceId { get; } = Guid.NewGuid();
    public DateTime CreatedAt { get; } = DateTime.Now;
    public int RequestCount { get; private set; }
    private readonly ILogger<SingletonService> _logger;
    private readonly object _lock = new object();

    public SingletonService(ILogger<SingletonService> logger)
    {
        _logger = logger;
        _logger.LogInformation("SingletonService criado: {InstanceId} em {CreatedAt}", InstanceId, CreatedAt);
    }

    public void IncrementRequestCount()
    {
        lock (_lock)
        {
            RequestCount++;
            _logger.LogDebug("RequestCount incrementado para: {RequestCount}", RequestCount);
        }
    }
}

// Serviço Transient - Nova instância a cada injeção
public class TransientService : ITransientService
{
    public Guid InstanceId { get; } = Guid.NewGuid();
    public DateTime CreatedAt { get; } = DateTime.Now;
    private readonly ILogger<TransientService> _logger;

    public TransientService(ILogger<TransientService> logger)
    {
        _logger = logger;
        _logger.LogDebug("TransientService criado: {InstanceId} em {CreatedAt}", InstanceId, CreatedAt);
    }

    public async Task<string> ProcessTransientOperationAsync()
    {
        _logger.LogInformation("Processando operação transient: {InstanceId}", InstanceId);
        await Task.Delay(50); // Simular processamento
        return $"Operação processada pelo TransientService {InstanceId}";
    }
}

// Serviço de API externa
public class ExternalApiService : IExternalApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ExternalApiService> _logger;

    public ExternalApiService(HttpClient httpClient, ILogger<ExternalApiService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<T?> GetAsync<T>(string endpoint) where T : class
    {
        try
        {
            _logger.LogInformation("Fazendo GET para: {Endpoint}", endpoint);
            
            var response = await _httpClient.GetAsync(endpoint);
            response.EnsureSuccessStatusCode();
            
            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions 
            { 
                PropertyNameCaseInsensitive = true 
            });
            
            _logger.LogInformation("GET bem-sucedido para: {Endpoint}", endpoint);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao fazer GET para: {Endpoint}", endpoint);
            throw;
        }
    }

    public async Task<T?> PostAsync<T>(string endpoint, object data) where T : class
    {
        try
        {
            _logger.LogInformation("Fazendo POST para: {Endpoint}", endpoint);
            
            var json = JsonSerializer.Serialize(data);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync(endpoint, content);
            response.EnsureSuccessStatusCode();
            
            var responseJson = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<T>(responseJson, new JsonSerializerOptions 
            { 
                PropertyNameCaseInsensitive = true 
            });
            
            _logger.LogInformation("POST bem-sucedido para: {Endpoint}", endpoint);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao fazer POST para: {Endpoint}", endpoint);
            throw;
        }
    }
}

// Serviço de trabalho em background
public class BackgroundTaskService : IBackgroundTaskService
{
    private readonly ILogger<BackgroundTaskService> _logger;
    private readonly Queue<Func<CancellationToken, Task>> _workItems = new();
    private readonly object _lock = new object();

    public BackgroundTaskService(ILogger<BackgroundTaskService> logger)
    {
        _logger = logger;
        
        // Simular processamento em background
        _ = Task.Run(ProcessWorkItemsAsync);
    }

    public Task QueueBackgroundWorkItemAsync(Func<CancellationToken, Task> workItem)
    {
        lock (_lock)
        {
            _workItems.Enqueue(workItem);
            _logger.LogInformation("Item de trabalho adicionado à fila. Total na fila: {QueueSize}", _workItems.Count);
        }
        
        return Task.CompletedTask;
    }

    private async Task ProcessWorkItemsAsync()
    {
        _logger.LogInformation("Processador de trabalho em background iniciado");
        
        while (true)
        {
            Func<CancellationToken, Task>? workItem = null;
            
            lock (_lock)
            {
                if (_workItems.Count > 0)
                {
                    workItem = _workItems.Dequeue();
                }
            }
            
            if (workItem != null)
            {
                try
                {
                    _logger.LogDebug("Processando item de trabalho em background");
                    await workItem(CancellationToken.None);
                    _logger.LogDebug("Item de trabalho processado com sucesso");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao processar item de trabalho em background");
                }
            }
            else
            {
                await Task.Delay(1000); // Aguardar por novos itens
            }
        }
    }
}

// Estratégias de processamento (demonstrando o padrão Strategy com DI)
public class FastProcessingStrategy : IProcessingStrategy
{
    public string StrategyName => "Fast";
    private readonly ILogger<FastProcessingStrategy> _logger;

    public FastProcessingStrategy(ILogger<FastProcessingStrategy> logger)
    {
        _logger = logger;
    }

    public async Task<ProcessingResult> ProcessAsync(ProcessingRequest request)
    {
        _logger.LogInformation("Processando com estratégia rápida: {RequestType}", request.Type);
        
        await Task.Delay(100); // Processamento rápido
        
        return new ProcessingResult
        {
            Success = true,
            Result = $"Processamento rápido concluído para {request.Type}",
            ProcessedBy = StrategyName
        };
    }
}

public class SlowProcessingStrategy : IProcessingStrategy
{
    public string StrategyName => "Slow";
    private readonly ILogger<SlowProcessingStrategy> _logger;

    public SlowProcessingStrategy(ILogger<SlowProcessingStrategy> logger)
    {
        _logger = logger;
    }

    public async Task<ProcessingResult> ProcessAsync(ProcessingRequest request)
    {
        _logger.LogInformation("Processando com estratégia lenta: {RequestType}", request.Type);
        
        await Task.Delay(500); // Processamento mais demorado
        
        return new ProcessingResult
        {
            Success = true,
            Result = $"Processamento detalhado concluído para {request.Type}",
            ProcessedBy = StrategyName
        };
    }
}

// Handler genérico
public class UserQueryHandler : IHandler<int, User?>
{
    private readonly IUserService _userService;
    private readonly ILogger<UserQueryHandler> _logger;

    public UserQueryHandler(IUserService userService, ILogger<UserQueryHandler> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    public async Task<User?> HandleAsync(int request)
    {
        _logger.LogInformation("Processando consulta de usuário: {UserId}", request);
        
        var user = await _userService.GetUserAsync(request);
        
        _logger.LogInformation("Consulta de usuário processada: {UserId}, Encontrado: {Found}", 
            request, user != null);
        
        return user;
    }
}

// Decorator para logging (demonstrando o padrão Decorator com DI)
public class LoggingUserServiceDecorator : IUserService
{
    private readonly IUserService _inner;
    private readonly ILogger<LoggingUserServiceDecorator> _logger;

    public LoggingUserServiceDecorator(IUserService inner, ILogger<LoggingUserServiceDecorator> logger)
    {
        _inner = inner;
        _logger = logger;
    }

    public async Task<User?> GetUserAsync(int id)
    {
        _logger.LogInformation("DECORATOR: Iniciando GetUserAsync para ID: {UserId}", id);
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        try
        {
            var result = await _inner.GetUserAsync(id);
            stopwatch.Stop();
            
            _logger.LogInformation("DECORATOR: GetUserAsync concluído em {ElapsedMs}ms para ID: {UserId}, Encontrado: {Found}", 
                stopwatch.ElapsedMilliseconds, id, result != null);
            
            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "DECORATOR: Erro em GetUserAsync após {ElapsedMs}ms para ID: {UserId}", 
                stopwatch.ElapsedMilliseconds, id);
            throw;
        }
    }

    public async Task<User> CreateUserAsync(string name, string email)
    {
        _logger.LogInformation("DECORATOR: Iniciando CreateUserAsync para: {Name}, {Email}", name, email);
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        try
        {
            var result = await _inner.CreateUserAsync(name, email);
            stopwatch.Stop();
            
            _logger.LogInformation("DECORATOR: CreateUserAsync concluído em {ElapsedMs}ms, ID criado: {UserId}", 
                stopwatch.ElapsedMilliseconds, result.Id);
            
            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "DECORATOR: Erro em CreateUserAsync após {ElapsedMs}ms", 
                stopwatch.ElapsedMilliseconds);
            throw;
        }
    }

    public async Task<User> UpdateUserAsync(int id, string name, string email)
    {
        _logger.LogInformation("DECORATOR: Iniciando UpdateUserAsync para ID: {UserId}", id);
        var result = await _inner.UpdateUserAsync(id, name, email);
        _logger.LogInformation("DECORATOR: UpdateUserAsync concluído para ID: {UserId}", id);
        return result;
    }

    public async Task DeleteUserAsync(int id)
    {
        _logger.LogInformation("DECORATOR: Iniciando DeleteUserAsync para ID: {UserId}", id);
        await _inner.DeleteUserAsync(id);
        _logger.LogInformation("DECORATOR: DeleteUserAsync concluído para ID: {UserId}", id);
    }

    public async Task<IEnumerable<User>> GetActiveUsersAsync()
    {
        _logger.LogInformation("DECORATOR: Iniciando GetActiveUsersAsync");
        var result = await _inner.GetActiveUsersAsync();
        _logger.LogInformation("DECORATOR: GetActiveUsersAsync concluído, {Count} usuários encontrados", result.Count());
        return result;
    }
}
