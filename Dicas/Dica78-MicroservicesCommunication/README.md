# 🚀 Dica 78: Microservices Communication Patterns

## 📋 Visão Geral

Esta dica demonstra padrões essenciais de comunicação entre microserviços usando .NET 9.0, focando em resilience, escalabilidade e observabilidade.

## 🎯 Padrões Demonstrados

### 1. 🌐 HTTP Communication Patterns
- **Resilience com Polly**: Retry, Circuit Breaker, Timeout
- **HTTP Client Factory**: Configuração centralizada e reutilização
- **Parallel Execution**: Chamadas simultâneas com controle de concorrência
- **Request/Response**: Patterns síncronos com timeout

### 2. 📨 Message Queue Patterns
- **Publish/Subscribe**: Comunicação assíncrona entre serviços
- **Request/Reply**: Pattern síncrono sobre infraestrutura assíncrona
- **Event-Driven Architecture**: Workflows orientados a eventos
- **Message Broker**: Implementação in-memory para demonstração

### 3. 📝 Event Sourcing Patterns
- **Event Store**: Armazenamento de eventos como fonte da verdade
- **State Reconstruction**: Reconstrução de estado a partir de eventos
- **Projections**: Views materializadas para consultas
- **Event Replay**: Reprodução de eventos para debugging/análise

### 4. ⚡ Circuit Breaker Patterns
- **Failure Detection**: Detecção automática de falhas
- **Fast Fail**: Falha rápida quando serviço está indisponível
- **Recovery Testing**: Teste automático de recuperação
- **Multiple Policies**: Circuit breakers específicos por tipo de serviço

### 5. 🔍 Service Discovery Patterns
- **Service Registry**: Registro e descoberta de serviços
- **Load Balancing**: Distribuição de carga entre instâncias
- **Regional Routing**: Roteamento baseado em localização
- **Health Monitoring**: Monitoramento contínuo de saúde dos serviços

### 6. 🏥 Health Check Patterns
- **Multi-Component Checks**: Verificação de múltiplos componentes
- **Dependency Health**: Monitoramento de dependências externas
- **Degraded States**: Identificação de estados degradados
- **Continuous Monitoring**: Monitoramento contínuo com métricas

## 🛠️ Tecnologias e Bibliotecas

- **.NET 9.0**: Framework principal
- **Polly v8.4.1**: Resilience patterns (Retry, Circuit Breaker, Timeout)
- **Microsoft.Extensions.Http.Resilience**: Integração nativa de resilience
- **Microsoft.Extensions.Hosting**: Host builder e dependency injection
- **System.Text.Json**: Serialização de dados
- **IHttpClientFactory**: Factory pattern para HTTP clients

## 🏃‍♂️ Como Executar

### Executar Demonstração Completa
```bash
dotnet run
```

### Executar Padrão Específico
```bash
# HTTP Communication
dotnet run http

# Message Queue
dotnet run queue

# Event Sourcing
dotnet run events

# Circuit Breaker
dotnet run circuit

# Service Discovery
dotnet run discovery

# Health Checks
dotnet run health
```

## 📊 Componentes Principais

### HttpCommunicationService
- Demonstra comunicação HTTP resiliente
- Patterns de retry com exponential backoff
- Execução paralela controlada
- Timeout e error handling

### MessageQueueService
- Simula message broker in-memory
- Pub/Sub pattern com múltiplos subscribers
- Request/Reply com correlation ID
- Event-driven workflows

### EventSourcingService
- Event store com múltiplos streams
- Reconstrução de estado de agregados
- Projeções para analytics
- Event replay para debugging

### CircuitBreakerDemo
- Circuit breaker com diferentes configurações
- Demonstração de estados (Closed/Open/Half-Open)
- Métricas e estatísticas
- Recovery automático

### ServiceDiscovery
- Registry de serviços com metadados
- Load balancing simples (round-robin)
- Roteamento regional
- Health checks automáticos

### HealthCheckService
- Verificações de múltiplos componentes
- Estados de saúde (Healthy/Degraded/Unhealthy)
- Monitoramento contínuo
- Métricas e alertas

## 🔧 Configurações

### Polly Resilience
```csharp
.AddStandardResilienceHandler(options =>
{
    // Retry com exponential backoff
    options.Retry.MaxRetryAttempts = 3;
    options.Retry.BackoffType = DelayBackoffType.Exponential;
    
    // Circuit breaker
    options.CircuitBreaker.FailureRatio = 0.5;
    options.CircuitBreaker.MinimumThroughput = 5;
    
    // Timeouts
    options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(10);
});
```

### Message Queue Configuration
```csharp
// Publish/Subscribe
await _messageBroker.PublishAsync("user.created", userData);
_messageBroker.Subscribe("user.created", async message => {
    // Process message
});

// Request/Reply
var correlationId = Guid.NewGuid().ToString();
await _messageBroker.PublishAsync("order.request", new {
    replyTo = $"order.reply.{correlationId}"
});
```

### Circuit Breaker Configuration
```csharp
var circuitBreaker = new ResiliencePipelineBuilder()
    .AddCircuitBreaker(new CircuitBreakerStrategyOptions
    {
        FailureRatio = 0.5,
        MinimumThroughput = 3,
        SamplingDuration = TimeSpan.FromSeconds(30),
        BreakDuration = TimeSpan.FromSeconds(10)
    })
    .Build();
```

## 📈 Métricas e Observabilidade

### Circuit Breaker Metrics
- Total de requests
- Taxa de sucesso/falha
- Número de ativações do circuit breaker
- Tempo de uptime

### Health Check Metrics
- Status geral do sistema
- Número de checks saudáveis/não saudáveis
- Tempo de resposta dos checks
- Histórico de saúde

### Service Discovery Metrics
- Número de serviços registrados
- Instâncias saudáveis por serviço
- Distribuição de carga
- Latência de descoberta

## 🚀 Casos de Uso

### Arquiteturas de Microserviços
- E-commerce com múltiplos serviços
- Sistemas distribuídos resilientes
- APIs Gateway com failover
- Event-driven architectures

### Patterns de Integração
- Comunicação síncrona resiliente
- Mensageria assíncrona
- Eventual consistency
- Saga patterns

### Observabilidade e Monitoramento
- Health checks centralizados
- Métricas de performance
- Alertas automáticos
- Dashboard de saúde do sistema

## 🎓 Conceitos Aprendidos

1. **Resilience Patterns**: Como implementar retry, circuit breaker e timeout
2. **Message Patterns**: Pub/Sub, Request/Reply e Event-Driven workflows
3. **Event Sourcing**: Store de eventos e reconstrução de estado
4. **Service Discovery**: Registro, descoberta e load balancing
5. **Health Monitoring**: Monitoramento contínuo e métricas
6. **Error Handling**: Tratamento robusto de falhas em sistemas distribuídos

## 🔗 Recursos Adicionais

- [Polly Documentation](https://www.pollydocs.org/)
- [.NET Resilience Extensions](https://docs.microsoft.com/en-us/dotnet/core/resilience/)
- [Microservices Patterns](https://microservices.io/patterns/)
- [Event Sourcing](https://martinfowler.com/eaaDev/EventSourcing.html)
- [Circuit Breaker Pattern](https://docs.microsoft.com/en-us/azure/architecture/patterns/circuit-breaker)

---

**Objetivo**: Demonstrar padrões essenciais para comunicação robusta e resiliente entre microserviços, fundamentais para sistemas distribuídos de produção.
