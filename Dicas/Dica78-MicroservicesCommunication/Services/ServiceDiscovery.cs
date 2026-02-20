using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Text.Json;

namespace Dica78.MicroservicesCommunication.Services;

public class ServiceRegistry
{
    private readonly ConcurrentDictionary<string, List<ServiceInstance>> _services = new();
    private readonly ILogger<ServiceRegistry> _logger;
    private readonly Timer _healthCheckTimer;

    public ServiceRegistry(ILogger<ServiceRegistry> logger)
    {
        _logger = logger;
        _healthCheckTimer = new Timer(PerformHealthChecks, null, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(30));
        SeedInitialServices();
    }

    private void SeedInitialServices()
    {
        // Registrar alguns serviços de exemplo
        RegisterService("user-service", "localhost", 5001, new { version = "1.0", region = "us-east" });
        RegisterService("user-service", "localhost", 5002, new { version = "1.0", region = "us-east" });
        RegisterService("order-service", "localhost", 5003, new { version = "2.1", region = "us-east" });
        RegisterService("order-service", "localhost", 5004, new { version = "2.1", region = "us-west" });
        RegisterService("payment-service", "localhost", 5005, new { version = "1.5", region = "us-east" });
        RegisterService("notification-service", "localhost", 5006, new { version = "1.2", region = "us-east" });
    }

    public void RegisterService(string serviceName, string host, int port, object? metadata = null)
    {
        var instance = new ServiceInstance
        {
            Id = Guid.NewGuid().ToString(),
            ServiceName = serviceName,
            Host = host,
            Port = port,
            Metadata = metadata != null ? JsonSerializer.Serialize(metadata) : string.Empty,
            RegisteredAt = DateTime.UtcNow,
            LastHealthCheck = DateTime.UtcNow,
            IsHealthy = true
        };

        _services.AddOrUpdate(serviceName,
            [instance],
            (key, existing) =>
            {
                existing.Add(instance);
                return existing;
            });

        _logger.LogInformation("📋 Serviço registrado: {ServiceName} em {Host}:{Port}", 
            serviceName, host, port);
    }

    public void DeregisterService(string serviceName, string instanceId)
    {
        if (_services.TryGetValue(serviceName, out var instances))
        {
            var toRemove = instances.FirstOrDefault(i => i.Id == instanceId);
            if (toRemove != null)
            {
                instances.Remove(toRemove);
                _logger.LogInformation("📋 Serviço removido: {ServiceName} - {InstanceId}", 
                    serviceName, instanceId);
            }
        }
    }

    public ServiceInstance? DiscoverService(string serviceName, string? region = null)
    {
        if (!_services.TryGetValue(serviceName, out var instances))
            return null;

        var healthyInstances = instances.Where(i => i.IsHealthy).ToList();
        
        if (region != null)
        {
            var regionalInstances = healthyInstances.Where(i => 
                i.Metadata.Contains($"\"region\":\"{region}\"")).ToList();
            
            if (regionalInstances.Any())
                healthyInstances = regionalInstances;
        }

        if (!healthyInstances.Any())
            return null;

        // Load balancing simples (round-robin baseado em timestamp)
        var selectedIndex = (int)(DateTime.UtcNow.Ticks % healthyInstances.Count);
        return healthyInstances[selectedIndex];
    }

    public List<ServiceInstance> GetAllInstances(string serviceName)
    {
        return _services.TryGetValue(serviceName, out var instances) 
            ? instances.ToList() 
            : new List<ServiceInstance>();
    }

    public Dictionary<string, List<ServiceInstance>> GetAllServices()
    {
        return _services.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToList());
    }

    private void PerformHealthChecks(object? state)
    {
        foreach (var service in _services)
        {
            foreach (var instance in service.Value)
            {
                // Simula health check - em implementação real, faria HTTP call
                var isHealthy = SimulateHealthCheck(instance);
                instance.IsHealthy = isHealthy;
                instance.LastHealthCheck = DateTime.UtcNow;

                if (!isHealthy)
                {
                    _logger.LogWarning("⚠️ Health check falhou: {ServiceName} - {InstanceId}", 
                        service.Key, instance.Id);
                }
            }
        }
    }

    private bool SimulateHealthCheck(ServiceInstance instance)
    {
        // Simula 10% de chance de falha no health check
        return Random.Shared.NextDouble() > 0.1;
    }

    public void Dispose()
    {
        _healthCheckTimer?.Dispose();
    }
}

