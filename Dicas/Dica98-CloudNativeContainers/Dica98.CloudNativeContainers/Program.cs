using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;

Console.WriteLine("☁️ Dica 98: Cloud Native & Containers (.NET 9)");
Console.WriteLine("================================================");

// 1. Container Environment Detection
Console.WriteLine("\n1. 🐳 Container Environment Detection:");
Console.WriteLine("─────────────────────────────────────");

DetectarAmbienteContainer();

// 2. Environment Variables & Configuration
Console.WriteLine("\n2. ⚙️ Environment Variables & Configuration:");
Console.WriteLine("───────────────────────────────────────────");

DemonstrarConfiguracao();

// 3. Health Checks
Console.WriteLine("\n3. 💚 Health Checks:");
Console.WriteLine("──────────────────");

await DemonstrarHealthChecks();

// 4. Graceful Shutdown
Console.WriteLine("\n4. 🛑 Graceful Shutdown:");
Console.WriteLine("───────────────────────");

await DemonstrarGracefulShutdown();

// 5. Resource Monitoring
Console.WriteLine("\n5. 📊 Resource Monitoring:");
Console.WriteLine("─────────────────────────");

MonitorarRecursos();

// 6. Logging for Containers
Console.WriteLine("\n6. 📝 Logging for Containers:");
Console.WriteLine("────────────────────────────");

DemonstrarContainerLogging();

// 7. Service Discovery Simulation
Console.WriteLine("\n7. 🔍 Service Discovery:");
Console.WriteLine("───────────────────────");

await SimularServiceDiscovery();

// 8. Cloud Native Patterns
Console.WriteLine("\n8. ☁️ Cloud Native Patterns:");
Console.WriteLine("───────────────────────────");

DemonstrarCloudNativePatterns();

Console.WriteLine("\n✅ Demonstração completa de Cloud Native & Containers!");

static void DetectarAmbienteContainer()
{
    // Verificar se está executando em container
    var isContainer = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";
    var isKubernetes = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("KUBERNETES_SERVICE_HOST"));
    var isDocker = File.Exists("/.dockerenv");
    
    Console.WriteLine($"🐳 Executando em container: {isContainer}");
    Console.WriteLine($"⚓ Executando em Kubernetes: {isKubernetes}");
    Console.WriteLine($"🐋 Docker detectado: {isDocker}");
    
    // Informações do hostname (útil em containers)
    Console.WriteLine($"🖥️ Hostname: {Environment.MachineName}");
    Console.WriteLine($"💻 OS: {Environment.OSVersion}");
    Console.WriteLine($"🔧 Framework: {Environment.Version}");
    
    // Process ID (útil para debugging em containers)
    using var process = System.Diagnostics.Process.GetCurrentProcess();
    Console.WriteLine($"🆔 Process ID: {process.Id}");
    Console.WriteLine($"👤 User: {Environment.UserName}");
    
    // Container-specific paths
    var containerPaths = new[]
    {
        "/app",
        "/tmp", 
        "/var/log",
        "/etc/hostname",
        "/proc/self/cgroup"
    };
    
    Console.WriteLine("\n📁 Container paths:");
    foreach (var path in containerPaths)
    {
        var exists = Directory.Exists(path) || File.Exists(path);
        Console.WriteLine($"   {(exists ? "✅" : "❌")} {path}");
    }
}

