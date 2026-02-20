using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;

Console.WriteLine("🏗️ Dica 90: Microservices Patterns (.NET 9)");
Console.WriteLine("============================================");

// 1. Service Discovery Pattern
Console.WriteLine("\n1. 🔍 Service Discovery Pattern:");
Console.WriteLine("─────────────────────────────────");

await DemonstrarServiceDiscovery();

// 2. Circuit Breaker Pattern
Console.WriteLine("\n2. ⚡ Circuit Breaker Pattern:");
Console.WriteLine("─────────────────────────────────");

await DemonstrarCircuitBreaker();

// 3. Retry Pattern
Console.WriteLine("\n3. 🔄 Retry Pattern:");
Console.WriteLine("────────────────────");

await DemonstrarRetryPattern();

// 4. Bulkhead Pattern
Console.WriteLine("\n4. 🛡️ Bulkhead Pattern:");
Console.WriteLine("───────────────────────");

await DemonstrarBulkheadPattern();

// 5. Health Check Pattern
Console.WriteLine("\n5. 💓 Health Check Pattern:");
Console.WriteLine("───────────────────────────");

await DemonstrarHealthChecks();

// 6. Event Sourcing
Console.WriteLine("\n6. 📚 Event Sourcing:");
Console.WriteLine("─────────────────────");

await DemonstrarEventSourcing();

Console.WriteLine("\n✅ Demonstração completa de Microservices Patterns!");

static async Task DemonstrarServiceDiscovery()
{
    Console.WriteLine("🔍 Simulando Service Discovery:");
    
    var serviceRegistry = new ServiceRegistry();
    
    // Registrar serviços
    await serviceRegistry.RegisterServiceAsync("user-service", "http://localhost:5001", "v1.0");
    await serviceRegistry.RegisterServiceAsync("order-service", "http://localhost:5002", "v1.2");
    await serviceRegistry.RegisterServiceAsync("payment-service", "http://localhost:5003", "v2.1");
    
    // Descobrir serviços
    var userService = await serviceRegistry.DiscoverServiceAsync("user-service");
    var orderService = await serviceRegistry.DiscoverServiceAsync("order-service");
    
    Console.WriteLine($"✅ User Service descoberto: {userService?.Endpoint} (v{userService?.Version})");
    Console.WriteLine($"✅ Order Service descoberto: {orderService?.Endpoint} (v{orderService?.Version})");
    
    // Listar todos os serviços
    var allServices = await serviceRegistry.GetAllServicesAsync();
    Console.WriteLine($"✅ Total de serviços registrados: {allServices.Count}");
    
    // Health check automático
    await serviceRegistry.PerformHealthChecksAsync();
}

static async Task DemonstrarCircuitBreaker()
{
    Console.WriteLine("⚡ Demonstrando Circuit Breaker:");
    
    var circuitBreaker = new CircuitBreaker(
        failureThreshold: 3,
        recoveryTimeout: TimeSpan.FromSeconds(5)
    );
    
    // Simular várias tentativas - algumas falham
    for (int i = 1; i <= 8; i++)
    {
        try
        {
            var result = await circuitBreaker.ExecuteAsync(async () =>
            {
                Console.WriteLine($"   Tentativa {i}...");
                await Task.Delay(100);
                
                // Simular falhas nos primeiros 4 calls
                if (i <= 4)
                    throw new HttpRequestException($"Falha simulada na tentativa {i}");
                
                return $"Sucesso na tentativa {i}";
            });
            
            Console.WriteLine($"✅ {result}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ {ex.Message}");
        }
        
        await Task.Delay(200);
    }
}

static async Task DemonstrarRetryPattern()
{
    Console.WriteLine("🔄 Demonstrando Retry Pattern:");
    
    var retryPolicy = new RetryPolicy(
        maxAttempts: 3,
        backoffStrategy: BackoffStrategy.Exponential
    );
    
    // Teste 1: Operação que falha várias vezes
    Console.WriteLine("🔄 Teste 1 - Operação instável:");
    var attempt = 0;
    
    try
    {
        var result = await retryPolicy.ExecuteAsync(async () =>
        {
            attempt++;
            Console.WriteLine($"   Tentativa {attempt}...");
            await Task.Delay(50);
            
            if (attempt < 3)
                throw new TimeoutException($"Timeout na tentativa {attempt}");
            
            return "Operação concluída com sucesso!";
        });
        
        Console.WriteLine($"✅ {result}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Falha final: {ex.Message}");
    }
    
    // Teste 2: Operação que sempre falha
    Console.WriteLine("\n🔄 Teste 2 - Operação que sempre falha:");
    attempt = 0;
    
    try
    {
        await retryPolicy.ExecuteAsync<string>(async () =>
        {
            attempt++;
            Console.WriteLine($"   Tentativa {attempt}...");
            await Task.Delay(50);
            throw new InvalidOperationException("Esta operação sempre falha");
        });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Todas as tentativas falharam: {ex.Message}");
    }
}