public class ServiceDiscovery
{
    private readonly ServiceRegistry _serviceRegistry;
    private readonly ILogger<ServiceDiscovery> _logger;

    public ServiceDiscovery(ServiceRegistry serviceRegistry, ILogger<ServiceDiscovery> logger)
    {
        _serviceRegistry = serviceRegistry;
        _logger = logger;
    }

    public async Task DemonstrateServiceDiscovery()
    {
        Console.WriteLine("🔍 Service Discovery Patterns");
        Console.WriteLine("=============================");

        await DemonstrateServiceRegistration();
        await DemonstrateServiceLookup();
        await DemonstrateLoadBalancing();
        await DemonstrateRegionalRouting();
        await DemonstrateHealthChecks();
    }

    private async Task DemonstrateServiceRegistration()
    {
        Console.WriteLine("\n📋 1. Service Registration");
        Console.WriteLine("-------------------------");

        // Registrar novos serviços dinamicamente
        _serviceRegistry.RegisterService("inventory-service", "localhost", 5007, 
            new { version = "1.0", region = "us-west", capabilities = new[] { "read", "write" } });

        _serviceRegistry.RegisterService("inventory-service", "localhost", 5008, 
            new { version = "1.1", region = "eu-west", capabilities = new[] { "read", "write", "analytics" } });

        Console.WriteLine("✅ Novos serviços registrados");
        await Task.Delay(100);
    }

    private async Task DemonstrateServiceLookup()
    {
        Console.WriteLine("\n🔍 2. Service Lookup");
        Console.WriteLine("-------------------");

        var services = new[] { "user-service", "order-service", "payment-service", "inventory-service" };

        foreach (var serviceName in services)
        {
            var instance = _serviceRegistry.DiscoverService(serviceName);
            if (instance != null)
            {
                Console.WriteLine($"✅ {serviceName}: {instance.Host}:{instance.Port} (Healthy: {instance.IsHealthy})");
            }
            else
            {
                Console.WriteLine($"❌ {serviceName}: Nenhuma instância disponível");
            }
        }

        await Task.Delay(100);
    }

    private async Task DemonstrateLoadBalancing()
    {
        Console.WriteLine("\n⚖️  3. Load Balancing");
        Console.WriteLine("--------------------");

        Console.WriteLine("Fazendo 5 chamadas para user-service (observe a distribuição):");
        
        for (int i = 1; i <= 5; i++)
        {
            var instance = _serviceRegistry.DiscoverService("user-service");
            if (instance != null)
            {
                Console.WriteLine($"  Request {i} → {instance.Host}:{instance.Port} (ID: {instance.Id[..8]})");
                await SimulateServiceCall(instance);
            }
            else
            {
                Console.WriteLine($"  Request {i} → Nenhuma instância disponível");
            }
            
            await Task.Delay(200);
        }
    }

    private async Task DemonstrateRegionalRouting()
    {
        Console.WriteLine("\n🌍 4. Regional Routing");
        Console.WriteLine("---------------------");

        var regions = new[] { "us-east", "us-west", "eu-west" };

        foreach (var region in regions)
        {
            Console.WriteLine($"\nRoteamento para região {region}:");
            
            var orderInstance = _serviceRegistry.DiscoverService("order-service", region);
            if (orderInstance != null)
            {
                Console.WriteLine($"  ✅ order-service: {orderInstance.Host}:{orderInstance.Port}");
            }
            else
            {
                Console.WriteLine($"  ❌ order-service: Nenhuma instância em {region}");
            }

            var inventoryInstance = _serviceRegistry.DiscoverService("inventory-service", region);
            if (inventoryInstance != null)
            {
                Console.WriteLine($"  ✅ inventory-service: {inventoryInstance.Host}:{inventoryInstance.Port}");
            }
            else
            {
                Console.WriteLine($"  ❌ inventory-service: Nenhuma instância em {region}");
            }
        }

        await Task.Delay(100);
    }