static void DemonstrarConfiguracao()
{
    // Simular configurações típicas de container
    var configBuilder = new ConfigurationBuilder();
    
    // Environment variables (padrão em containers)
    var envVars = new Dictionary<string, string?>
    {
        ["APP_NAME"] = "MeuApp",
        ["APP_VERSION"] = "1.0.0",
        ["ENVIRONMENT"] = "Production",
        ["DATABASE_URL"] = "postgresql://user:pass@db:5432/myapp",
        ["REDIS_URL"] = "redis://redis:6379",
        ["LOG_LEVEL"] = "Information",
        ["PORT"] = "8080",
        ["ASPNETCORE_URLS"] = "http://+:8080"
    };
    
    foreach (var kvp in envVars)
    {
        Environment.SetEnvironmentVariable(kvp.Key, kvp.Value);
    }
    
    var config = configBuilder
        .AddEnvironmentVariables()
        .Build();
    
    Console.WriteLine("🔧 Configurações detectadas:");
    foreach (var env in envVars)
    {
        var value = config[env.Key];
        Console.WriteLine($"   {env.Key}: {value}");
    }
    
    // Demonstrar configuração hierárquica
    Console.WriteLine("\n📊 Fontes de configuração (ordem de precedência):");
    Console.WriteLine("   1. Argumentos da linha de comando");
    Console.WriteLine("   2. Variáveis de ambiente");
    Console.WriteLine("   3. Arquivos de configuração");
    Console.WriteLine("   4. Valores padrão");
    
    // Secrets management (simulado)
    Console.WriteLine("\n🔐 Secrets Management:");
    Console.WriteLine("   📁 /var/secrets/ (Kubernetes secrets)");
    Console.WriteLine("   🔑 Docker secrets (/run/secrets/)");
    Console.WriteLine("   ☁️ Cloud provider secret stores");
    Console.WriteLine("   🛡️ HashiCorp Vault integration");
}

static async Task DemonstrarHealthChecks()
{
    // Simular health checks típicos de aplicações containerizadas
    var healthChecks = new[]
    {
        new { Name = "Database", EndPoint = "/health/db", Status = "Healthy", ResponseTime = "45ms" },
        new { Name = "Redis Cache", EndPoint = "/health/cache", Status = "Healthy", ResponseTime = "12ms" },
        new { Name = "External API", EndPoint = "/health/external", Status = "Degraded", ResponseTime = "1250ms" },
        new { Name = "Disk Space", EndPoint = "/health/disk", Status = "Healthy", ResponseTime = "5ms" },
        new { Name = "Memory Usage", EndPoint = "/health/memory", Status = "Healthy", ResponseTime = "3ms" }
    };
    
    Console.WriteLine("💚 Health Check Results:");
    
    foreach (var check in healthChecks)
    {
        await Task.Delay(50); // Simular latência do check
        
        var statusIcon = check.Status switch
        {
            "Healthy" => "✅",
            "Degraded" => "⚠️",
            "Unhealthy" => "❌",
            _ => "❓"
        };
        
        Console.WriteLine($"   {statusIcon} {check.Name}: {check.Status} ({check.ResponseTime})");
    }
    
    var overallStatus = healthChecks.Any(h => h.Status == "Unhealthy") ? "Unhealthy" :
                       healthChecks.Any(h => h.Status == "Degraded") ? "Degraded" : "Healthy";
    
    Console.WriteLine($"\n🏥 Overall Health: {overallStatus}");
    
    // Demonstrar endpoints padrão
    Console.WriteLine("\n🔗 Health Check Endpoints:");
    Console.WriteLine("   GET /health - Liveness probe");
    Console.WriteLine("   GET /health/ready - Readiness probe");
    Console.WriteLine("   GET /health/startup - Startup probe");
    Console.WriteLine("   GET /metrics - Prometheus metrics");
    
    // Kubernetes probes explanation
    Console.WriteLine("\n⚓ Kubernetes Probes:");
    Console.WriteLine("   🔄 Liveness: Restart container se falhar");
    Console.WriteLine("   🚦 Readiness: Remove do load balancer se falhar");
    Console.WriteLine("   🚀 Startup: Aguarda inicialização completa");
}