static async Task DemonstrarBulkheadPattern()
{
    Console.WriteLine("🛡️ Demonstrando Bulkhead Pattern:");
    
    var bulkhead = new BulkheadPattern();
    
    // Pool para operações críticas (maior prioridade)
    var criticalPool = bulkhead.CreatePool("critical", maxConcurrency: 2);
    
    // Pool para operações não-críticas
    var normalPool = bulkhead.CreatePool("normal", maxConcurrency: 1);
    
    // Simular operações concorrentes
    var tasks = new List<Task>();
    
    // Operações críticas
    for (int i = 1; i <= 3; i++)
    {
        int taskId = i;
        tasks.Add(criticalPool.ExecuteAsync(async () =>
        {
            Console.WriteLine($"🔥 Operação crítica {taskId} iniciada");
            await Task.Delay(1000);
            Console.WriteLine($"✅ Operação crítica {taskId} concluída");
            return $"Critical-{taskId}";
        }));
    }
    
    // Operações normais
    for (int i = 1; i <= 3; i++)
    {
        int taskId = i;
        tasks.Add(normalPool.ExecuteAsync(async () =>
        {
            Console.WriteLine($"📝 Operação normal {taskId} iniciada");
            await Task.Delay(800);
            Console.WriteLine($"✅ Operação normal {taskId} concluída");
            return $"Normal-{taskId}";
        }));
    }
    
    await Task.WhenAll(tasks);
    Console.WriteLine("✅ Todas as operações concluídas com isolamento");
}

static async Task DemonstrarHealthChecks()
{
    Console.WriteLine("💓 Demonstrando Health Checks:");
    
    var healthChecker = new HealthCheckService();
    
    // Registrar diferentes tipos de health checks
    healthChecker.RegisterCheck("database", new DatabaseHealthCheck());
    healthChecker.RegisterCheck("cache", new CacheHealthCheck());
    healthChecker.RegisterCheck("external-api", new ExternalApiHealthCheck());
    
    // Executar health checks
    var healthReport = await healthChecker.CheckHealthAsync();
    
    Console.WriteLine($"📊 Status geral: {healthReport.Status}");
    Console.WriteLine($"📊 Tempo total: {healthReport.TotalDuration.TotalMilliseconds:F0}ms");
    
    foreach (var check in healthReport.Entries)
    {
        var emoji = check.Value.Status switch
        {
            HealthStatus.Healthy => "✅",
            HealthStatus.Degraded => "⚠️",
            HealthStatus.Unhealthy => "❌",
            _ => "❓"
        };
        
        Console.WriteLine($"{emoji} {check.Key}: {check.Value.Status} ({check.Value.Duration.TotalMilliseconds:F0}ms)");
        
        if (!string.IsNullOrEmpty(check.Value.Description))
        {
            Console.WriteLine($"   {check.Value.Description}");
        }
    }
}

static async Task DemonstrarEventSourcing()
{
    Console.WriteLine("📚 Demonstrando Event Sourcing:");
    
    var eventStore = new InMemoryEventStore();
    var orderAggregate = new OrderAggregate(eventStore);
    
    // Criar um novo pedido
    var orderId = Guid.NewGuid();
    await orderAggregate.CreateOrderAsync(orderId, "user-123", "Pedido de teste");
    
    // Adicionar itens
    await orderAggregate.AddItemAsync(orderId, "produto-1", 2, 29.99m);
    await orderAggregate.AddItemAsync(orderId, "produto-2", 1, 15.50m);
    
    // Confirmar pedido
    await orderAggregate.ConfirmOrderAsync(orderId);
    
    // Cancelar pedido
    await orderAggregate.CancelOrderAsync(orderId, "Cliente desistiu");
    
    // Recuperar estado atual
    var currentState = await orderAggregate.GetOrderStateAsync(orderId);
    Console.WriteLine($"✅ Estado atual: {currentState.Status}");
    Console.WriteLine($"✅ Total de itens: {currentState.Items.Count}");
    Console.WriteLine($"✅ Valor total: {currentState.TotalAmount:C}");
    
    // Mostrar histórico de eventos
    var events = await eventStore.GetEventsAsync(orderId);
    Console.WriteLine($"\n📜 Histórico de eventos ({events.Count} eventos):");
    
    foreach (var evt in events)
    {
        Console.WriteLine($"   {evt.Timestamp:HH:mm:ss} - {evt.EventType}: {evt.Data}");
    }
}

