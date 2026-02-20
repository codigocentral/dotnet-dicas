# Dica 98: Cloud Native & Containers

## ☁️ Cloud Native e Containers com .NET 9

Esta dica demonstra como desenvolver aplicações **cloud-native** e **container-ready** usando **.NET 9**, abordando desde detecção de ambiente até padrões de 12-factor app.

## 📋 Conceitos Abordados

### 1. 🐳 Container Environment Detection

- **Environment Detection**: Identificação automática de ambiente de container
- **Runtime Analysis**: Análise do ambiente de execução
- **Adaptive Behavior**: Comportamento adaptativo baseado no ambiente
- **Resource Awareness**: Consciência de recursos disponíveis

### 2. ⚙️ Configuration Management

- **Environment Variables**: Configuração via variáveis de ambiente
- **Configuration Providers**: Múltiplas fontes de configuração
- **Hierarchical Config**: Configuração hierárquica e override
- **Secret Management**: Gerenciamento seguro de segredos

### 3. 🏥 Health Checks

- **Application Health**: Verificação de saúde da aplicação
- **Dependency Checks**: Verificação de dependências externas
- **Circuit Breaker**: Padrão para falhas em cascata
- **Monitoring Integration**: Integração com sistemas de monitoramento

### 4. 🛑 Graceful Shutdown

- **Signal Handling**: Tratamento de sinais do sistema
- **Resource Cleanup**: Limpeza adequada de recursos
- **Connection Draining**: Drenagem de conexões ativas
- **State Persistence**: Persistência de estado crítico

### 5. 📊 Resource Monitoring

- **Memory Usage**: Monitoramento de uso de memória
- **CPU Metrics**: Métricas de CPU e processamento
- **GC Analysis**: Análise do Garbage Collector
- **Performance Counters**: Contadores de performance

### 6. 📝 Structured Logging

- **JSON Logging**: Logs estruturados em JSON
- **Correlation IDs**: Rastreamento de requisições
- **Log Levels**: Níveis apropriados de log
- **Centralized Logging**: Integração com sistemas centralizados

### 7. 🔍 Service Discovery

- **Service Registry**: Registro de serviços
- **Load Balancing**: Balanceamento de carga
- **Circuit Breaker**: Proteção contra falhas
- **Retry Policies**: Políticas de retry inteligentes

### 8. 📋 12-Factor App Principles

- **Codebase**: Base de código única
- **Dependencies**: Declaração explícita de dependências
- **Config**: Configuração em ambiente
- **Backing Services**: Serviços como recursos anexados
- **Build/Release/Run**: Separação de estágios
- **Processes**: Execução como processos stateless
- **Port Binding**: Vinculação de porta
- **Concurrency**: Scale out via modelo de processo
- **Disposability**: Inicialização/shutdown rápidos
- **Dev/Prod Parity**: Paridade entre ambientes
- **Logs**: Tratamento de logs como streams
- **Admin Processes**: Processos administrativos

## 🚀 Funcionalidades Demonstradas

### Container Detection
```csharp
var isInContainer = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";
var hasDockerEnv = File.Exists("/.dockerenv");
```

### Health Checks
```csharp
var healthChecks = new List<(string Name, bool IsHealthy, TimeSpan ResponseTime)>();
var overallStatus = healthChecks.All(h => h.IsHealthy) ? "Healthy" : "Degraded";
```

### Graceful Shutdown
```csharp
var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) => {
    e.Cancel = true;
    cts.Cancel();
};
```

### Resource Monitoring
```csharp
var process = Process.GetCurrentProcess();
var workingSet = process.WorkingSet64;
var gcMemory = GC.GetTotalMemory(false);
```

## 🔧 Tecnologias Utilizadas

- **.NET 9**: Framework com suporte cloud-native
- **Microsoft.Extensions.Hosting**: Host genérico para aplicações
- **Microsoft.Extensions.Logging**: Sistema de logging estruturado
- **Microsoft.Extensions.Configuration**: Sistema de configuração flexível

## 📦 Pacotes NuGet

```xml
<PackageReference Include="Microsoft.Extensions.Hosting" Version="9.0.7" />
<PackageReference Include="Microsoft.Extensions.Logging" Version="9.0.7" />
<PackageReference Include="Microsoft.Extensions.Configuration" Version="9.0.7" />
```

## ⚡ Principais Vantagens

### ☁️ **Cloud Ready**

- Configuração via ambiente
- Logs estruturados
- Health checks integrados
- Shutdown gracioso

### 🐳 **Container Optimized**

- Detecção automática de container
- Resource awareness
- Signal handling apropriado
- Minimal footprint

### 📊 **Observable**

- Métricas de sistema
- Logs correlacionados
- Health endpoints
- Performance monitoring

### 🔄 **Scalable**

- Stateless design
- Service discovery
- Load balancing ready
- Horizontal scaling

## 🌟 Padrões Cloud-Native

### 🏗️ **Infrastructure as Code**

- Definição declarativa
- Version control
- Automated deployment
- Consistent environments

### 🔄 **CI/CD Integration**

- Automated builds
- Container registries
- Blue-green deployment
- Canary releases

### 🛡️ **Security Best Practices**

- Least privilege principle
- Secret management
- Network policies
- Runtime security

### 📈 **Observability**

- Distributed tracing
- Metrics collection
- Log aggregation
- APM integration

## 📋 12-Factor App Checklist

1. **✅ Codebase**: One codebase, many deploys
2. **✅ Dependencies**: Explicitly declare dependencies
3. **✅ Config**: Store config in environment
4. **✅ Backing Services**: Treat as attached resources
5. **✅ Build/Release/Run**: Strict separation
6. **✅ Processes**: Execute as stateless processes
7. **✅ Port Binding**: Export services via port binding
8. **✅ Concurrency**: Scale out via process model
9. **✅ Disposability**: Fast startup/graceful shutdown
10. **✅ Dev/Prod Parity**: Keep environments similar
11. **✅ Logs**: Treat logs as event streams
12. **✅ Admin Processes**: Run as one-off processes

## 🔮 Considerações Avançadas

- **Kubernetes Integration**: Native K8s support
- **Service Mesh**: Istio, Linkerd integration
- **Event-Driven Architecture**: Event sourcing, CQRS
- **Microservices Patterns**: Circuit breaker, saga, etc.
- **GitOps**: Git-based deployment workflows
- **Multi-Cloud**: Cloud-agnostic deployment
- **Edge Computing**: Edge-optimized deployments
- **Serverless**: Function-as-a-Service integration

---

💡 **Dica Pro**: Cloud-native não é apenas sobre containers. É uma filosofia completa que engloba cultura, práticas e tecnologias para construir aplicações resilientes, escaláveis e observáveis na nuvem.