static async Task DemonstrarGracefulShutdown()
{
    // Simular graceful shutdown pattern
    using var cts = new CancellationTokenSource();
    
    Console.WriteLine("🛑 Configurando Graceful Shutdown...");
    
    // Simular shutdown sequence
    var shutdownTasks = new[]
    {
        "Parando de aceitar novas conexões",
        "Aguardando requests ativas terminarem",
        "Fechando conexões de database", 
        "Limpando cache em memória",
        "Salvando estado da aplicação",
        "Liberando recursos"
    };
    
    Console.WriteLine("📋 Sequência de shutdown:");
    
    foreach (var task in shutdownTasks)
    {
        Console.WriteLine($"   ⏳ {task}...");
        await Task.Delay(100, cts.Token); // Simular tempo de processamento
        Console.WriteLine($"   ✅ {task} - Concluído");
    }
    
    Console.WriteLine("\n🕐 Timeouts configurados:");
    Console.WriteLine("   ⏰ Graceful timeout: 30s");
    Console.WriteLine("   🚨 Force kill timeout: 45s");
    Console.WriteLine("   🔄 Health check timeout: 5s");
    
    Console.WriteLine("\n📡 Signals tratados:");
    Console.WriteLine("   SIGTERM - Shutdown graceful");
    Console.WriteLine("   SIGINT - Interrupt (Ctrl+C)");
    Console.WriteLine("   SIGKILL - Force termination");
    
    Console.WriteLine("\n💡 Best Practices:");
    Console.WriteLine("   🎯 Sempre implementar CancellationToken");
    Console.WriteLine("   ⏱️ Configurar timeouts apropriados");
    Console.WriteLine("   📊 Log todas as etapas do shutdown");
    Console.WriteLine("   🔄 Testar shutdown em diferentes cenários");
}

static void MonitorarRecursos()
{
    // Simular monitoramento de recursos
    using var process = System.Diagnostics.Process.GetCurrentProcess();
    
    Console.WriteLine("📊 Resource Monitoring:");
    
    // Memory usage
    var workingSet = process.WorkingSet64 / 1024 / 1024;
    var privateMemory = process.PrivateMemorySize64 / 1024 / 1024;
    var virtualMemory = process.VirtualMemorySize64 / 1024 / 1024;
    
    Console.WriteLine($"   🧠 Working Set: {workingSet} MB");
    Console.WriteLine($"   🔒 Private Memory: {privateMemory} MB");
    Console.WriteLine($"   💾 Virtual Memory: {virtualMemory} MB");
    
    // CPU usage
    Console.WriteLine($"   ⚡ CPU Time: {process.TotalProcessorTime.TotalMilliseconds:F0}ms");
    Console.WriteLine($"   🔢 Thread Count: {process.Threads.Count}");
    
    // GC Information
    Console.WriteLine($"   🗑️ GC Gen 0: {GC.CollectionCount(0)} collections");
    Console.WriteLine($"   🗑️ GC Gen 1: {GC.CollectionCount(1)} collections");
    Console.WriteLine($"   🗑️ GC Gen 2: {GC.CollectionCount(2)} collections");
    Console.WriteLine($"   💾 GC Memory: {GC.GetTotalMemory(false) / 1024 / 1024} MB");
    
    // Container limits (simulado)
    Console.WriteLine("\n🐳 Container Limits:");
    Console.WriteLine("   📏 Memory Limit: 512 MB");
    Console.WriteLine("   ⚡ CPU Limit: 0.5 cores");
    Console.WriteLine("   💾 Storage Limit: 1 GB");
    
    // Metrics que devem ser expostos
    Console.WriteLine("\n📈 Metrics to Export:");
    Console.WriteLine("   📊 Request count & latency");
    Console.WriteLine("   🧠 Memory usage & GC stats");
    Console.WriteLine("   ⚡ CPU utilization");
    Console.WriteLine("   💾 Disk usage");
    Console.WriteLine("   🔗 Active connections");
    Console.WriteLine("   ❌ Error rates");
}