// Classes de suporte
public class ServiceRegistry
{
    private readonly Dictionary<string, ServiceInfo> _services = new();
    private readonly Random _random = new();
    
    public async Task RegisterServiceAsync(string name, string endpoint, string version)
    {
        await Task.Delay(50); // Simular latência de registro
        _services[name] = new ServiceInfo(name, endpoint, version, true);
        Console.WriteLine($"🔍 Serviço registrado: {name} -> {endpoint}");
    }
    
    public async Task<ServiceInfo?> DiscoverServiceAsync(string name)
    {
        await Task.Delay(20); // Simular latência de descoberta
        return _services.GetValueOrDefault(name);
    }
    
    public async Task<Dictionary<string, ServiceInfo>> GetAllServicesAsync()
    {
        await Task.Delay(30);
        return new Dictionary<string, ServiceInfo>(_services);
    }
    
    public async Task PerformHealthChecksAsync()
    {
        Console.WriteLine("🔍 Executando health checks dos serviços...");
        
        foreach (var service in _services.Values)
        {
            await Task.Delay(100); // Simular health check
            service.IsHealthy = _random.NextDouble() > 0.2; // 80% chance de sucesso
            
            var status = service.IsHealthy ? "✅ Saudável" : "❌ Não saudável";
            Console.WriteLine($"   {service.Name}: {status}");
        }
    }
}

public class ServiceInfo
{
    public string Name { get; }
    public string Endpoint { get; }
    public string Version { get; }
    public bool IsHealthy { get; set; }
    
    public ServiceInfo(string name, string endpoint, string version, bool isHealthy)
    {
        Name = name;
        Endpoint = endpoint;
        Version = version;
        IsHealthy = isHealthy;
    }
}

public class CircuitBreaker
{
    private readonly int _failureThreshold;
    private readonly TimeSpan _recoveryTimeout;
    private int _failureCount;
    private DateTime _lastFailureTime;
    private CircuitState _state = CircuitState.Closed;
    
    public CircuitBreaker(int failureThreshold, TimeSpan recoveryTimeout)
    {
        _failureThreshold = failureThreshold;
        _recoveryTimeout = recoveryTimeout;
    }
    
    public async Task<T> ExecuteAsync<T>(Func<Task<T>> operation)
    {
        if (_state == CircuitState.Open)
        {
            if (DateTime.UtcNow - _lastFailureTime < _recoveryTimeout)
            {
                throw new InvalidOperationException("Circuit breaker is OPEN - calls blocked");
            }
            
            _state = CircuitState.HalfOpen;
            Console.WriteLine("⚡ Circuit breaker: OPEN -> HALF-OPEN");
        }
        
        try
        {
            var result = await operation();
            
            if (_state == CircuitState.HalfOpen)
            {
                _state = CircuitState.Closed;
                _failureCount = 0;
                Console.WriteLine("⚡ Circuit breaker: HALF-OPEN -> CLOSED (recuperado)");
            }
            
            return result;
        }
        catch (Exception)
        {
            _failureCount++;
            _lastFailureTime = DateTime.UtcNow;
            
            if (_failureCount >= _failureThreshold)
            {
                _state = CircuitState.Open;
                Console.WriteLine($"⚡ Circuit breaker: FECHADO -> ABERTO (falhas: {_failureCount})");
            }
            
            throw;
        }
    }
}

public enum CircuitState
{
    Closed,
    Open,
    HalfOpen
}

public class RetryPolicy
{
    private readonly int _maxAttempts;
    private readonly BackoffStrategy _backoffStrategy;
    
    public RetryPolicy(int maxAttempts, BackoffStrategy backoffStrategy)
    {
        _maxAttempts = maxAttempts;
        _backoffStrategy = backoffStrategy;
    }
    