    private async Task DemonstrateHealthChecks()
    {
        Console.WriteLine("\n💚 5. Health Checks");
        Console.WriteLine("------------------");

        var allServices = _serviceRegistry.GetAllServices();

        foreach (var service in allServices)
        {
            Console.WriteLine($"\n{service.Key}:");
            foreach (var instance in service.Value)
            {
                var healthStatus = instance.IsHealthy ? "🟢 Healthy" : "🔴 Unhealthy";
                var timeSinceCheck = DateTime.UtcNow - instance.LastHealthCheck;
                
                Console.WriteLine($"  {instance.Host}:{instance.Port} - {healthStatus} " +
                    $"(Último check: {timeSinceCheck.TotalSeconds:F0}s atrás)");
            }
        }

        await Task.Delay(100);
    }

    private async Task SimulateServiceCall(ServiceInstance instance)
    {
        // Simula uma chamada de serviço
        await Task.Delay(Random.Shared.Next(50, 150));
        
        // Simula 5% de chance de falha
        if (Random.Shared.NextDouble() < 0.05)
        {
            throw new Exception($"Falha na comunicação com {instance.ServiceName}");
        }
    }
}

public class ServiceInstance
{
    public string Id { get; set; } = string.Empty;
    public string ServiceName { get; set; } = string.Empty;
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; }
    public string Metadata { get; set; } = string.Empty;
    public DateTime RegisteredAt { get; set; }
    public DateTime LastHealthCheck { get; set; }
    public bool IsHealthy { get; set; }
    public string Endpoint => $"http://{Host}:{Port}";
}

public class ServiceDiscoveryClient
{
    private readonly ServiceRegistry _serviceRegistry;
    private readonly ILogger<ServiceDiscoveryClient> _logger;
    private readonly Dictionary<string, ServiceInstance?> _cache = new();
    private readonly Timer _cacheRefreshTimer;

    public ServiceDiscoveryClient(ServiceRegistry serviceRegistry, ILogger<ServiceDiscoveryClient> logger)
    {
        _serviceRegistry = serviceRegistry;
        _logger = logger;
        _cacheRefreshTimer = new Timer(RefreshCache, null, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(10));
    }

    public async Task<string> GetServiceEndpoint(string serviceName, string? region = null)
    {
        var cacheKey = $"{serviceName}:{region ?? "any"}";
        
        if (_cache.TryGetValue(cacheKey, out var cachedInstance) && 
            cachedInstance != null && cachedInstance.IsHealthy)
        {
            _logger.LogDebug("📋 Usando instância do cache: {ServiceName}", serviceName);
            return cachedInstance.Endpoint;
        }

        var instance = _serviceRegistry.DiscoverService(serviceName, region);
        if (instance != null)
        {
            _cache[cacheKey] = instance;
            _logger.LogInformation("🔍 Novo endpoint descoberto: {ServiceName} → {Endpoint}", 
                serviceName, instance.Endpoint);
            return instance.Endpoint;
        }

        throw new ServiceDiscoveryException($"Nenhuma instância saudável encontrada para {serviceName}");
    }

    private void RefreshCache(object? state)
    {
        var keysToRemove = new List<string>();
        
        foreach (var kvp in _cache.ToList())
        {
            if (kvp.Value == null || !kvp.Value.IsHealthy)
            {
                keysToRemove.Add(kvp.Key);
            }
        }

        foreach (var key in keysToRemove)
        {
            _cache.Remove(key);
            _logger.LogDebug("🗑️ Removido do cache: {CacheKey}", key);
        }
    }

    public void Dispose()
    {
        _cacheRefreshTimer?.Dispose();
    }
}

public class ServiceDiscoveryException : Exception
{
    public ServiceDiscoveryException(string message) : base(message) { }
    public ServiceDiscoveryException(string message, Exception innerException) : base(message, innerException) { }
}