static void DemonstrarContainerLogging()
{
    // Configurar logging para containers
    var serviceCollection = new ServiceCollection();
    serviceCollection.AddLogging(builder =>
    {
        builder.AddConsole(options =>
        {
            options.FormatterName = "json"; // JSON para containers
        });
    });
    
    var serviceProvider = serviceCollection.BuildServiceProvider();
    var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
    
    Console.WriteLine("📝 Container Logging Best Practices:");
    
    // Structured logging examples
    var requestId = Guid.NewGuid().ToString("N")[..8];
    var userId = "user_12345";
    
    Console.WriteLine("\n📋 Structured Logging Examples:");
    Console.WriteLine($"   📊 Request started: {{\"RequestId\":\"{requestId}\", \"UserId\":\"{userId}\", \"Endpoint\":\"/api/users\"}}");
    Console.WriteLine($"   ⏱️ Request completed: {{\"RequestId\":\"{requestId}\", \"Duration\":\"245ms\", \"StatusCode\":200}}");
    Console.WriteLine($"   ❌ Error occurred: {{\"RequestId\":\"{requestId}\", \"Error\":\"DatabaseTimeout\", \"Details\":\"Connection timeout after 30s\"}}");
    
    Console.WriteLine("\n🔧 Log Configuration:");
    Console.WriteLine("   📄 Format: JSON (máquina legível)");
    Console.WriteLine("   📊 Level: Information (produção)");
    Console.WriteLine("   📍 Output: stdout/stderr");
    Console.WriteLine("   🔗 Correlation: Request ID em todos os logs");
    
    Console.WriteLine("\n📈 Log Aggregation:");
    Console.WriteLine("   🔍 ELK Stack (Elasticsearch, Logstash, Kibana)");
    Console.WriteLine("   📊 Fluentd + Prometheus + Grafana");
    Console.WriteLine("   ☁️ Cloud native: AWS CloudWatch, Azure Monitor");
    Console.WriteLine("   🔗 Distributed tracing: Jaeger, Zipkin");
    
    Console.WriteLine("\n⚠️ Security Considerations:");
    Console.WriteLine("   🔐 Nunca logar senhas ou tokens");
    Console.WriteLine("   🏷️ Mascarar PII (dados pessoais)");
    Console.WriteLine("   📊 Rate limiting em logs verbosos");
    Console.WriteLine("   🔒 Criptografar logs sensíveis");
}

static async Task SimularServiceDiscovery()
{
    // Simular service discovery em ambiente containerizado
    var services = new[]
    {
        new { Name = "user-service", Host = "user-service.default.svc.cluster.local", Port = 8080, Status = "Healthy" },
        new { Name = "order-service", Host = "order-service.default.svc.cluster.local", Port = 8081, Status = "Healthy" },
        new { Name = "payment-service", Host = "payment-service.default.svc.cluster.local", Port = 8082, Status = "Degraded" },
        new { Name = "notification-service", Host = "notification-service.default.svc.cluster.local", Port = 8083, Status = "Healthy" }
    };
    
    Console.WriteLine("🔍 Service Discovery Results:");
    
    foreach (var service in services)
    {
        await Task.Delay(25); // Simular latência de discovery
        
        var statusIcon = service.Status == "Healthy" ? "✅" : "⚠️";
        Console.WriteLine($"   {statusIcon} {service.Name}: {service.Host}:{service.Port} ({service.Status})");
    }
    
    Console.WriteLine("\n🔧 Service Discovery Mechanisms:");
    Console.WriteLine("   ⚓ Kubernetes DNS (service.namespace.svc.cluster.local)");
    Console.WriteLine("   🔗 Service Mesh (Istio, Linkerd)");
    Console.WriteLine("   📡 Consul, etcd");
    Console.WriteLine("   ☁️ Cloud Load Balancers");
    
    Console.WriteLine("\n🌐 Load Balancing Strategies:");
    Console.WriteLine("   🔄 Round Robin");
    Console.WriteLine("   📊 Least Connections");
    Console.WriteLine("   ⚡ Response Time Based");
    Console.WriteLine("   📍 Geographic Proximity");
    
    Console.WriteLine("\n🛡️ Circuit Breaker Pattern:");
    Console.WriteLine("   🔴 Closed: Normal operation");
    Console.WriteLine("   🟡 Open: Failing fast");
    Console.WriteLine("   🟢 Half-Open: Testing recovery");
    
    Console.WriteLine("\n🔄 Retry Strategies:");
    Console.WriteLine("   ⏱️ Exponential backoff");
    Console.WriteLine("   🎲 Jitter to prevent thundering herd");
    Console.WriteLine("   📊 Max retry limits");
    Console.WriteLine("   🚨 Dead letter queues");
}