    public async Task<T> ExecuteAsync<T>(Func<Task<T>> operation)
    {
        var attempt = 0;
        
        while (true)
        {
            attempt++;
            
            try
            {
                return await operation();
            }
            catch (Exception) when (attempt < _maxAttempts)
            {
                var delay = CalculateDelay(attempt);
                Console.WriteLine($"   Aguardando {delay.TotalMilliseconds:F0}ms antes da próxima tentativa...");
                await Task.Delay(delay);
            }
        }
    }
    
    private TimeSpan CalculateDelay(int attempt)
    {
        return _backoffStrategy switch
        {
            BackoffStrategy.Fixed => TimeSpan.FromMilliseconds(1000),
            BackoffStrategy.Linear => TimeSpan.FromMilliseconds(1000 * attempt),
            BackoffStrategy.Exponential => TimeSpan.FromMilliseconds(1000 * Math.Pow(2, attempt - 1)),
            _ => TimeSpan.FromMilliseconds(1000)
        };
    }
}

public enum BackoffStrategy
{
    Fixed,
    Linear,
    Exponential
}

public class BulkheadPattern
{
    private readonly Dictionary<string, ResourcePool> _pools = new();
    
    public ResourcePool CreatePool(string name, int maxConcurrency)
    {
        var pool = new ResourcePool(name, maxConcurrency);
        _pools[name] = pool;
        Console.WriteLine($"🛡️ Pool '{name}' criado com {maxConcurrency} slots");
        return pool;
    }
}

public class ResourcePool
{
    private readonly string _name;
    private readonly SemaphoreSlim _semaphore;
    
    public ResourcePool(string name, int maxConcurrency)
    {
        _name = name;
        _semaphore = new SemaphoreSlim(maxConcurrency, maxConcurrency);
    }
    
    public async Task<T> ExecuteAsync<T>(Func<Task<T>> operation)
    {
        Console.WriteLine($"🛡️ Aguardando slot no pool '{_name}'...");
        await _semaphore.WaitAsync();
        
        try
        {
            Console.WriteLine($"🛡️ Executando no pool '{_name}'");
            return await operation();
        }
        finally
        {
            _semaphore.Release();
            Console.WriteLine($"🛡️ Slot liberado no pool '{_name}'");
        }
    }
}

public class HealthCheckService
{
    private readonly Dictionary<string, IHealthCheck> _checks = new();
    
    public void RegisterCheck(string name, IHealthCheck healthCheck)
    {
        _checks[name] = healthCheck;
        Console.WriteLine($"💓 Health check registrado: {name}");
    }
    
    public async Task<HealthReport> CheckHealthAsync()
    {
        var startTime = DateTime.UtcNow;
        var entries = new Dictionary<string, HealthCheckResult>();
        var overallStatus = HealthStatus.Healthy;
        
        foreach (var check in _checks)
        {
            var checkStart = DateTime.UtcNow;
            var result = await check.Value.CheckHealthAsync();
            var checkDuration = DateTime.UtcNow - checkStart;
            
            result.Duration = checkDuration;
            entries[check.Key] = result;
            
            if (result.Status == HealthStatus.Unhealthy)
                overallStatus = HealthStatus.Unhealthy;
            else if (result.Status == HealthStatus.Degraded && overallStatus == HealthStatus.Healthy)
                overallStatus = HealthStatus.Degraded;
        }
        
        var totalDuration = DateTime.UtcNow - startTime;
        
        return new HealthReport
        {
            Status = overallStatus,
            TotalDuration = totalDuration,
            Entries = entries
        };
    }
}

public interface IHealthCheck
{
    Task<HealthCheckResult> CheckHealthAsync();
}

public class DatabaseHealthCheck : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync()
    {
        await Task.Delay(150); // Simular verificação de DB
        
        return new HealthCheckResult
        {
            Status = HealthStatus.Healthy,
            Description = "Database connection is healthy"
        };
    }
}

public class CacheHealthCheck : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync()
    {
        await Task.Delay(80); // Simular verificação de cache
        
        return new HealthCheckResult
        {
            Status = HealthStatus.Degraded,
            Description = "Cache is slow but functional"
        };
    }
}

public class ExternalApiHealthCheck : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync()
    {
        await Task.Delay(200); // Simular verificação de API externa
        
        return new HealthCheckResult
        {
            Status = HealthStatus.Unhealthy,
            Description = "External API is not responding"
        };
    }
}

public class HealthReport
{
    public HealthStatus Status { get; set; }
    public TimeSpan TotalDuration { get; set; }
    public Dictionary<string, HealthCheckResult> Entries { get; set; } = new();
}

public class HealthCheckResult
{
    public HealthStatus Status { get; set; }
    public string Description { get; set; } = string.Empty;
    public TimeSpan Duration { get; set; }
}

public enum HealthStatus
{
    Healthy,
    Degraded,
    Unhealthy
}

public class InMemoryEventStore
{
    private readonly Dictionary<Guid, List<EventRecord>> _events = new();
    
    public async Task AppendEventAsync(Guid aggregateId, string eventType, object data)
    {
        await Task.Delay(10); // Simular persistência
        
        if (!_events.ContainsKey(aggregateId))
            _events[aggregateId] = new List<EventRecord>();
        
        var evt = new EventRecord
        {
            Id = Guid.NewGuid(),
            AggregateId = aggregateId,
            EventType = eventType,
            Data = JsonSerializer.Serialize(data),
            Timestamp = DateTime.UtcNow
        };
        
        _events[aggregateId].Add(evt);
        Console.WriteLine($"📚 Evento salvo: {eventType}");
    }
    
    public async Task<List<EventRecord>> GetEventsAsync(Guid aggregateId)
    {
        await Task.Delay(20); // Simular leitura
        return _events.GetValueOrDefault(aggregateId) ?? new List<EventRecord>();
    }
}

public class EventRecord
{
    public Guid Id { get; set; }
    public Guid AggregateId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string Data { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}

public class OrderAggregate
{
    private readonly InMemoryEventStore _eventStore;
    
    public OrderAggregate(InMemoryEventStore eventStore)
    {
        _eventStore = eventStore;
    }
    
    public async Task CreateOrderAsync(Guid orderId, string userId, string description)
    {
        await _eventStore.AppendEventAsync(orderId, "OrderCreated", new
        {
            OrderId = orderId,
            UserId = userId,
            Description = description
        });
    }
    
    public async Task AddItemAsync(Guid orderId, string productId, int quantity, decimal price)
    {
        await _eventStore.AppendEventAsync(orderId, "ItemAdded", new
        {
            ProductId = productId,
            Quantity = quantity,
            Price = price
        });
    }
    
    public async Task ConfirmOrderAsync(Guid orderId)
    {
        await _eventStore.AppendEventAsync(orderId, "OrderConfirmed", new
        {
            ConfirmedAt = DateTime.UtcNow
        });
    }
    
    public async Task CancelOrderAsync(Guid orderId, string reason)
    {
        await _eventStore.AppendEventAsync(orderId, "OrderCancelled", new
        {
            Reason = reason,
            CancelledAt = DateTime.UtcNow
        });
    }
    
    public async Task<OrderState> GetOrderStateAsync(Guid orderId)
    {
        var events = await _eventStore.GetEventsAsync(orderId);
        var state = new OrderState();
        
        foreach (var evt in events)
        {
            ApplyEvent(state, evt);
        }
        
        return state;
    }
    
    private void ApplyEvent(OrderState state, EventRecord evt)
    {
        switch (evt.EventType)
        {
            case "OrderCreated":
                var created = JsonSerializer.Deserialize<dynamic>(evt.Data);
                state.Status = "Created";
                break;
                
            case "ItemAdded":
                var item = JsonSerializer.Deserialize<JsonElement>(evt.Data);
                var price = item.GetProperty("Price").GetDecimal();
                var quantity = item.GetProperty("Quantity").GetInt32();
                
                state.Items.Add(new OrderItem
                {
                    ProductId = item.GetProperty("ProductId").GetString() ?? "",
                    Quantity = quantity,
                    Price = price
                });
                state.TotalAmount += price * quantity;
                break;
                
            case "OrderConfirmed":
                state.Status = "Confirmed";
                break;
                
            case "OrderCancelled":
                state.Status = "Cancelled";
                break;
        }
    }
}

public class OrderState
{
    public string Status { get; set; } = "Unknown";
    public List<OrderItem> Items { get; set; } = new();
    public decimal TotalAmount { get; set; }
}

public class OrderItem
{
    public string ProductId { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Price { get; set; }
}