static void DemonstrarCloudNativePatterns()
{
    Console.WriteLine("☁️ Cloud Native Patterns & Practices:");
    
    Console.WriteLine("\n🏗️ 12-Factor App Principles:");
    var principles = new[]
    {
        "Codebase: One codebase, many deploys",
        "Dependencies: Explicitly declare dependencies",
        "Config: Store config in environment",
        "Backing Services: Treat as attached resources",
        "Build/Release/Run: Separate stages",
        "Processes: Execute as stateless processes",
        "Port Binding: Export services via port binding",
        "Concurrency: Scale out via process model",
        "Disposability: Fast startup & graceful shutdown",
        "Dev/Prod Parity: Keep environments similar",
        "Logs: Treat logs as event streams",
        "Admin Processes: Run as one-off processes"
    };
    
    for (int i = 0; i < principles.Length; i++)
    {
        Console.WriteLine($"   {i + 1,2}. {principles[i]}");
    }
    
    Console.WriteLine("\n🔄 Microservices Patterns:");
    Console.WriteLine("   🎯 Single Responsibility");
    Console.WriteLine("   🔗 API Gateway");
    Console.WriteLine("   📊 Distributed Data Management");
    Console.WriteLine("   🔍 Service Discovery");
    Console.WriteLine("   🛡️ Circuit Breaker");
    Console.WriteLine("   📈 Bulkhead Isolation");
    Console.WriteLine("   🔄 Event Sourcing");
    Console.WriteLine("   📡 CQRS (Command Query Responsibility Segregation)");
    
    Console.WriteLine("\n🐳 Container Best Practices:");
    Console.WriteLine("   📦 Minimal base images (Alpine, Distroless)");
    Console.WriteLine("   👤 Non-root user");
    Console.WriteLine("   🔒 Read-only filesystem");
    Console.WriteLine("   📊 Multi-stage builds");
    Console.WriteLine("   🏷️ Proper image tagging");
    Console.WriteLine("   🔍 Vulnerability scanning");
    Console.WriteLine("   📏 Resource limits & requests");
    Console.WriteLine("   💚 Health checks");
    
    Console.WriteLine("\n⚓ Kubernetes Deployment Strategies:");
    Console.WriteLine("   🔄 Rolling Updates");
    Console.WriteLine("   🔵 Blue-Green Deployment");
    Console.WriteLine("   🕵️ Canary Deployment");
    Console.WriteLine("   🎯 A/B Testing");
    Console.WriteLine("   🔙 Feature Flags");
    
    Console.WriteLine("\n📊 Observability (3 Pillars):");
    Console.WriteLine("   📝 Logs: Discrete events");
    Console.WriteLine("   📈 Metrics: Aggregated measurements");
    Console.WriteLine("   🔍 Traces: Request journey");
    
    Console.WriteLine("\n🛡️ Security Considerations:");
    Console.WriteLine("   🔐 Secrets management");
    Console.WriteLine("   🛡️ Network policies");
    Console.WriteLine("   👤 RBAC (Role-Based Access Control)");
    Console.WriteLine("   🔍 Pod Security Standards");
    Console.WriteLine("   📡 Service Mesh security");
    Console.WriteLine("   🔒 Image signing & verification");
}
